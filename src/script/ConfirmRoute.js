let originMarker, destinationMarker, routeLine;

function confirmRoute() {
    const origin = document.getElementById('origin').value;
    const destination = document.getElementById('destination').value;
    document.getElementById('notification-message').textContent = `Itinéraire confirmé de ${origin} à ${destination}`;

    const notification = document.getElementById('custom-notification');
    notification.classList.remove('hidden');

    setTimeout(() => {
        notification.classList.add('hidden');
    }, 2000);

    // Utiliser un service de géocodage pour obtenir les coordonnées du départ et de l'arrivée
    L.Control.Geocoder.nominatim().geocode(origin, function(results) {
        if (results.length > 0) {
            const originLatLng = results[0].center;
            if (originMarker) {
                map.removeLayer(originMarker);
            }
            originMarker = L.marker(originLatLng).addTo(map).bindPopup("Départ").openPopup();

            L.Control.Geocoder.nominatim().geocode(destination, function(results) {
                if (results.length > 0) {
                    const destinationLatLng = results[0].center;
                    if (destinationMarker) {
                        map.removeLayer(destinationMarker);
                    }
                    destinationMarker = L.marker(destinationLatLng).addTo(map).bindPopup("Arrivée").openPopup();

                    // Appeler l'API de routage pour obtenir l'itinéraire entre le départ et l'arrivée
                    getRoute(originLatLng, destinationLatLng);
                }
            });
        }
    });
}

function getRoute(originLatLng, destinationLatLng) {
    const apiUrl = `https://router.project-osrm.org/route/v1/driving/${originLatLng.lng},${originLatLng.lat};${destinationLatLng.lng},${destinationLatLng.lat}?overview=full&geometries=geojson`;

    fetch(apiUrl)
        .then(response => response.json())
        .then(data => {
            const routeCoordinates = data.routes[0].geometry.coordinates.map(coord => [coord[1], coord[0]]);

            if (routeLine) {
                map.removeLayer(routeLine);
            }

            routeLine = L.polyline(routeCoordinates, { color: 'blue', weight: 4 }).addTo(map);
            map.fitBounds(routeLine.getBounds()); // Ajuste la carte pour que l'itinéraire soit visible
        })
        .catch(error => {
            console.error('Erreur lors de la récupération de l\'itinéraire:', error);
        });
}

var map = L.map('map').setView([51.505, -0.09], 13);
L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
}).addTo(map);
L.Control.geocoder().addTo(map);

// Fonction pour récupérer les suggestions du Geocoder, limitées à la France
function getGeocoderSuggestions(query, callback) {
    L.Control.Geocoder.nominatim({
        geocodingQueryParams: {
            countrycodes: 'fr', // Limite les résultats à la France
            limit: 5 // Limite le nombre de résultats à 5 pour de meilleures performances
        }
    }).geocode(query, function(results) {
        callback(results);
    });
}

// Fonction pour afficher les suggestions dans une boîte sous l'input
function displaySuggestionsFromGeocoder(input, suggestions) {
    const suggestionBox = input.nextElementSibling; // Sélectionne l'élément suivant l'input pour afficher les suggestions
    suggestionBox.innerHTML = '';

    suggestions.forEach(suggestion => {
        const item = document.createElement('div');
        item.classList.add('suggestion-item');
        item.textContent = suggestion.name;
        item.onclick = function () {
            input.value = suggestion.name; // Remplir l'input avec la suggestion sélectionnée
            suggestionBox.innerHTML = ''; // Vider les suggestions après la sélection
        };
        suggestionBox.appendChild(item);
    });
}

// Ajout de l'auto-complétion via le Geocoder Leaflet pour le champ départ
document.getElementById('origin').addEventListener('input', function () {
    const input = this;
    if (input.value.length > 2) {  // Ne pas déclencher si moins de 3 caractères
        getGeocoderSuggestions(input.value, function (suggestions) {
            displaySuggestionsFromGeocoder(input, suggestions);
        });
    }
});

// Ajout de l'auto-complétion via le Geocoder Leaflet pour le champ arrivée
document.getElementById('destination').addEventListener('input', function () {
    const input = this;
    if (input.value.length > 2) {  // Ne pas déclencher si moins de 3 caractères
        getGeocoderSuggestions(input.value, function (suggestions) {
            displaySuggestionsFromGeocoder(input, suggestions);
        });
    }
});



