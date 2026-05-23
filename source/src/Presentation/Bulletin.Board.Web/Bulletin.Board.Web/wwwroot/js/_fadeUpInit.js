// _fadeUpInit.js — IntersectionObserver for .fade-up animation in Manage pages.
// Idempotent: safe to call multiple times; uses a WeakSet to avoid re-observing nodes.
window.bulletinDells = window.bulletinDells || {};

(function () {
    var _observed = typeof WeakSet !== 'undefined' ? new WeakSet() : null;

    window.bulletinDells.initFadeUp = function () {
        if (!('IntersectionObserver' in window)) {
            // Fallback: make all elements visible immediately.
            document.querySelectorAll('.fade-up').forEach(function (el) {
                el.classList.add('visible');
            });
            return;
        }

        var obs = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry, i) {
                if (entry.isIntersecting) {
                    setTimeout(function () {
                        entry.target.classList.add('visible');
                    }, i * 80);
                    obs.unobserve(entry.target);
                }
            });
        }, { threshold: 0.04 });

        document.querySelectorAll('.fade-up').forEach(function (el) {
            if (_observed && _observed.has(el)) return;
            obs.observe(el);
            if (_observed) _observed.add(el);
        });
    };
}());
