let originMarker, destinationMarker;
let map, drawnItems;
let startCoordinates, destinationCoordinates;
const API_URL = 'http://localhost:5000/';

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

const bikeStationIcon = L.icon({
    iconUrl: 'assets/images/bike_icon.png', // Chemin vers l'image
    iconSize: [30, 30], // Taille de l'icône
    iconAnchor: [15, 30], // Point d'ancrage
});

// Fonction mise à jour pour placer les stations de vélo
function placeBikeStation(coordinates, label) {
    L.marker(coordinates, { icon: bikeStationIcon })
        .addTo(drawnItems)
        .bindPopup(label)
        .openPopup();
    console.log(`Station de vélo ajoutée : ${label}`, coordinates);
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

    // Calculer les durées
    const walkingDuration = calculateTotalDuration(data, "walking");
    const cyclingDuration = calculateTotalDuration(data, "cycling");

    if (walkingDuration > 0 && cyclingDuration > 0) {
        displayDuration(walkingDuration, cyclingDuration); // Afficher la durée
    } else {
        console.warn("Impossible de calculer les durées pour les deux modes de transport.");
    }

    if (data.UseBike && (!data.ClosestOriginStation || !data.ClosestDestinationStation)) {
        console.warn("Aucun contrat trouvé pour les vélos dans cette ville. Calcul d'un itinéraire direct à vélo.");

        try {
            const directItinerary = parseItinerary(data.Itinerary);
            if (directItinerary && directItinerary.routes && directItinerary.routes[0].geometry) {
                const coordinates = decodePolyline(directItinerary.routes[0].geometry);
                L.polyline(coordinates, { color: "blue", weight: 4 }).addTo(drawnItems);
                console.log("Itinéraire direct à vélo tracé :", coordinates);

                // Extraire les instructions
                if (directItinerary.routes[0].segments) {
                    const instructions = extractInstructions(directItinerary.routes[0].segments);
                    displayInstructions(instructions);
                } else {
                    console.warn("Pas d'instructions pour l'itinéraire direct.");
                }
            } else {
                console.warn("Itinéraire direct mal formé ou sans géométrie.");
                showNotification("Aucun itinéraire direct valide trouvé.");
            }
        } catch (error) {
            console.error("Erreur lors du traitement de l'itinéraire direct :", error);
            showNotification("Erreur lors de la récupération de l'itinéraire direct.");
        }
        return;
    }

    if (data.UseBike === false) {
        console.log("Mode walking détecté. Traçage direct de l'itinéraire.");
        const walkingItinerary = parseItinerary(data.Itinerary);
        if (walkingItinerary && walkingItinerary.routes && walkingItinerary.routes[0].segments) {
            // Extraire les instructions des segments
            const instructions = extractInstructions(walkingItinerary.routes[0].segments);
            displayInstructions(instructions);
        } else {
            console.warn("Itinéraire à pied mal formé ou sans géométrie.");
        }
    } else {
        // Mode vélo avec segments
        const instructions = [];
        ["OriginToStation", "StationToStation", "StationToDestination"].forEach(key => {
            if (data.Itinerary[key]) {
                const segmentData = parseItinerary(data.Itinerary[key]);
                if (segmentData && segmentData.routes && segmentData.routes[0].segments) {
                    instructions.push(...extractInstructions(segmentData.routes[0].segments));
                }
            }
        });
        displayInstructions(instructions);
    }

    // Vérifier si le mode est walking
    if (data.UseBike === false) {
        console.log("Mode walking détecté. Traçage direct de l'itinéraire.");
        try {
            // Décoder l'itinéraire principal
            const walkingItinerary = JSON.parse(data.Itinerary);
            if (walkingItinerary && walkingItinerary.routes && walkingItinerary.routes[0].geometry) {
                const coordinates = decodePolyline(walkingItinerary.routes[0].geometry);
                L.polyline(coordinates, { color: "blue", weight: 4 }).addTo(drawnItems);
                console.log("Itinéraire à pied tracé :", coordinates);
            } else {
                console.warn("Itinéraire à pied mal formé ou sans géométrie.");
                showNotification("Aucun itinéraire valide trouvé pour le mode walking.");
            }
        } catch (error) {
            console.error("Erreur lors du traitement de l'itinéraire à pied :", error);
            showNotification("Erreur lors de la récupération de l'itinéraire à pied.");
        }
        return;
    }

    // Mode cycling ou autre
    if (data.ClosestOriginStation) {
        placeBikeStation(
            [data.ClosestOriginStation.Latitude, data.ClosestOriginStation.Longitude],
            `Station de départ : ${data.ClosestOriginStation.Name}`
        );
    }

    if (data.ClosestDestinationStation) {
        placeBikeStation(
            [data.ClosestDestinationStation.Latitude, data.ClosestDestinationStation.Longitude],
            `Station d'arrivée : ${data.ClosestDestinationStation.Name}`
        );
    }

    // Liste des segments pour le mode cycling
    const segments = [
        { key: "OriginToStation", color: "red", dashArray: "6 6", label: "Segment vers station" },
        { key: "StationToStation", color: "blue", dashArray: "", label: "Segment inter-stations" },
        { key: "StationToDestination", color: "red", dashArray: "6 6", label: "Segment vers destination" },
    ];

    // Tracer les segments pour cycling
    segments.forEach(({ key, color, dashArray, label }) => {
        if (data.Itinerary[key]) {
            try {
                const segmentData = JSON.parse(data.Itinerary[key]);
                console.log(`Segment ${key} trouvé :`, segmentData);

                if (segmentData.routes && segmentData.routes[0].geometry) {
                    const coordinates = decodePolyline(segmentData.routes[0].geometry);
                    L.polyline(coordinates, { color, weight: 4, dashArray }).addTo(drawnItems);
                    console.log(`Segment ${label} tracé :`, coordinates);
                } else {
                    console.warn(`Segment ${label} mal formé ou sans géométrie.`);
                }
            } catch (error) {
                console.error(`Erreur lors du traitement du segment ${key} :`, error);
            }
        } else {
            console.warn(`Segment ${key} manquant dans les données.`);
        }
    });
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
// Fonction mise à jour pour gérer l'auto-complétion
function setupAutocomplete(inputId, callback) {
    const input = document.getElementById(inputId);
    let suggestionBox;
    if (inputId=='origin'){
        suggestionBox = document.getElementById('suggestion-box');
    }
    else {
        suggestionBox = document.getElementById('suggestion-box-1');
    }

    input.addEventListener('input', function () {
        const query = input.value.trim();
        if (query.length > 2) { // Ne pas déclencher si moins de 3 caractères
            fetchSuggestions(query, suggestions => displaySuggestions(input, suggestionBox, suggestions, callback));
        } else {
            suggestionBox.innerHTML = ''; // Efface les suggestions si le texte est trop court
        }
    });
}

// Fonction pour récupérer les suggestions via l'API française
function fetchSuggestions(query, callback) {
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
        .catch(error => console.error('Erreur lors de la récupération des suggestions :', error));
}

// Affichage des suggestions sous l'input
function displaySuggestions(input, suggestionBox, suggestions, callback) {
    suggestionBox.innerHTML = ''; // Vide les suggestions précédentes
    suggestions.forEach(suggestion => {
        const item = document.createElement('div');
        item.classList.add('suggestion-item');
        item.textContent = suggestion.name;
        item.addEventListener('click', () => {
            input.value = suggestion.name; // Remplit le champ avec la suggestion choisie
            suggestionBox.innerHTML = ''; // Efface les suggestions après sélection
            callback(suggestion.coordinates); // Passe les coordonnées au callback
        });
        suggestionBox.appendChild(item);
    });
}


function extractInstructions(segments) {
    const instructions = [];
    segments.forEach(segment => {
        segment.steps.forEach(step => {
            instructions.push(step.instruction);
        });
    });
    return instructions;
}

// Afficher les instructions dans la section HTML dédiée
function displayInstructions(instructions) {
    const instructionsContainer = document.getElementById("instructions");
    instructionsContainer.innerHTML = "<h3><strong>Instructions</strong></h3>"; // Réinitialiser la section

    if (instructions.length === 0) {
        instructionsContainer.innerHTML += "<p>Aucune instruction disponible.</p>";
        return;
    }

    const list = document.createElement("ol");
    instructions.forEach((instruction, index) => {
        const item = document.createElement("li");
        item.textContent = instruction;
        list.appendChild(item);
    });

    instructionsContainer.appendChild(list);
}

function parseItinerary(itinerary) {
    if (!itinerary) return null;
    if (typeof itinerary === "string") {
        try {
            return JSON.parse(itinerary);
        } catch (e) {
            console.error("Échec de l'analyse JSON :", e);
            return null;
        }
    }
    return itinerary;
}

// Fonction pour calculer la durée totale en secondes
function calculateTotalDuration(data) {
    let totalDuration = 0;

    // Si des itinéraires segmentés existent, les parcourir
    ["OriginToStation", "StationToStation", "StationToDestination"].forEach(key => {
        if (data.Itinerary[key]) {
            const segmentData = parseItinerary(data.Itinerary[key]);
            if (segmentData && segmentData.routes && segmentData.routes[0].summary) {
                totalDuration += segmentData.routes[0].summary.duration; // Ajouter la durée
            }
        }
    });

    // Si un itinéraire direct existe
    if (data.Itinerary && typeof data.Itinerary === "string") {
        const directItinerary = parseItinerary(data.Itinerary);
        if (directItinerary && directItinerary.routes && directItinerary.routes[0].summary) {
            totalDuration += directItinerary.routes[0].summary.duration; // Ajouter la durée
        }
    }

    return totalDuration; // Retourner la durée totale en secondes
}

// Fonction pour afficher la durée totale
// Fonction pour afficher la durée totale
function displayDuration(walkingDuration, cyclingDuration) {
    const durationContainer = document.getElementById("duration");
    const mode = getSelectedMode(); // Obtenir le mode de transport sélectionné

    let formattedDuration = "";
    let additionalMessage = "";

    // Définir le texte principal basé sur le mode de transport
    if (mode === "walking") {
        const minutes = Math.floor(walkingDuration / 60);
        formattedDuration = `<strong>Durée totale à pieds :</strong> ${minutes} minutes`;
    } else if (mode === "cycling") {
        const minutes = Math.floor(cyclingDuration / 60);
        formattedDuration = `<strong>Durée totale à vélo :</strong> ${minutes} minutes`;
    }

    // Afficher le contenu dans la div #duration
    durationContainer.style.display = "block";
    durationContainer.innerHTML = `${formattedDuration}${additionalMessage}`;
    console.log("Durée totale et message affichés :", formattedDuration, additionalMessage);
}


