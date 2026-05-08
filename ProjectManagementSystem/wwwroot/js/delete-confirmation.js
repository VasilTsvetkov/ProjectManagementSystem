function confirmDelete(url, itemName, itemType) {
    const modalElement = document.getElementById('deleteModal');
    const form = document.getElementById('deleteForm');
    const message = document.getElementById('deleteModalMessage');

    if (modalElement && form && message) {
        form.action = url;

        const displayName = itemName
            ? `${itemType} <strong>"${itemName}"</strong>`
            : itemType;

        message.innerHTML = `Are you sure you want to delete ${displayName}?`;

        const modal = new bootstrap.Modal(modalElement);
        modal.show();
    }
}