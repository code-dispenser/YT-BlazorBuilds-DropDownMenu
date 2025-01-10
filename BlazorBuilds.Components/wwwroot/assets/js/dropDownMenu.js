const _handlerMap = new WeakMap();
const raiseLostFocus = (blazorCallBackRef, callBackName) => blazorCallBackRef.invokeMethodAsync(callBackName);
function registerLostMenuFocus(blazorCallBackRef, callBackName, element) {
    const handler = (event) => {
        if (!element.contains(event.relatedTarget)) {
            raiseLostFocus(blazorCallBackRef, callBackName);
        }
    };
    element.addEventListener("focusout", handler);
    _handlerMap.set(element, handler);
}
function unRegisterLostMenuFocus(element) {
    const handler = _handlerMap.get(element);
    if (handler) {
        element.removeEventListener("focusout", handler);
        _handlerMap.delete(element);
    }
}
function checkMenuBoundaries(classSelelctor, classToAdd) {
    var menusToCheck = Array.from(document.querySelectorAll(classSelelctor));
    if (!menusToCheck)
        return; // does a null / undefined check.
    const viewportWidth = window.innerWidth;
    const viewportHeight = window.innerHeight;
    menusToCheck.forEach(menu => {
        const rect = menu.getBoundingClientRect();
        if (rect.right > viewportWidth)
            menu.classList.toggle(classToAdd);
    });
}
export { registerLostMenuFocus, unRegisterLostMenuFocus, checkMenuBoundaries };
//# sourceMappingURL=dropDownMenu.js.map