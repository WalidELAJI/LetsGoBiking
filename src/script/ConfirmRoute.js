function confirmRoute() {
    const origin = document.getElementById('origin').value;
    const destination = document.getElementById('destination').value;
    document.getElementById('notification-message').textContent = `Itinéraire confirmé de ${origin} à ${destination}`;

    const notification = document.getElementById('custom-notification');
    notification.classList.remove('hidden');

    setTimeout(() => {
        notification.classList.add('hidden');
    }, 2000);
}

var map = L.map('map').setView([51.505, -0.09], 13);
L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
}).addTo(map);
