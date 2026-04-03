document.addEventListener('DOMContentLoaded', () => {
    // --------- Solo hay eventos aquí.
    $('#theme-btn').on('click', function(event) {
        event.preventDefault();

        const htmlLabel = document.documentElement;

        // ------ Si el tema actual es oscuro, pasar a claro y definir el nuevo preferido.
        if (htmlLabel.getAttribute('data-theme') == 'dark') {
            htmlLabel.removeAttribute('data-theme');
            localStorage.setItem('theme-preferences', 'light');
        } else {
            htmlLabel.setAttribute('data-theme', 'dark');
            localStorage.setItem('theme-preferences', 'dark');
        }
    });
});