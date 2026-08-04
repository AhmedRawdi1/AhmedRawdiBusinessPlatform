// SB Admin Pro Sidebar & Theme Toggle JS
(function () {
    // Apply saved theme immediately to prevent FOUC (Flash of Unstyled Content)
    const savedTheme = localStorage.getItem('sb|theme-mode') || 'light';
    document.documentElement.setAttribute('data-theme', savedTheme);
    document.documentElement.setAttribute('data-bs-theme', savedTheme);
})();

window.addEventListener('DOMContentLoaded', () => {

    // 1. Sidebar Toggle
    const sidebarToggle = document.body.querySelector('#sidebarToggle');
    if (sidebarToggle) {
        if (localStorage.getItem('sb|sidebar-toggle') === 'true') {
            document.body.classList.add('sidenav-toggled');
        }

        sidebarToggle.addEventListener('click', event => {
            event.preventDefault();
            document.body.classList.toggle('sidenav-toggled');
            localStorage.setItem('sb|sidebar-toggle', document.body.classList.contains('sidenav-toggled'));
        });
    }

    // 2. Theme Mode Toggle Functionality
    const themeToggleBtn = document.querySelector('#themeToggle');
    const themeToggleIcon = document.querySelector('#themeToggleIcon');

    function updateThemeUI(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        document.documentElement.setAttribute('data-bs-theme', theme);
        localStorage.setItem('sb|theme-mode', theme);

        if (themeToggleIcon) {
            if (theme === 'dark') {
                themeToggleIcon.classList.replace('bi-sun-fill', 'bi-moon-stars-fill');
            } else {
                themeToggleIcon.classList.replace('bi-moon-stars-fill', 'bi-sun-fill');
            }
        }
    }

    // Initial theme icon sync
    const currentTheme = localStorage.getItem('sb|theme-mode') || 'light';
    updateThemeUI(currentTheme);

    if (themeToggleBtn) {
        themeToggleBtn.addEventListener('click', event => {
            event.preventDefault();
            const activeTheme = document.documentElement.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
            updateThemeUI(activeTheme);
        });
    }

    // Also support any element with [data-set-theme="light|dark"]
    document.querySelectorAll('[data-set-theme]').forEach(el => {
        el.addEventListener('click', e => {
            e.preventDefault();
            const chosenTheme = el.getAttribute('data-set-theme');
            updateThemeUI(chosenTheme);
        });
    });

});
