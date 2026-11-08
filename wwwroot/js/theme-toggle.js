(function () {
    'use strict';

    function currentTheme() {
        return document.documentElement.getAttribute('data-theme') === 'light' ? 'light' : 'dark';
    }

    function applyTheme(theme) {
        if (theme === 'light') {
            document.documentElement.setAttribute('data-theme', 'light');
        } else {
            document.documentElement.removeAttribute('data-theme');
        }
        try {
            localStorage.setItem('theme', theme);
        } catch (e) {
            /* localStorage no disponible (modo privado, etc.) */
        }
    }

    document.addEventListener('DOMContentLoaded', function () {
        var btn = document.getElementById('theme-toggle');
        if (!btn) { return; }

        btn.setAttribute('aria-pressed', currentTheme() === 'light' ? 'true' : 'false');

        btn.addEventListener('click', function () {
            var next = currentTheme() === 'light' ? 'dark' : 'light';
            applyTheme(next);
            btn.setAttribute('aria-pressed', next === 'light' ? 'true' : 'false');
        });
    });
}());
