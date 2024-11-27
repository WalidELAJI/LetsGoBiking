let originMarker, destinationMarker;
let map, drawnItems;
let startCoordinates, destinationCoordinates;
const API_URL = 'http://localhost:8080/';

document.addEventListener('DOMContentLoaded', () => {
    initializeMap();

    // Configurer les champs d'auto-complétion
    setupAutocomplete('origin', coords => {
        startCoordinates = coords;
        console.log('Coordonnées de départ :', startCoordinates);
    });
    setupAutocomplete('destination', coords => {
        destinationCoordinates = coords;
        console.log('Coordonnées d\'arrivée :', destinationCoordinates);
    });

    // Ajouter un écouteur au bouton de confirmation
    const confirmRouteButton = document.getElementById('confirmRoute');
    if (confirmRouteButton) {
        confirmRouteButton.addEventListener('click', () => {
            const mode = getSelectedMode(); // Récupérer le mode sélectionné
            if (!mode) {
                console.error("Aucun mode de transport sélectionné.");
                showNotification("Erreur : Veuillez sélectionner un mode de transport.");
                return;
            }
            fetchItinerary(mode);
        });
    } else {
        console.error("Bouton 'confirmRoute' introuvable dans le DOM.");
    }
});

// Initialiser la carte
function initializeMap() {
    if (map) {
        map.off();
        map.remove();
    }
    map = L.map('map').setView([46.2276, 2.2137], 6); // Centré sur la France
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; OpenStreetMap contributors',
    }).addTo(map);
    drawnItems = L.layerGroup().addTo(map);
    console.log('Carte initialisée.');
}

// Récupérer l'itinéraire depuis le serveur
function fetchItinerary(mode) {
    if (!startCoordinates || !destinationCoordinates) {
        showNotification("Veuillez sélectionner un point de départ et une destination.");
        console.error('Coordonnées manquantes :', { startCoordinates, destinationCoordinates });
        return;
    }

    const url = `${API_URL}itinerary?originLat=${startCoordinates[0]}&originLon=${startCoordinates[1]}&destinationLat=${destinationCoordinates[0]}&destinationLon=${destinationCoordinates[1]}&mode=${mode}`;
    console.log('URL de requête :', url);

    fetch(url)
        .then(response => {
            if (!response.ok) throw new Error(`Erreur : ${response.statusText}`);
            return response.json();
        })
        .then(data => {
            console.log("Données reçues du serveur :", data);
            drawItineraryOnMap(data);
        })
        .catch(error => {
            console.error('Erreur lors de la récupération de l\'itinéraire :', error);
            showNotification("Erreur lors de la récupération de l'itinéraire.");
        });
}

// Dessiner l'itinéraire sur la carte
function drawItineraryOnMap(data) {
    console.log("Traitement des données reçues pour l'itinéraire...");

    // Effacer les itinéraires et marqueurs précédents
    drawnItems.clearLayers();
    console.log("Ancien itinéraire nettoyé.");

    // Ajouter les marqueurs de départ et d'arrivée
    placePin(startCoordinates, "Point de départ");
    placePin(destinationCoordinates, "Point d'arrivée");

    if (data.Itinerary && data.Itinerary.routes) {
        // Tracer les segments de l'itinéraire principal
        data.Itinerary.routes.forEach(route => {
            const coordinates = decodePolyline(route.geometry);
            L.polyline(coordinates, { color: "blue", weight: 4 }).addTo(drawnItems);
            console.log("Itinéraire tracé :", coordinates);
        });
    } else {
        console.warn("Données d'itinéraire manquantes ou mal formatées :", data);
    }
}

// Décoder une polyline OpenRouteService
function decodePolyline(encoded) {
    let points = [], index = 0, lat = 0, lng = 0;

    while (index < encoded.length) {
        let b, shift = 0, result = 0;
        do {
            b = encoded.charCodeAt(index++) - 63;
            result |= (b & 0x1f) << shift;
            shift += 5;
        } while (b >= 0x20);
        const deltaLat = (result & 1) ? ~(result >> 1) : (result >> 1);
        lat += deltaLat;

        shift = result = 0;
        do {
            b = encoded.charCodeAt(index++) - 63;
            result |= (b & 0x1f) << shift;
            shift += 5;
        } while (b >= 0x20);
        const deltaLng = (result & 1) ? ~(result >> 1) : (result >> 1);
        lng += deltaLng;

        points.push([lat / 1e5, lng / 1e5]);
    }

    return points;
}


// Ajouter un marqueur
function placePin(coordinates, label) {
    L.marker(coordinates).addTo(drawnItems).bindPopup(label).openPopup();
    console.log(`Marqueur ajouté : ${label}`, coordinates);
}

// Dessiner un segment d'itinéraire
function drawRouteSegment(segment, color, dashArray, description) {
    if (!segment || !segment.coordinates) {
        console.error(`Segment invalide pour '${description}' :`, segment);
        return;
    }

    const coordinates = segment.coordinates.map(coord => [parseFloat(coord[0]), parseFloat(coord[1])]);
    L.polyline(coordinates, { color, weight: 4, dashArray }).addTo(drawnItems);
    console.log(`Segment dessiné : ${description}`, coordinates);
}

// Notifications utilisateur
function showNotification(message) {
    const notificationContainer = document.getElementById('notifications');
    const notification = document.createElement('div');
    notification.classList.add('notification');
    notification.textContent = message;

    const closeBtn = document.createElement('button');
    closeBtn.classList.add('delete-notification');
    closeBtn.innerHTML = '&times;';
    closeBtn.addEventListener('click', () => notification.remove());

    notification.appendChild(closeBtn);
    notificationContainer.appendChild(notification);

    setTimeout(() => notification.remove(), 5000);
    console.log("Notification affichée :", message);
}

// Récupérer le mode de transport sélectionné
function getSelectedMode() {
    const modeRadios = document.getElementsByName('mode');
    for (const radio of modeRadios) {
        if (radio.checked) {
            console.log('Mode sélectionné :', radio.value);
            return radio.value;
        }
    }
    console.warn("Aucun mode sélectionné.");
    return null;
}

// Auto-complétion
function setupAutocomplete(inputId, callback) {
    const input = document.getElementById(inputId);
    input.addEventListener('input', function () {
        const query = input.value;
        if (query.length > 2) {
            fetchSuggestions(query, suggestions => displaySuggestions(input, suggestions, callback));
        }
    });
}

function fetchSuggestions(query, callback) {
    const url = `${API_URL}suggestions?query=${encodeURIComponent(query)}`;
    console.log('URL pour suggestions :', url);

    fetch(url)
        .then(response => response.json())
        .then(data => {
            const suggestions = data.map(item => ({
                displayName: item.display_name,
                coordinates: [parseFloat(item.lat), parseFloat(item.lon)],
            }));
            console.log("Suggestions reçues :", suggestions);
            callback(suggestions);
        })
        .catch(error => console.error('Erreur lors de la récupération des suggestions :', error));
}

function displaySuggestions(input, suggestions, callback) {
    const suggestionBox = input.nextElementSibling || document.createElement('div');
    suggestionBox.innerHTML = '';
    suggestionBox.classList.add('suggestion-box');

    suggestions.forEach(suggestion => {
        const item = document.createElement('div');
        item.classList.add('suggestion-item');
        item.textContent = suggestion.displayName;
        item.onclick = () => {
            input.value = suggestion.displayName;
            callback(suggestion.coordinates);
            suggestionBox.innerHTML = '';
        };
        suggestionBox.appendChild(item);
    });

    if (!input.nextElementSibling) {
        input.parentElement.appendChild(suggestionBox);
    }
    console.log("Suggestions affichées pour :", input.id, suggestions);
}
