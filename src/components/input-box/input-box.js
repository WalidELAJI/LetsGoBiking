class InputBox extends HTMLElement {
    constructor() {
        super();
        this._initialize(); // Appeler une méthode asynchrone qui gère tout.
    }

    async _initialize() {
        try {
            // Charger le template dans le shadowRoot
            await this.loadTemplate();

            // Initialiser la carte et les écouteurs après que le template soit chargé
            this._initializeMap();
            this._setupEventListeners();
        } catch (error) {
            console.error("Erreur lors de l'initialisation du composant :", error);
        }
    }

    // Méthode pour charger le template
    async loadTemplate() {
        try {
            const response = await fetch('./components/input-box/input-box.html');
            if (!response.ok) {
                throw new Error(`Erreur de chargement du template: ${response.statusText}`);
            }
            const htmlContent = await response.text();
            const doc = new DOMParser().parseFromString(htmlContent, "text/html");
            const templateContent = doc.querySelector("template").content;
            const shadowRoot = this.attachShadow({ mode: 'open' });
            shadowRoot.appendChild(templateContent.cloneNode(true));
        } catch (error) {
            console.error("Erreur lors du chargement du template :", error);
        }
    }

    // Initialisation de la carte
    _initializeMap() {
        const mapContainer = this.shadowRoot.querySelector('#map');
        this.map = L.map(mapContainer).setView([46.2276, 2.2137], 6);
        L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
          maxZoom: 19,
          attribution: '© OpenStreetMap contributors',
        }).addTo(this.map);
    
        // Forcer le redimensionnement de la carte
        setTimeout(() => {
          this.map.invalidateSize();
        }, 0);
      }

    // Configurer les écouteurs d'événements
    _setupEventListeners() {

        // Ajouter l'autocomplétion aux champs d'origine et de destination
        this._attachAutocompleteListeners('#origin');
        this._attachAutocompleteListeners('#destination');

        const confirmButton = this.shadowRoot.querySelector('#confirm-btn');
        if (confirmButton) {
            confirmButton.addEventListener('click', () => this.confirmRoute());
        } else {
            console.error("Bouton de confirmation introuvable dans le template !");
        }
    }
   
    // Fonction pour confirmer un itinéraire
    confirmRoute() {
        const origin = this.shadowRoot.querySelector('#origin').value;
        const destination = this.shadowRoot.querySelector('#destination').value;

        /*document.getElementById('notification-message').textContent = `Itinéraire confirmé de ${origin} à ${destination}`;
        const notification = document.getElementById('custom-notification');
        notification.classList.remove('hidden');*/

        const notificationMessage = this.shadowRoot.querySelector('#notification-message');
        notificationMessage.textContent = `Itinéraire confirmé de ${origin} à ${destination}`;
        // Afficher la notification
        const notification = this.shadowRoot.querySelector('#custom-notification');
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
                this.displayItinerary(data);
            })
            .catch(error => {
                console.error('Erreur lors de la récupération de l\'itinéraire:', error);
            });
    }

    // Fonction pour afficher l'itinéraire reçu du serveur
    displayItinerary(data) {
        // Supprimer les anciens éléments (lignes, marqueurs, etc.)
        if (this.routeLine) {
            this.map.removeLayer(this.routeLine);
            this.routeLine = null;
        }
        if (this.originMarker) {
            this.map.removeLayer(this.originMarker);
            this.originMarker = null;
        }
        if (this.destinationMarker) {
            this.map.removeLayer(this.destinationMarker);
            this.destinationMarker = null;
        }

        // Afficher les segments de l'itinéraire
        data.segments.forEach(segment => {
            const coordinates = segment.coordinates.map(coord => [coord[0], coord[1]]); // Assurez-vous du bon ordre des coordonnées
            const line = L.polyline(coordinates, { color: segment.color, weight: 4 }).addTo(this.map);

            // Afficher les marqueurs pour les stations de vélos si nécessaire
            if (segment.mode === 'bicycle') {
                L.marker(coordinates[0], { icon: this.bikeIcon }).addTo(this.map).bindPopup('Station de départ');
                L.marker(coordinates[coordinates.length - 1], { icon: this.bikeIcon }).addTo(this.map).bindPopup('Station d\'arrivée');
            }
        });

        // Centrer la carte sur l'itinéraire
        if (data.bounds) {
            const bounds = L.latLngBounds([data.bounds.southWest, data.bounds.northEast]);
            this.map.fitBounds(bounds);
        }

        // Afficher les instructions
        this.displayInstructions(data.instructions);
    }

    // Fonction pour afficher les instructions
    displayInstructions(instructions) {
        const instructionsContainer = this.shadowRoot.querySelector('#instructions');
        let instructionsHtml = '<h3><strong>Instructions</strong></h3>';

        instructions.forEach((step, index) => {
            instructionsHtml += `<p><strong>Étape ${index + 1} :</strong> ${step}</p>`;
        });

        instructionsContainer.innerHTML = instructionsHtml;
    }

    // Définir une icône personnalisée pour les stations de vélos
    get bikeIcon() {
        return L.icon({
            iconUrl: 'assets/images/bike_icon.png', // Assurez-vous que le chemin est correct
            iconSize: [40, 40], // Taille de l'icône
        });
    }

    // Fonction pour récupérer les suggestions du Geocoder
    getGeocoderSuggestions(query, callback) {
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
    displaySuggestionsFromGeocoder(input, suggestions) {
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

    // Attacher les écouteurs pour l'autocomplétion
    _attachAutocompleteListeners(inputSelector) {
        const input = this.shadowRoot.querySelector(inputSelector);
        const suggestionBox = input.nextElementSibling;
        input.addEventListener("input", () => {
          if (input.value.length > 2) {
            this.getGeocoderSuggestions(input.value, (suggestions) => {
              this.displaySuggestionsFromGeocoder(input, suggestions);
            });
          } else {
            suggestionBox.innerHTML = '';
          }
        });
      }
    

    displaySuggestionsFromGeocoder(input, suggestions) {
        const suggestionBox = input.nextElementSibling;
        suggestionBox.innerHTML = '';
        suggestionBox.classList.add('suggestion-box');
        suggestions.forEach(suggestion => {
          const item = document.createElement('div');
          item.classList.add('suggestion-item');
          item.textContent = suggestion.name;
          item.onclick = () => {
            input.value = suggestion.name;
            suggestionBox.innerHTML = '';
          };
          suggestionBox.appendChild(item);
        });
    }
}

customElements.define('input-box', InputBox);