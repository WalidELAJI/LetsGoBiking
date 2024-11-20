let originMarker, destinationMarker;
let routeLine; // Ligne d'itinéraire actuelle

function confirmRoute() {
    const origin = document.getElementById('origin').value;
    const destination = document.getElementById('destination').value;

    document.getElementById('notification-message').textContent = `Itinéraire confirmé de ${origin} à ${destination}`;

    const notification = document.getElementById('custom-notification');
    notification.classList.remove('hidden');

    setTimeout(() => {
        notification.classList.add('hidden');
    }, 2000);

    // Appeler le serveur C# pour obtenir l'itinéraire
    const url = `http://localhost:5000/Itinerary?origin=${encodeURIComponent(origin)}&destination=${encodeURIComponent(destination)}`;

    fetch(url)
        .then(response => response.json())
        .then(data => {
            // Afficher l'itinéraire sur la carte
            displayItinerary(data);
        })
        .catch(error => {
            console.error('Erreur lors de la récupération de l\'itinéraire:', error);
        });
}

// Supprimer ou commenter les fonctions suivantes car la logique sera déplacée vers le serveur :

/*
// Fonction pour obtenir l'itinéraire depuis OpenRouteService
function getRoute(originLatLng, destinationLatLng, mode) {
    // ... Code à supprimer
}

// Fonction pour récupérer et afficher les stations de vélos
function getBikeStationsAlongRoute(routeLine) {
    // ... Code à supprimer
}
*/

// Fonction pour afficher l'itinéraire reçu du serveur
function displayItinerary(data) {
    // Supprimer les anciens éléments (lignes, marqueurs, etc.)
    if (routeLine) {
        map.removeLayer(routeLine);
        routeLine = null;
    }
    if (originMarker) {
        map.removeLayer(originMarker);
        originMarker = null;
    }
    if (destinationMarker) {
        map.removeLayer(destinationMarker);
        destinationMarker = null;
    }

    // Afficher les segments de l'itinéraire
    data.segments.forEach(segment => {
        const coordinates = segment.coordinates.map(coord => [coord[0], coord[1]]); // Assurez-vous du bon ordre des coordonnées
        const line = L.polyline(coordinates, { color: segment.color, weight: 4 }).addTo(map);

        // Afficher les marqueurs pour les stations de vélos si nécessaire
        if (segment.mode === 'bicycle') {
            L.marker(coordinates[0], { icon: bikeIcon }).addTo(map).bindPopup('Station de départ');
            L.marker(coordinates[coordinates.length - 1], { icon: bikeIcon }).addTo(map).bindPopup('Station d\'arrivée');
        }
    });

    // Centrer la carte sur l'itinéraire
    if (data.bounds) {
        const bounds = L.latLngBounds([data.bounds.southWest, data.bounds.northEast]);
        map.fitBounds(bounds);
    }

    // Afficher les instructions
    displayInstructions(data.instructions);
}

// Fonction pour afficher les instructions (peut être conservée ou adaptée)
function displayInstructions(instructions) {
    const instructionsContainer = document.getElementById('instructions');
    let instructionsHtml = '<h3><strong>Instructions</strong></h3>';

    instructions.forEach((step, index) => {
        instructionsHtml += `<p><strong>Étape ${index + 1} :</strong> ${step}</p>`;
    });

    instructionsContainer.innerHTML = instructionsHtml;
}

// Définir une icône personnalisée pour les stations de vélos
const bikeIcon = L.icon({
    iconUrl: 'assets/images/bike_icon.png', // Assurez-vous que le chemin est correct
    iconSize: [40, 40], // Taille de l'icône
});

// Initialisation de la carte
var map = L.map('map').setView([46.2276, 2.2137], 6); // Centré sur la France
L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '&copy; OpenStreetMap'
}).addTo(map);

// Le reste de votre code pour l'auto-complétion peut être conservé


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

