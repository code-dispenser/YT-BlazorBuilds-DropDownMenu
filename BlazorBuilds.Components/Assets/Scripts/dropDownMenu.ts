const _handlerMap = new WeakMap<HTMLElement, EventListener>();

const raiseLostFocus = (blazorCallBackRef: any, callBackName: string) => blazorCallBackRef.invokeMethodAsync(callBackName);

function registerLostMenuFocus(blazorCallBackRef: any, callBackName: string, element: HTMLElement): void {

    const handler = (event: FocusEvent) => {

        if (!element.contains(event.relatedTarget as Node)) {
            raiseLostFocus(blazorCallBackRef, callBackName);
        }
    };

    element.addEventListener("focusout", handler);
    _handlerMap.set(element, handler as EventListener);
}

function unRegisterLostMenuFocus(element: HTMLElement): void {

    if (!element) return;

    const handler = _handlerMap.get(element);

    if (handler) {
        element.removeEventListener("focusout", handler);
        _handlerMap.delete(element);
    }
}


function checkMenuBoundaries(classSelelctor: string, classToAdd: string) {

    var menusToCheck = Array.from(document.querySelectorAll(classSelelctor)) as HTMLElement[];

    if (!menusToCheck) return; // does a null / undefined check.

    const viewportWidth  = window.innerWidth;
    const viewportHeight = window.innerHeight;

    menusToCheck.forEach(menu => {

        const rect = menu.getBoundingClientRect();

        if (rect.right > viewportWidth) menu.classList.toggle(classToAdd);
    });

}
export { registerLostMenuFocus, unRegisterLostMenuFocus, checkMenuBoundaries };
