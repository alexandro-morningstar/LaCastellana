function init() {
    const htmlLabel = document.documentElement;
    const theme_preferences = localStorage.getItem('theme-preferences');

    if (theme_preferences == 'dark') {
        htmlLabel.setAttribute('data-theme', 'dark');
    }
}

document.addEventListener('DOMContentLoaded', init);