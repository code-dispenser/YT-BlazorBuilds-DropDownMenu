namespace BlazorBuilds.Components.DropDownMenu;

internal interface IFocusableChild
{
    string        DisplayText  { get;}
    DropDownMenu? ParentMenu    { get; }
    FocusItemType FocusItemType { get; }
    
    ValueTask SetFocus();
}
