/**
 * @fileoverview Implementa efectos visuales dinámicos (tipo "scramble" y "glitch") 
 * para el contenedor del código de error al cargar la página.
 * Configura los atributos de datos necesarios para las animaciones CSS 
 * y utiliza temporizadores (setInterval/setTimeout) para manipular el DOM, 
 * generando transiciones de texto aleatorias y picos de fallos visuales impredecibles.
 */

document.addEventListener("DOMContentLoaded", () => {
    const errorCodeContainer = document.getElementById("error_code");
    const errorCodeText = errorCodeContainer.querySelector("h4");

    // 1. Asegurar que el <h4> tenga el data-text para que los pseudo-elementos CSS funcionen
    const textValue = errorCodeContainer.getAttribute("data-text") || errorCodeText.innerText;
    errorCodeText.setAttribute("data-text", textValue);

    // 2. Efecto Scramble (Letras aleatorias al cargar)
    const characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@#$%&*<>";
    let iterations = 0;
    const originalText = textValue;

    const interval = setInterval(() => {
        errorCodeText.innerText = originalText
            .split("")
            .map((char, index) => {
                if (index < iterations) {
                    return originalText[index];
                }
                return characters[Math.floor(Math.random() * characters.length)];
            })
            .join("");

        if (iterations >= originalText.length) {
            clearInterval(interval);
            // Asegurar que el texto vuelva a la normalidad y concuerde con el data-text
            errorCodeText.innerText = originalText; 
        }
        iterations += 1 / 4; // Ajusta este valor para cambiar la velocidad del efecto
    }, 40);

    // 3. Generador de picos de Glitch aleatorios
    // Añade la clase 'heavy-glitch' en intervalos impredecibles para dar un efecto de fallo real
    setInterval(() => {
        errorCodeText.classList.add("heavy-glitch");
        
        // Quita el glitch fuerte después de unos milisegundos
        setTimeout(() => {
            errorCodeText.classList.remove("heavy-glitch");
        }, 100 + Math.random() * 200); 

    }, 2000 + Math.random() * 4000); // Sucede entre cada 2 y 6 segundos
});