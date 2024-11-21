class Instructions extends HTMLElement {
    constructor() {
        super();
        this.loadTemplate(); // Appeler une méthode asynchrone qui gère tout.
    }

    // Méthode pour charger le template
    async loadTemplate() {
        try {
            const response = await fetch('./components/instructions/instructions.html');
            if (!response.ok) {
                throw new Error(`Erreur de chargement du template: ${response.statusText}`);
            }
            const htmlContent = await response.text();
            console.log(htmlContent);

            const doc = new DOMParser().parseFromString(htmlContent, "text/html");
            const templateContent = doc.querySelector("template").content;
            if(templateContent){

                const shadowRoot = this.attachShadow({ mode: 'open' });
                shadowRoot.appendChild(templateContent.cloneNode(true));
            }else{
                console.log("ERREUR PAS de templateContent");
            }
            
        } catch (error) {
            console.error("Erreur lors du chargement du template :", error);
        }
    }

}
customElements.define('instructions-box', Instructions);