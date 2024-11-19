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

    // Utiliser l'API du gouvernement pour géocoder l'adresse de départ
    geocodeAddress(origin, function(originLatLng) {
        if (originLatLng) {
            if (originMarker) {
                map.removeLayer(originMarker);
            }
            originMarker = L.marker(originLatLng).addTo(map).bindPopup("Départ").openPopup();

            // Géocoder l'adresse d'arrivée
            geocodeAddress(destination, function(destinationLatLng) {
                if (destinationLatLng) {
                    if (destinationMarker) {
                        map.removeLayer(destinationMarker);
                    }
                    destinationMarker = L.marker(destinationLatLng).addTo(map).bindPopup("Arrivée").openPopup();

                    // Appeler l'API de routage pour obtenir l'itinéraire entre le départ et l'arrivée
                    getRoute(originLatLng, destinationLatLng);
                } else {
                    alert("Adresse d'arrivée introuvable.");
                }
            });
        } else {
            alert("Adresse de départ introuvable.");
        }
    });
}

function geocodeAddress(address, callback) {
    const url = `https://api-adresse.data.gouv.fr/search/?q=${encodeURIComponent(address)}&limit=1`;
    fetch(url)
        .then(response => response.json())
        .then(data => {
            if (data.features.length > 0) {
                const feature = data.features[0];
                const latLng = [feature.geometry.coordinates[1], feature.geometry.coordinates[0]];
                callback(latLng);
            } else {
                callback(null);
            }
        })
        .catch(error => {
            console.error('Erreur lors du géocodage de l\'adresse:', error);
            callback(null);
        });
}

function getRoute(originLatLng, destinationLatLng) {
    const apiUrl = `https://router.project-osrm.org/route/v1/driving/${originLatLng[1]},${originLatLng[0]};${destinationLatLng[1]},${destinationLatLng[0]}?overview=full&geometries=geojson`;

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

var map = L.map('map').setView([46.2276, 2.2137], 6); // Centré sur la France
L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
}).addTo(map);

// Fonction pour récupérer les suggestions de l'API du gouvernement français
function getGeocoderSuggestions(query, callback) {
    const url = `https://api-adresse.data.gouv.fr/search/?q=${encodeURIComponent(query)}&limit=5&autocomplete=1`;
    fetch(url)
        .then(response => response.json())
        .then(data => {
            const suggestions = data.features.map(feature => ({
                name: feature.properties.label,
                coordinates: [feature.geometry.coordinates[1], feature.geometry.coordinates[0]]
            }));
            callback(suggestions);
        })
        .catch(error => console.error('Erreur lors de la récupération des suggestions:', error));
}

// Fonction pour afficher les suggestions dans une boîte sous l'input
function displaySuggestionsFromGeocoder(input, suggestions) {
    const suggestionBox = input.nextElementSibling; // Sélectionne l'élément suivant l'input pour afficher les suggestions
    suggestionBox.innerHTML = '';
    suggestionBox.classList.add('suggestion-box'); // Assure que la classe est présente

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

// Ajout de l'auto-complétion via l'API du gouvernement pour le champ départ
document.getElementById('origin').addEventListener('input', function () {
    const input = this;
    if (input.value.length > 2) {  // Ne pas déclencher si moins de 3 caractères
        getGeocoderSuggestions(input.value, function (suggestions) {
            displaySuggestionsFromGeocoder(input, suggestions);
        });
    } else {
        input.nextElementSibling.innerHTML = ''; // Vider les suggestions si moins de 3 caractères
    }
});

// Ajout de l'auto-complétion via l'API du gouvernement pour le champ arrivée
document.getElementById('destination').addEventListener('input', function () {
    const input = this;
    if (input.value.length > 2) {  // Ne pas déclencher si moins de 3 caractères
        getGeocoderSuggestions(input.value, function (suggestions) {
            displaySuggestionsFromGeocoder(input, suggestions);
        });
    } else {
        input.nextElementSibling.innerHTML = ''; // Vider les suggestions si moins de 3 caractères
    }
});
