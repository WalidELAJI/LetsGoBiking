
package com.example.generated;

import javax.xml.bind.JAXBElement;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElementRef;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Classe Java pour anonymous complex type.
 * 
 * <p>Le fragment de schéma suivant indique le contenu attendu figurant dans cette classe.
 * 
 * <pre>
 * &lt;complexType&gt;
 *   &lt;complexContent&gt;
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType"&gt;
 *       &lt;sequence&gt;
 *         &lt;element name="originLat" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/&gt;
 *         &lt;element name="originLon" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/&gt;
 *         &lt;element name="destinationLat" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/&gt;
 *         &lt;element name="destinationLon" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/&gt;
 *         &lt;element name="mode" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/&gt;
 *       &lt;/sequence&gt;
 *     &lt;/restriction&gt;
 *   &lt;/complexContent&gt;
 * &lt;/complexType&gt;
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "", propOrder = {
    "originLat",
    "originLon",
    "destinationLat",
    "destinationLon",
    "mode"
})
@XmlRootElement(name = "getGeneratedItinerary")
public class GetGeneratedItinerary {

    @XmlElementRef(name = "originLat", namespace = "http://tempuri.org/", type = JAXBElement.class, required = false)
    protected JAXBElement<String> originLat;
    @XmlElementRef(name = "originLon", namespace = "http://tempuri.org/", type = JAXBElement.class, required = false)
    protected JAXBElement<String> originLon;
    @XmlElementRef(name = "destinationLat", namespace = "http://tempuri.org/", type = JAXBElement.class, required = false)
    protected JAXBElement<String> destinationLat;
    @XmlElementRef(name = "destinationLon", namespace = "http://tempuri.org/", type = JAXBElement.class, required = false)
    protected JAXBElement<String> destinationLon;
    @XmlElementRef(name = "mode", namespace = "http://tempuri.org/", type = JAXBElement.class, required = false)
    protected JAXBElement<String> mode;

    /**
     * Obtient la valeur de la propriété originLat.
     * 
     * @return
     *     possible object is
     *     {@link JAXBElement }{@code <}{@link String }{@code >}
     *     
     */
    public JAXBElement<String> getOriginLat() {
        return originLat;
    }

    /**
     * Définit la valeur de la propriété originLat.
     * 
     * @param value
     *     allowed object is
     *     {@link JAXBElement }{@code <}{@link String }{@code >}
     *     
     */
    public void setOriginLat(JAXBElement<String> value) {
        this.originLat = value;
    }

    /**
     * Obtient la valeur de la propriété originLon.
     * 
     * @return
     *     possible object is
     *     {@link JAXBElement }{@code <}{@link String }{@code >}
     *     
     */
    public JAXBElement<String> getOriginLon() {
        return originLon;
    }

    /**
     * Définit la valeur de la propriété originLon.
     * 
     * @param value
     *     allowed object is
     *     {@link JAXBElement }{@code <}{@link String }{@code >}
     *     
     */
    public void setOriginLon(JAXBElement<String> value) {
        this.originLon = value;
    }

    /**
     * Obtient la valeur de la propriété destinationLat.
     * 
     * @return
     *     possible object is
     *     {@link JAXBElement }{@code <}{@link String }{@code >}
     *     
     */
    public JAXBElement<String> getDestinationLat() {
        return destinationLat;
    }

    /**
     * Définit la valeur de la propriété destinationLat.
     * 
     * @param value
     *     allowed object is
     *     {@link JAXBElement }{@code <}{@link String }{@code >}
     *     
     */
    public void setDestinationLat(JAXBElement<String> value) {
        this.destinationLat = value;
    }

    /**
     * Obtient la valeur de la propriété destinationLon.
     * 
     * @return
     *     possible object is
     *     {@link JAXBElement }{@code <}{@link String }{@code >}
     *     
     */
    public JAXBElement<String> getDestinationLon() {
        return destinationLon;
    }

    /**
     * Définit la valeur de la propriété destinationLon.
     * 
     * @param value
     *     allowed object is
     *     {@link JAXBElement }{@code <}{@link String }{@code >}
     *     
     */
    public void setDestinationLon(JAXBElement<String> value) {
        this.destinationLon = value;
    }

    /**
     * Obtient la valeur de la propriété mode.
     * 
     * @return
     *     possible object is
     *     {@link JAXBElement }{@code <}{@link String }{@code >}
     *     
     */
    public JAXBElement<String> getMode() {
        return mode;
    }

    /**
     * Définit la valeur de la propriété mode.
     * 
     * @param value
     *     allowed object is
     *     {@link JAXBElement }{@code <}{@link String }{@code >}
     *     
     */
    public void setMode(JAXBElement<String> value) {
        this.mode = value;
    }

}
