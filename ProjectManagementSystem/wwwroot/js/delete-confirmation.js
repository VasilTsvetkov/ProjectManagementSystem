function confirmDelete(url, itemName, itemType) {
    const modalElement = document.getElementById('deleteModal');
    const form = document.getElementById('deleteForm');
    const message = document.getElementById('deleteModalMessage');

    if (modalElement && form && message) {
        form.action = url;

        message.innerHTML = `Are you sure you want to delete ${itemType} <strong>"${itemName}"</strong>?`;

        const modal = new bootstrap.Modal(modalElement);
        modal.show();
    }
}