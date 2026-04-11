function confirmDelete(url, itemName, itemType) {
    const modal = new bootstrap.Modal(document.getElementById('deleteModal'));
    const form = document.getElementById('deleteForm');
    const message = document.getElementById('deleteModalMessage');

    form.action = url;
    message.textContent = `Are you sure you want to delete ${itemType} "${itemName}"?`;

    modal.show();
}