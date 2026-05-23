// dell-landing.js — Initializers for the dark OLED Home landing.
// Idempotent: safe to call multiple times. Uses WeakSets to avoid re-observing.
window.bulletinDells = window.bulletinDells || {};

(function () {
    var _timelineSeen = typeof WeakSet !== 'undefined' ? new WeakSet() : null;
    var _fadeUpSeen   = typeof WeakSet !== 'undefined' ? new WeakSet() : null;

    function reducedMotion() {
        return window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    }

    window.bulletinDells.initDellLanding = function () {
        // 1) .fade-up observer (threshold 0.08, no stagger — matches mockup)
        var fadeNodes = document.querySelectorAll('.dell-landing .fade-up');
        if (reducedMotion() || !('IntersectionObserver' in window)) {
            fadeNodes.forEach(function (el) { el.classList.add('visible'); });
        } else {
            var fadeIo = new IntersectionObserver(function (entries) {
                entries.forEach(function (e) {
                    if (e.isIntersecting) {
                        e.target.classList.add('visible');
                        fadeIo.unobserve(e.target);
                    }
                });
            }, { threshold: 0.08 });
            fadeNodes.forEach(function (el) {
                if (_fadeUpSeen && _fadeUpSeen.has(el)) return;
                fadeIo.observe(el);
                if (_fadeUpSeen) _fadeUpSeen.add(el);
            });
        }

        // 2) [data-timeline] observer (threshold 0.4 — starts the rail draw + node glows)
        var timelineNodes = document.querySelectorAll('.dell-landing [data-timeline]');
        if (reducedMotion() || !('IntersectionObserver' in window)) {
            timelineNodes.forEach(function (el) { el.classList.add('run'); });
            return;
        }
        var tlIo = new IntersectionObserver(function (entries) {
            entries.forEach(function (e) {
                if (e.isIntersecting) {
                    e.target.classList.add('run');
                    tlIo.unobserve(e.target);
                }
            });
        }, { threshold: 0.4 });
        timelineNodes.forEach(function (el) {
            if (_timelineSeen && _timelineSeen.has(el)) return;
            tlIo.observe(el);
            if (_timelineSeen) _timelineSeen.add(el);
        });
    };

    // Auto-run on script load (covers initial SSR/prerender + WASM hydration).
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', window.bulletinDells.initDellLanding);
    } else {
        window.bulletinDells.initDellLanding();
    }
}());
