// Site-wide JS
(function () {
    var nav = document.querySelector('.navbar');
    if (nav) {
        var onScroll = function () {
            nav.style.boxShadow = window.scrollY > 8
                ? '0 4px 16px rgba(15, 118, 110, 0.45)'
                : '0 2px 12px rgba(15, 118, 110, 0.35)';
        };
        window.addEventListener('scroll', onScroll, { passive: true });
        onScroll();
    }
})();
