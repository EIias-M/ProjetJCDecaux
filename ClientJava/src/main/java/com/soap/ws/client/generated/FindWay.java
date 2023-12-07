
package com.soap.ws.client.generated;

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
 *         &lt;element name="addressStart" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/&gt;
 *         &lt;element name="addressEnd" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/&gt;
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
    "addressStart",
    "addressEnd"
})
@XmlRootElement(name = "findWay")
public class FindWay {

    @XmlElementRef(name = "addressStart", namespace = "http://tempuri.org/", type = JAXBElement.class, required = false)
    protected JAXBElement<String> addressStart;
    @XmlElementRef(name = "addressEnd", namespace = "http://tempuri.org/", type = JAXBElement.class, required = false)
    protected JAXBElement<String> addressEnd;

    /**
     * Obtient la valeur de la propriété addressStart.
     * 
     * @return
     *     possible object is
     *     {@link JAXBElement }{@code <}{@link String }{@code >}
     *     
     */
    public JAXBElement<String> getAddressStart() {
        return addressStart;
    }

    /**
     * Définit la valeur de la propriété addressStart.
     * 
     * @param value
     *     allowed object is
     *     {@link JAXBElement }{@code <}{@link String }{@code >}
     *     
     */
    public void setAddressStart(JAXBElement<String> value) {
        this.addressStart = value;
    }

    /**
     * Obtient la valeur de la propriété addressEnd.
     * 
     * @return
     *     possible object is
     *     {@link JAXBElement }{@code <}{@link String }{@code >}
     *     
     */
    public JAXBElement<String> getAddressEnd() {
        return addressEnd;
    }

    /**
     * Définit la valeur de la propriété addressEnd.
     * 
     * @param value
     *     allowed object is
     *     {@link JAXBElement }{@code <}{@link String }{@code >}
     *     
     */
    public void setAddressEnd(JAXBElement<String> value) {
        this.addressEnd = value;
    }

}
