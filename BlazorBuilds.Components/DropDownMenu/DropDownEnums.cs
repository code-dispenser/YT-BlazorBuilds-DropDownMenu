namespace BlazorBuilds.Components.DropDownMenu;

internal enum StyleInfo     : int { DropDown, DropDownMenu, DropDownMenuItem, DropDownMenuToggler, DropDownMenuIcon, DropDownMenuIconClr, DropDownMenuText }
internal enum Direction     : int { Up, Down }
public   enum MenuItemType  : int { Action, Url }
public   enum FocusItemType : int { Menu, MenuItem }
public   enum Target        : int { Self, Blank }