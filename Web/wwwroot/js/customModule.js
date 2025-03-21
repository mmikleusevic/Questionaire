export function scrollToActiveCategory(index) {
	const activeItem = document.getElementById(`category-${index}`);
	if (activeItem) {
		activeItem.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
	}
}