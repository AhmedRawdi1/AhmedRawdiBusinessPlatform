(() => {
    const root = document.documentElement;
    const applyTheme = theme => {
        root.dataset.theme = theme;
        root.setAttribute('data-bs-theme', theme);
        localStorage.setItem('arbp-theme', theme);
        const icon = document.getElementById('themeToggleIcon');
        if (icon) icon.className = theme === 'dark' ? 'bi bi-sun' : 'bi bi-moon-stars';
    };
    applyTheme(localStorage.getItem('arbp-theme') || 'light');
    document.addEventListener('DOMContentLoaded', () => {
        const themeToggle = document.getElementById('themeToggle');
        themeToggle?.addEventListener('click', () => applyTheme(root.dataset.theme === 'dark' ? 'light' : 'dark'));
        const sidebarToggle = document.getElementById('sidebarToggle');
        const backdrop = document.getElementById('mobileBackdrop');
        const closeSidebar = () => { document.body.classList.remove('sidebar-open'); sidebarToggle?.setAttribute('aria-expanded', 'false'); };
        sidebarToggle?.addEventListener('click', () => { const open = document.body.classList.toggle('sidebar-open'); sidebarToggle.setAttribute('aria-expanded', String(open)); });
        backdrop?.addEventListener('click', closeSidebar);
        document.addEventListener('keydown', e => { if (e.key === 'Escape') closeSidebar(); });
        const password = document.getElementById('PasswordInput');
        const passwordButton = document.getElementById('togglePasswordBtn');
        passwordButton?.addEventListener('click', () => {
            const show = password?.type === 'password';
            if (password) password.type = show ? 'text' : 'password';
            const icon = document.getElementById('togglePasswordIcon');
            if (icon) icon.className = show ? 'bi bi-eye-slash' : 'bi bi-eye';
            passwordButton.setAttribute('aria-label', show ? 'Hide password' : 'Show password');
        });

        // Enter moves between form fields; Shift+Enter moves backwards.
        document.addEventListener('keydown', event => {
            if (event.key !== 'Enter' || event.isComposing) return;

            const current = event.target;
            if (!(current instanceof HTMLInputElement ||
                  current instanceof HTMLSelectElement ||
                  current instanceof HTMLTextAreaElement)) return;

            // Preserve expected behavior for multiline and action controls.
            if (current instanceof HTMLTextAreaElement) return;
            if (current instanceof HTMLInputElement &&
                ['submit', 'button', 'reset', 'image', 'file'].includes(current.type)) return;

            const form = current.form;
            if (!form) return;

            const fieldSelector = [
                'input:not([type="hidden"]):not([type="submit"]):not([type="button"]):not([type="reset"]):not([type="checkbox"]):not([type="radio"]):not([disabled]):not([readonly])',
                'select:not([disabled]):not([readonly])',
                'textarea:not([disabled]):not([readonly])'
            ].join(',');

            const fields = Array.from(form.querySelectorAll(fieldSelector)).filter(field => {
                const style = window.getComputedStyle(field);
                return field.getClientRects().length > 0 &&
                       style.visibility !== 'hidden' &&
                       style.display !== 'none' &&
                       field.tabIndex !== -1;
            });

            const index = fields.indexOf(current);
            if (index < 0) return;

            event.preventDefault();
            const nextIndex = event.shiftKey ? index - 1 : index + 1;
            const nextField = fields[nextIndex];

            if (nextField) {
                nextField.focus();
                if (nextField instanceof HTMLInputElement &&
                    ['text', 'password', 'email', 'search', 'tel', 'url', 'number'].includes(nextField.type)) {
                    nextField.select();
                }
            } else if (!event.shiftKey && form.checkValidity()) {
                form.requestSubmit();
            } else if (!event.shiftKey) {
                form.reportValidity();
            }
        });
    });
})();
