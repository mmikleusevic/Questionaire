export function scrollToActiveCategory(index) {
    const activeItem = document.getElementById(`category-${index}`);
    if (activeItem) {
        activeItem.scrollIntoView({behavior: 'instant', block: 'nearest'});
    }
}

export function blurElement(element) {
    if (element) {
        element.blur();
    }
}