// SB Admin Pro Sidebar, Theme Toggle & Keyboard Enter Key Field Navigation JS
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
        if (document.body) {
            document.body.setAttribute('data-theme', theme);
            document.body.setAttribute('data-bs-theme', theme);
        }
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

    // 3. ENTER KEY FIELD NAVIGATION
    // Move focus between input fields using the Keyboard Enter Key
    document.addEventListener('keydown', function (e) {
        if (e.key !== 'Enter') return;

        const target = e.target;
        if (!target) return;

        const tagName = target.tagName.toLowerCase();
        const isInputControl = tagName === 'input' || tagName === 'select' || tagName === 'textarea';

        // Ignore if target is not an input control
        if (!isInputControl) return;

        // Allow normal enter behavior for submit buttons, regular buttons, links, or image inputs
        const inputType = (target.type || '').toLowerCase();
        if (inputType === 'submit' || inputType === 'button' || inputType === 'reset' || inputType === 'image') {
            return;
        }

        // For multi-line textareas, allow Shift+Enter to insert newline
        if (tagName === 'textarea' && e.shiftKey) {
            return;
        }

        // Determine scope container (enclosing form or entire document)
        const container = target.form || document;

        // Query all focusable form controls
        const selector = 'input:not([type="hidden"]):not([disabled]):not([readonly]), select:not([disabled]):not([readonly]), textarea:not([disabled]):not([readonly]), button:not([disabled]):not([tabindex="-1"])';
        
        const focusables = Array.from(container.querySelectorAll(selector)).filter(el => {
            // Must be visible and interactable
            return el.offsetWidth > 0 && el.offsetHeight > 0 && getComputedStyle(el).visibility !== 'hidden' && getComputedStyle(el).display !== 'none';
        });

        const currentIndex = focusables.indexOf(target);
        if (currentIndex !== -1) {
            e.preventDefault();

            let nextIndex;
            if (e.shiftKey) {
                // Shift + Enter: Move to previous field
                nextIndex = currentIndex - 1;
                if (nextIndex < 0) nextIndex = focusables.length - 1;
            } else {
                // Enter: Move to next field
                nextIndex = currentIndex + 1;
                if (nextIndex >= focusables.length) nextIndex = 0;
            }

            const nextElement = focusables[nextIndex];
            if (nextElement) {
                nextElement.focus();
                
                // Automatically select text in text inputs for quick editing
                if (typeof nextElement.select === 'function') {
                    const type = (nextElement.type || '').toLowerCase();
                    if (type === 'text' || type === 'password' || type === 'number' || type === 'email' || type === 'tel' || type === 'search' || type === 'url') {
                        nextElement.select();
                    }
                }
            }
        }
    });

});
