document.addEventListener('DOMContentLoaded', () => {
    const toggleButton = document.getElementById('theme-toggle');
    const themeIcon = document.getElementById('theme-icon');

    if (document.documentElement.getAttribute('data-theme') === 'dark' && themeIcon) {
        themeIcon.classList.replace('bi-moon-fill', 'bi-sun-fill');
    }

    if (toggleButton) {
        toggleButton.addEventListener('click', () => {
            let currentTheme = document.documentElement.getAttribute('data-theme');
            let newTheme = 'light';

            if (currentTheme !== 'dark') {
                document.documentElement.setAttribute('data-theme', 'dark');
                themeIcon.classList.replace('bi-moon-fill', 'bi-sun-fill');
                newTheme = 'dark';
            } else {
                document.documentElement.removeAttribute('data-theme');
                themeIcon.classList.replace('bi-sun-fill', 'bi-moon-fill');
            }

            localStorage.setItem('theme', newTheme);
        });
    }
});