////////////////////////// CAMPO DE PARTÍCULAS //////////////////////////////
// Motor de partículas en Canvas2D puro (sin dependencias externas).
// Reemplaza al motor anterior basado en CreateJS + TweenMax.
///////////////////////////////////////////////////////////////////////////

(function () {
    'use strict';

    var PALETTE = [
        [255, 193, 7],
        [253, 126, 20],
        [255, 224, 102],
        [243, 126, 30]
    ];

    function pick(arr) { return arr[Math.floor(Math.random() * arr.length)]; }
    function range(min, max) { return min + Math.random() * (max - min); }

    function Particle(width, height) {
        this.reset(width, height);
    }

    Particle.prototype.reset = function (width, height) {
        var tier = Math.random();
        if (tier < 0.55) {
            this.radius = range(30, 55);
            this.alphaMax = range(0.14, 0.26);
        } else if (tier < 0.9) {
            this.radius = range(60, 100);
            this.alphaMax = range(0.1, 0.2);
        } else {
            this.radius = range(110, 180);
            this.alphaMax = range(0.06, 0.14);
        }

        this.baseX = range(0, width);
        this.baseY = range(0, height);
        this.ampX = range(30, 90);
        this.ampY = range(30, 90);
        this.freqX = range(0.05, 0.15);
        this.freqY = range(0.05, 0.15);
        this.phaseX = range(0, Math.PI * 2);
        this.phaseY = range(0, Math.PI * 2);
        this.alphaFreq = range(0.1, 0.3);
        this.alphaPhase = range(0, Math.PI * 2);
        this.color = pick(PALETTE);
    };

    Particle.prototype.update = function (t) {
        this.x = this.baseX + Math.sin(t * this.freqX + this.phaseX) * this.ampX;
        this.y = this.baseY + Math.cos(t * this.freqY + this.phaseY) * this.ampY;
        this.alpha = (Math.sin(t * this.alphaFreq + this.alphaPhase) * 0.5 + 0.5) * this.alphaMax;
    };

    Particle.prototype.draw = function (ctx) {
        if (this.alpha <= 0.01) { return; }

        var c = this.color;
        var gradient = ctx.createRadialGradient(this.x, this.y, 0, this.x, this.y, this.radius);
        gradient.addColorStop(0, 'rgba(' + c[0] + ',' + c[1] + ',' + c[2] + ',' + this.alpha + ')');
        gradient.addColorStop(1, 'rgba(' + c[0] + ',' + c[1] + ',' + c[2] + ',0)');

        ctx.fillStyle = gradient;
        ctx.beginPath();
        ctx.arc(this.x, this.y, this.radius, 0, Math.PI * 2);
        ctx.fill();
    };

    function ParticleField(canvas) {
        this.canvas = canvas;
        this.ctx = canvas.getContext('2d');
        this.particles = [];
        this.rafId = null;
        this.running = false;
        this.reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

        this.loop = this.loop.bind(this);
        this.handleResize = this.handleResize.bind(this);
        this.handleVisibility = this.handleVisibility.bind(this);

        this.handleResize();
        window.addEventListener('resize', this.handleResize);
        document.addEventListener('visibilitychange', this.handleVisibility);

        if (this.reducedMotion) {
            this.update(0);
            this.render();
        } else {
            this.start();
        }
    }

    ParticleField.prototype.handleResize = function () {
        var dpr = window.devicePixelRatio || 1;
        this.width = this.canvas.clientWidth;
        this.height = this.canvas.clientHeight;
        this.canvas.width = this.width * dpr;
        this.canvas.height = this.height * dpr;
        this.ctx.setTransform(dpr, 0, 0, dpr, 0, 0);

        var area = this.width * this.height;
        var count = Math.min(26, Math.max(10, Math.round(area / 70000)));
        this.particles = [];
        for (var i = 0; i < count; i++) {
            this.particles.push(new Particle(this.width, this.height));
        }
    };

    ParticleField.prototype.update = function (t) {
        for (var i = 0; i < this.particles.length; i++) {
            this.particles[i].update(t);
        }
    };

    ParticleField.prototype.render = function () {
        var ctx = this.ctx;
        ctx.clearRect(0, 0, this.width, this.height);
        ctx.globalCompositeOperation = 'lighter';
        for (var i = 0; i < this.particles.length; i++) {
            this.particles[i].draw(ctx);
        }
        ctx.globalCompositeOperation = 'source-over';
    };

    ParticleField.prototype.loop = function (now) {
        if (!this.running) { return; }
        this.update(now / 1000);
        this.render();
        this.rafId = window.requestAnimationFrame(this.loop);
    };

    ParticleField.prototype.start = function () {
        if (this.running) { return; }
        this.running = true;
        this.rafId = window.requestAnimationFrame(this.loop);
    };

    ParticleField.prototype.stop = function () {
        this.running = false;
        if (this.rafId !== null) {
            window.cancelAnimationFrame(this.rafId);
            this.rafId = null;
        }
    };

    ParticleField.prototype.handleVisibility = function () {
        if (document.hidden) {
            this.stop();
        } else if (!this.reducedMotion) {
            this.start();
        }
    };

    document.addEventListener('DOMContentLoaded', function () {
        var canvas = document.getElementById('projector');
        if (canvas) {
            new ParticleField(canvas);
        }
    });
}());
