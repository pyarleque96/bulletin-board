// ── Sliding active indicator for the nav pill.
// Tracks the .active NavLink and animates a single absolute pill behind it.
// Hover over any nav link → indicator slides to the hovered link.
// Mouse leaves the list → indicator snaps back to the active link.
// Reacts to Blazor route changes via MutationObserver on class attributes.
//
// Loaded ONCE from App.razor (after blazor.web.js). Idempotent via
// window.__navIndicatorBound guard. Tolerant to NavBar (re)mounts.
(function () {
    'use strict';

    if (window.__navIndicatorBound) return;
    window.__navIndicatorBound = true;

    var rafAttempts = 0;
    var RAF_MAX = 240; // ~4s at 60fps

    function init() {
        var list = document.getElementById('nav-pill-list');
        if (!list) {
            if (rafAttempts++ < RAF_MAX) {
                requestAnimationFrame(init);
            } else {
                // Try again later — Blazor may mount the layout after WASM init.
                document.addEventListener('click', initOnce, { once: true, passive: true });
                window.setTimeout(reinit, 1500);
            }
            return;
        }

        var indicator = list.querySelector('.nav-indicator');
        if (!indicator) return;

        // Avoid double-binding the same list element.
        if (list.__navIndicatorBound) return;
        list.__navIndicatorBound = true;

        function getActive() {
            return list.querySelector('.nav-link.active');
        }

        function moveTo(el) {
            if (!el) {
                indicator.classList.remove('is-visible');
                return;
            }
            var ulRect = list.getBoundingClientRect();
            var r = el.getBoundingClientRect();
            if (r.width === 0) return; // hidden
            indicator.style.transform = 'translateX(' + (r.left - ulRect.left) + 'px)';
            indicator.style.width = r.width + 'px';
            if (!indicator.classList.contains('is-visible')) {
                indicator.classList.add('is-visible');
            }
        }

        function snapToActive() {
            moveTo(getActive());
        }

        function bindHovers() {
            var links = list.querySelectorAll('.nav-link');
            for (var i = 0; i < links.length; i++) {
                var link = links[i];
                if (link.__indicatorBound) continue;
                link.__indicatorBound = true;
                link.addEventListener('mouseenter', (function (l) {
                    return function () { moveTo(l); };
                })(link));
                link.addEventListener('focus', (function (l) {
                    return function () { moveTo(l); };
                })(link));
            }
        }

        bindHovers();
        list.addEventListener('mouseleave', snapToActive);
        list.addEventListener('focusout', function () {
            window.setTimeout(snapToActive, 0);
        });
        window.addEventListener('resize', snapToActive);

        // Initial position (after layout).
        requestAnimationFrame(snapToActive);
        window.setTimeout(snapToActive, 120);

        // Reposition when Blazor toggles .active or rerenders the list.
        // Anti-loop guard: ignore mutations originating from the indicator itself.
        var obs = new MutationObserver(function (mutations) {
            var allFromIndicator = true;
            for (var i = 0; i < mutations.length; i++) {
                var t = mutations[i].target;
                if (t !== indicator && !indicator.contains(t)) {
                    allFromIndicator = false;
                    break;
                }
            }
            if (allFromIndicator) return;
            bindHovers();
            if (!list.matches(':hover')) snapToActive();
        });
        obs.observe(list, {
            attributes: true,
            subtree: true,
            childList: true,
            attributeFilter: ['class', 'href']
        });
    }

    function initOnce() {
        rafAttempts = 0;
        init();
    }

    function reinit() {
        rafAttempts = 0;
        init();
    }

    // Blazor enhanced navigation hook (Blazor 8+) — re-snap on each enhanced load.
    if (window.Blazor && typeof window.Blazor.addEventListener === 'function') {
        try {
            window.Blazor.addEventListener('enhancedload', reinit);
        } catch (_) { /* older Blazor — ignore */ }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init, { once: true });
    } else {
        init();
    }
})();
