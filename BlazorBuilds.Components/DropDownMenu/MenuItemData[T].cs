namespace BlazorBuilds.Components.DropDownMenu;

public  class MenuItemData<TData>(string menuName, TData? payload, MenuItemType menuItemType, string path = "", Target target = Target.Self)
{
    private readonly TData? _payload = payload;

    public MenuItemType MenuItemType { get; } = menuItemType;
    public string? Path              { get; } = String.IsNullOrWhiteSpace(path) ? String.Empty : path.Trim();
    public string MenuName           { get; } = String.IsNullOrWhiteSpace(menuName) ? String.Empty : menuName.Trim();
    public Target Target             { get; } = target;

    public bool TryGetData(out TData? menuItemData)
    {
        menuItemData = _payload ?? default(TData); 
        return _payload is not null;
    }
}
