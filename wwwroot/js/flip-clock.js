//////////////////////////// FLIP CLOCK /////////////////////////////////////
// Reloj minimalista estilo "split-flap" (fecha dd·mm·aaaa / hora hh:mm:ss am-pm).
// Sin dependencias externas.
////////////////////////////////////////////////////////////////////////////

(function () {
    'use strict';

    var FIELDS = ['day', 'month', 'year', 'hour', 'minute', 'second', 'meridiem'];
    var FLIP_DURATION = 350;

    function pad(n) {
        return n < 10 ? '0' + n : String(n);
    }

    function readFields(date) {
        var hours24 = date.getHours();
        var hours12 = hours24 % 12;
        if (hours12 === 0) { hours12 = 12; }

        return {
            day: pad(date.getDate()),
            month: pad(date.getMonth() + 1),
            year: String(date.getFullYear()),
            hour: pad(hours12),
            minute: pad(date.getMinutes()),
            second: pad(date.getSeconds()),
            meridiem: hours24 < 12 ? 'AM' : 'PM'
        };
    }

    function buildCard(container, initialValue) {
        container.innerHTML =
            '<div class="flip-card">' +
                '<div class="flip-card__half flip-card__half--top"><span>' + initialValue + '</span></div>' +
                '<div class="flip-card__half flip-card__half--bottom"><span>' + initialValue + '</span></div>' +
                '<div class="flip-card__leaf flip-card__leaf--front"><span>' + initialValue + '</span></div>' +
                '<div class="flip-card__leaf flip-card__leaf--back"><span>' + initialValue + '</span></div>' +
            '</div>';
        container.dataset.value = initialValue;
    }

    function flipCard(container, newValue, reducedMotion) {
        if (container.dataset.value === newValue) { return; }

        var card = container.querySelector('.flip-card');
        var front = card.querySelector('.flip-card__leaf--front span');
        var back = card.querySelector('.flip-card__leaf--back span');
        var topHalf = card.querySelector('.flip-card__half--top span');
        var bottomHalf = card.querySelector('.flip-card__half--bottom span');

        if (reducedMotion) {
            topHalf.textContent = newValue;
            bottomHalf.textContent = newValue;
            front.textContent = newValue;
            back.textContent = newValue;
            container.dataset.value = newValue;
            return;
        }

        front.textContent = container.dataset.value;
        back.textContent = newValue;

        card.classList.remove('is-flipping');
        void card.offsetHeight; // fuerza reflow para poder reiniciar la animación
        card.classList.add('is-flipping');

        window.setTimeout(function () {
            topHalf.textContent = newValue;
            bottomHalf.textContent = newValue;
            front.textContent = newValue;
            back.textContent = newValue;
            card.classList.remove('is-flipping');
            container.dataset.value = newValue;
        }, FLIP_DURATION);
    }

    function scheduleNextTick(fn) {
        var delay = 1000 - (Date.now() % 1000);
        window.setTimeout(function () {
            fn();
            scheduleNextTick(fn);
        }, delay);
    }

    document.addEventListener('DOMContentLoaded', function () {
        var clock = document.querySelector('.flip-clock');
        if (!clock) { return; }

        var reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
        var containers = {};
        var initial = readFields(new Date());

        FIELDS.forEach(function (field) {
            var el = clock.querySelector('[data-field="' + field + '"]');
            if (!el) { return; }
            buildCard(el, initial[field]);
            containers[field] = el;
        });

        function tick() {
            var values = readFields(new Date());
            FIELDS.forEach(function (field) {
                if (containers[field]) {
                    flipCard(containers[field], values[field], reducedMotion);
                }
            });
        }

        scheduleNextTick(tick);
    });
}());
