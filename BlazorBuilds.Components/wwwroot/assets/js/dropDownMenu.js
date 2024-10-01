const raiseLostFocus = (blazorCallBackRef, callBackName) => blazorCallBackRef.invokeMethodAsync(callBackName);
function registerLostMenuFocus(blazorCallBackRef, callBackName, element) {
    const handler = (event) => {
        if (!element.contains(event.relatedTarget)) {
            raiseLostFocus(blazorCallBackRef, callBackName);
        }
    };
    element.addEventListener("focusout", handler);
}
function unRegisterLostMenuFocus(blazorCallBackRef, callBackName, element) {
    const handler = (event) => {
        if (!element.contains(event.relatedTarget)) {
            raiseLostFocus(blazorCallBackRef, callBackName);
        }
    };
    element === null || element === void 0 ? void 0 : element.removeEventListener("focusout", handler);
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