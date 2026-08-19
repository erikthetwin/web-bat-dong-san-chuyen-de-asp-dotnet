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

    document.addEventListener('DOMContentLoaded', function () {
        var el = document.getElementById('map');
        if (el && el.dataset.lat && el.dataset.lng && window.L) {
            var lat = parseFloat(el.dataset.lat), lng = parseFloat(el.dataset.lng);
            var map = L.map('map').setView([lat, lng], 14);
            L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', { attribution: '&copy; OpenStreetMap' }).addTo(map);
            L.marker([lat, lng]).addTo(map).bindPopup(el.dataset.title);
        }
    });
})();
