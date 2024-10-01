using BlazorBuilds.Components.Common.Extensions;
using BlazorBuilds.Components.Common.Seeds;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace BlazorBuilds.Components.DropDownMenu;

public partial class DropDownMenu : IFocusableChild, IAsyncDisposable
{
    [Inject] IJSRuntime JsRuntime { get; set; } = default!;

    [CascadingParameter] public DropDownMenu?   ParentMenu   { get; set; } = default!;
    [Parameter]          public RenderFragment? ChildContent { get; set; } = default;
    [Parameter]          public string          DisplayText  { get; set; } = default!;

    private List<DropDownMenu>    ChildMenus      { get; } = [];
    private List<IFocusableChild> FocusableItems  { get; } = [];
    public  FocusItemType         FocusItemType   { get; } = FocusItemType.Menu;
    private ElementReference?     DropDownMenuRef { get; set; }
    private ElementReference?     MainButtonRef   { get; set; }
    private ElementReference      ElementRef      { get; set; }
    private string                MenuID          { get; } = $"menu-id-{Guid.NewGuid()}";
    private bool                  IsRootMenu      { get; set; } = false;
    private bool                  IsActive        { get; set; } = false;
    private int                   FocusItemIndex  { get; set; } = 0;

    private DotNetObjectReference<DropDownMenu>? _dropDownMenuObjectRef;
    private IJSObjectReference?                  _jsModule;

    protected override void OnInitialized()
    {
        if (ParentMenu is null) IsRootMenu = true;
        ParentMenu?.AddMenu(this);
    }

    internal void AddMenu(DropDownMenu menu)
    {
        if (false == ChildMenus.Contains(menu))
        {
            ChildMenus.Add(menu);
            FocusableItems.Add(menu);
        }
    }
    internal void AddMenuItem(IFocusableChild menuItem)

        => FocusableItems.Contains(menuItem).WhenFalse(() => FocusableItems.Add(menuItem));

    protected override void OnParametersSet()
    {
        if (true == String.IsNullOrWhiteSpace(DisplayText)) throw new ArgumentNullException(nameof(DisplayText), GlobalStrings.Menu_Name_Exception_Message);
        base.OnParametersSet();
    }

    private async ValueTask RegisterJSCallBack()
    {
        _dropDownMenuObjectRef = DotNetObjectReference.Create(this);
        _jsModule        = await this.JsRuntime.InvokeAsync<IJSObjectReference>("import", GlobalStrings.JavaScript_File_Path);

        await _jsModule.InvokeVoidAsync(GlobalStrings.JavaScript_Register_Func, _dropDownMenuObjectRef, nameof(FocusLeftControl), DropDownMenuRef);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)

        => await (true == firstRender && true == IsRootMenu).WhenTrue(() => RegisterJSCallBack());

    private void SetMenuActiveState(bool isActive, bool raiseStateHasChanged = false)
    {
        IsActive = isActive;
        raiseStateHasChanged.WhenTrue(() => StateHasChanged());
    }

    private static async Task AddDelayForScreenReader(int milliseconds)

        => await Task.Delay(milliseconds);//Needed in order to get NVDA announce that the menu/submen is expanded and collapsed.

    private void CloseAllMenusFor(DropDownMenu menu)
    {
        foreach (var childMenu in menu.ChildMenus)
        {
            childMenu.SetMenuActiveState(false, true);
            CloseAllMenusFor(childMenu);
        }
        menu.SetMenuActiveState(false, true);
    }

    private DropDownMenu GetRootMenu(DropDownMenu currentMenu)
    {
        while (false == currentMenu.IsRootMenu) currentMenu = currentMenu.ParentMenu!;

        return currentMenu;
    }

    private async Task CheckMenuBoundaries()
    {
        var rootMenu = GetRootMenu(this);
        await rootMenu._jsModule!.InvokeVoidAsync(GlobalStrings.JavaScript_CheckBounds_Func, $".{GlobalStrings.DropDown_Menu_Active_Class}", GlobalStrings.DropDown_Menu_Active_Class_Move);
    }

    private async Task ToggleDropDownMenu()
    {
        ChildMenus.ForEach(menu => CloseAllMenusFor(menu));//close all children if open
        SetMenuActiveState(this.IsActive = !IsActive);

        var parentMenu = this.IsRootMenu ? this : this.ParentMenu;
        var otherMenus = parentMenu!.ChildMenus.Where(menu => menu != this).ToList();

        otherMenus.ForEach(menu => menu.SetMenuActiveState(false, true)); //Close other menus at this level i.e user has another open an then clicks this one

        await AddDelayForScreenReader(50);

        if (IsActive == true)
        {
            await CheckMenuBoundaries();
            await (FocusableItems.Count > 0).WhenTrue(() => FocusableItems[0].SetFocus());
        }
    }

    private async Task ToggleButtonMenu()
    {
        await ToggleDropDownMenu();
        FocusItemIndex = 0;

        if (true == this.IsRootMenu && this.IsActive)//set focus to first item in list on opening main menu
        {
            var focusItem = FocusableItems.FirstOrDefault();
            await (focusItem != null).WhenTrue(() => focusItem!.SetFocus());
        }
    }

    private async Task HandleShiftTab()
    {
        var rootMenu = GetRootMenu(this);
        CloseAllMenusFor(rootMenu);
        rootMenu.SetMenuActiveState(false);
        await AddDelayForScreenReader(50);
        await rootMenu.MainButtonRef!.Value.FocusAsync();
    }

    private async Task HandleDownArrowOpen(KeyboardEventArgs keyArgs)
    {
        if (this.IsRootMenu && this.IsActive == false && keyArgs.Key == GlobalStrings.KeyBoard_Down_Arrow_Key) await ToggleButtonMenu();
    }

    private async Task<int> HandleSearchChars(List<IFocusableChild> focusableItems, int currentIndex, string lowerCaseChar)
    {
        if (false == focusableItems.Any(a => a.DisplayText.ToLower().StartsWith(lowerCaseChar))) return currentIndex;

        int searchIndex = focusableItems.FindIndex(currentIndex + 1, a => a.DisplayText.ToLower().StartsWith(lowerCaseChar));

        if (searchIndex == -1) searchIndex = focusableItems.FindIndex(0, a => a.DisplayText.ToLower().StartsWith(lowerCaseChar));

        searchIndex = searchIndex == -1 ? currentIndex : searchIndex;

        await FocusableItems[searchIndex].SetFocus();

        return searchIndex;
    }
    private async Task<int> HandleHomeEndKeys(List<IFocusableChild> focusableItems, int focusIndex, bool isHome)
    {
        focusIndex = isHome ? 0 : focusableItems.Count -1;

        await FocusableItems[focusIndex].SetFocus();

        return focusIndex;
    }

    private async Task<int> HandleLeftArrowKey(List<IFocusableChild> focusableItems, IFocusableChild focusableItem, int focusIndex)
    {
        await IsRootMenu.WhenTrueElse(() => this.MainButtonRef!.Value.FocusAsync(), () => focusableItem.ParentMenu!.SetFocus());

        SetMenuActiveState(false);

        await AddDelayForScreenReader(50);

        return 0;
    }

    private static async Task<int> HandleRightArrowKey(IFocusableChild focusableItem, int focusIndex)
    {
        if (focusableItem is DropDownMenu menu)//check if has children
        {
            if (menu.FocusableItems.Count > 0)//go into sub menu and set focus to first item
            {
                menu.IsActive = true;
                await AddDelayForScreenReader(50);
                menu.FocusItemIndex = 0;
                await menu.SetFocus();
                await menu.FocusableItems[0].SetFocus();
            }

        }
        return focusIndex;
    }
        
    private async Task HandleEscapeKey(IFocusableChild focusableItem)
    {
        this.IsActive = false;

        if (focusableItem.FocusItemType == FocusItemType.MenuItem) await focusableItem.ParentMenu!.SetFocus();
        if (focusableItem is DropDownMenu menu && menu.ParentMenu is not null) await menu.ParentMenu.SetFocus();
        if (true == this.IsRootMenu) await MainButtonRef!.Value.FocusAsync();
    }

    private static async Task<int> CycleMenuItemSetFocus(List<IFocusableChild> focusItems, int currentIndex, Direction direction)
    {
        int maxIndex = focusItems.Count -1;

        var newIndex = direction switch
        {
            Direction.Down  when currentIndex < maxIndex    => currentIndex + 1,
            Direction.Down  when currentIndex >= maxIndex   => 0,
            Direction.Up    when currentIndex > 0           => currentIndex - 1,
            Direction.Up    when currentIndex <= 0          => maxIndex,
            _ => currentIndex
        };

        await focusItems[newIndex].SetFocus();
        
        return newIndex;
    }

    private async Task HandleKeyPress(KeyboardEventArgs keyArgs)
    {

        FocusItemIndex = keyArgs.Key switch
        {
            GlobalStrings.KeyBoard_Down_Arrow_Key   => await CycleMenuItemSetFocus(FocusableItems, FocusItemIndex, Direction.Down),
            GlobalStrings.KeyBoard_Up_Arrow_Key     => await CycleMenuItemSetFocus(FocusableItems, FocusItemIndex, Direction.Up),
            GlobalStrings.KeyBoard_Right_Arrow_Key  => await HandleRightArrowKey(FocusableItems[FocusItemIndex], FocusItemIndex),
            GlobalStrings.KeyBoard_Left_Arrow_Key   => await HandleLeftArrowKey(FocusableItems, FocusableItems[FocusItemIndex], FocusItemIndex),
            GlobalStrings.KeyBoard_Home_Key         => await HandleHomeEndKeys(FocusableItems, FocusItemIndex, true),
            GlobalStrings.KeyBoard_End_Key          => await HandleHomeEndKeys(FocusableItems, FocusItemIndex, false),
            _ => FocusItemIndex
        };

        await (keyArgs.Key == GlobalStrings.KeyBoard_Escape_Key).WhenTrue(() => HandleEscapeKey(FocusableItems[FocusItemIndex]));

        await (keyArgs.ShiftKey && keyArgs.Key == "Tab").WhenTrue(HandleShiftTab);

        if (keyArgs.Key.Length == 1)
        {
            char lowerCaseChar = keyArgs.Key.ToLower()[0];

            if (char.IsLetterOrDigit(lowerCaseChar)) FocusItemIndex = await HandleSearchChars(FocusableItems, FocusItemIndex, lowerCaseChar.ToString());

            return;
        }
    }

    internal async Task MenuItemClicked()
    {
        var rootMenu = GetRootMenu(this);
        CloseAllMenusFor(rootMenu);
        rootMenu.SetMenuActiveState(false, true);
        await (rootMenu.MainButtonRef.HasValue).WhenTrue(() => rootMenu.MainButtonRef!.Value.FocusAsync());
    }

    private string GetStyleInfo(StyleInfo styleInfo)

    => styleInfo switch
    {
        StyleInfo.DropDownMenuToggler => GlobalStrings.DropDown_Menu_Toggler_Class,
        StyleInfo.DropDownMenu => IsActive ? $"{GlobalStrings.DropDown_Menu_Class} {GlobalStrings.DropDown_Menu_Active_Class}" : GlobalStrings.DropDown_Menu_Class,
        _ => String.Empty
    };

    public async ValueTask SetFocus()

        => await ElementRef.FocusAsync();
    

    [JSInvokable]
    public async Task FocusLeftControl()
    {
        this.IsActive = false;//this will always be the root
        await InvokeAsync(StateHasChanged);
    }

    public async ValueTask DisposeAsync()
    {
        if (true == this.IsRootMenu && _jsModule is not null)
        {
            await _jsModule!.InvokeVoidAsync(GlobalStrings.JavaScript_UnRegister_Func, _dropDownMenuObjectRef, nameof(FocusLeftControl), DropDownMenuRef);
            await _jsModule.DisposeAsync();
            _dropDownMenuObjectRef?.Dispose();
        }
    }
}
