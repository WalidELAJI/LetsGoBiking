let originMarker, destinationMarker, routeLine;
let walkingLine, cyclingLine;

function confirmRoute() {
    const origin = document.getElementById('origin').value;
    const destination = document.getElementById('destination').value;
    document.getElementById('notification-message').textContent = `Itinéraire confirmé de ${origin} à ${destination}`;

    const notification = document.getElementById('custom-notification');
    notification.classList.remove('hidden');

    setTimeout(() => {
        notification.classList.add('hidden');
    }, 2000);

    geocodeAddress(origin, function(originLatLng) {
        if (originLatLng) {
            if (originMarker) {
                map.removeLayer(originMarker);
            }
            originMarker = L.marker(originLatLng).addTo(map).bindPopup("Départ").openPopup();

            geocodeAddress(destination, function(destinationLatLng) {
                if (destinationLatLng) {
                    if (destinationMarker) {
                        map.removeLayer(destinationMarker);
                    }
                    destinationMarker = L.marker(destinationLatLng).addTo(map).bindPopup("Arrivée").openPopup();

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
    // Supprimer les anciennes lignes d'itinéraire si elles existent
    if (walkingLine) {
        map.removeLayer(walkingLine);
    }
    if (cyclingLine) {
        map.removeLayer(cyclingLine);
    }

    // Récupérer l'itinéraire à pied
    const walkingUrl = `https://router.project-osrm.org/route/v1/foot/${originLatLng[1]},${originLatLng[0]};${destinationLatLng[1]},${destinationLatLng[0]}?overview=full&geometries=geojson`;

    fetch(walkingUrl)
        .then(response => response.json())
        .then(data => {
            const routeCoordinates = data.routes[0].geometry.coordinates.map(coord => [coord[1], coord[0]]);

            walkingLine = L.polyline(routeCoordinates, { color: 'green', weight: 4, dashArray: '5,10' }).addTo(map);
            map.fitBounds(walkingLine.getBounds()); // Ajuste la carte pour que l'itinéraire soit visible
        })
        .catch(error => {
            console.error('Erreur lors de la récupération de l\'itinéraire à pied:', error);
        });

    // Récupérer l'itinéraire à vélo
    const cyclingUrl = `https://router.project-osrm.org/route/v1/bicycle/${originLatLng[1]},${originLatLng[0]};${destinationLatLng[1]},${destinationLatLng[0]}?overview=full&geometries=geojson`;

    fetch(cyclingUrl)
        .then(response => response.json())
        .then(data => {
            const routeCoordinates = data.routes[0].geometry.coordinates.map(coord => [coord[1], coord[0]]);

            cyclingLine = L.polyline(routeCoordinates, { color: 'blue', weight: 4 }).addTo(map);
            map.fitBounds(cyclingLine.getBounds()); // Ajuste la carte pour que l'itinéraire soit visible

            // Afficher les stations de vélos le long de l'itinéraire
            getBikeStationsAlongRoute(cyclingLine);
        })
        .catch(error => {
            console.error('Erreur lors de la récupération de l\'itinéraire à vélo:', error);
        });
}

// Fonction pour récupérer et afficher les stations de vélos le long de l'itinéraire
function getBikeStationsAlongRoute(routeLine) {
    const apiKey = 'c8cb5a7b30b3bac4849ab1a43f40174505597837';
    const contractName = 'lyon'; // Remplacez par le nom de la ville souhaitée
    const url = `https://api.jcdecaux.com/vls/v3/stations?contract=${contractName}&apiKey=${apiKey}`;

    fetch(url)
        .then(response => response.json())
        .then(data => {
            data.forEach(station => {
                const latLng = [station.position.latitude, station.position.longitude];
                if (isPointNearLine(latLng, routeLine, 500)) { // 500 mètres
                    L.marker(latLng, { icon: bikeIcon }).addTo(map)
                        .bindPopup(`<strong>${station.name}</strong><br>Vélos disponibles: ${station.totalStands.availabilities.bikes}<br>Places libres: ${station.totalStands.availabilities.stands}`);
                }
            });
        })
        .catch(error => {
            console.error('Erreur lors de la récupération des stations de vélos:', error);
        });
}

// Fonction pour vérifier si un point est proche d'une polyligne (itinéraire)
function isPointNearLine(point, polyline, maxDistance) {
    const latlngPoint = L.latLng(point);
    const latlngs = polyline.getLatLngs();
    for (let i = 0; i < latlngs.length - 1; i++) {
        const segmentStart = latlngs[i];
        const segmentEnd = latlngs[i + 1];
        const distance = L.GeometryUtil.distanceSegment(map, latlngPoint, segmentStart, segmentEnd);
        if (distance <= maxDistance) {
            return true;
        }
    }
    return false;
}

// Définir une icône personnalisée pour les stations de vélos
const bikeIcon = L.icon({
    iconUrl: 'assets/images/bike_icon.png', // Assurez-vous que le chemin est correct
    iconSize: [40, 40], // Taille de l'icône
});

var map = L.map('map').setView([46.2276, 2.2137], 6); // Centré sur la France
L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '&copy; OpenStreetMap'
}).addTo(map);

// Fonction pour récupérer les suggestions du Geocoder
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
