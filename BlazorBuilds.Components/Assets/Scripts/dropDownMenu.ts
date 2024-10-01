const raiseLostFocus = (blazorCallBackRef: any, callBackName: string) => blazorCallBackRef.invokeMethodAsync(callBackName);

function registerLostMenuFocus(blazorCallBackRef: any, callBackName: string, element: HTMLElement): void {

    const handler = (event: FocusEvent) => {

        if (!element.contains(event.relatedTarget as Node)) {
            raiseLostFocus(blazorCallBackRef, callBackName);
        }
    };
    element.addEventListener("focusout", handler);
}

function unRegisterLostMenuFocus(blazorCallBackRef: any, callBackName: string, element: HTMLElement): void {

    const handler = (event: FocusEvent) => {

        if (!element.contains(event.relatedTarget as Node)) {
            raiseLostFocus(blazorCallBackRef, callBackName);
        }
    };
    element?.removeEventListener("focusout", handler);
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
