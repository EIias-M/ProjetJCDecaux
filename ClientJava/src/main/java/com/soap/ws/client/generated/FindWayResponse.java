
package com.soap.ws.client.generated;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
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
 *         &lt;element name="findWayResult" type="{http://schemas.datacontract.org/2004/07/System}ValueTupleOfstringArrayOfstringArrayOfArrayOfArrayOfdoublejpFD8EcM" minOccurs="0"/&gt;
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
    "findWayResult"
})
@XmlRootElement(name = "findWayResponse", namespace = "http://tempuri.org/")
public class FindWayResponse {

    @XmlElement(namespace = "http://tempuri.org/")
    protected ValueTupleOfstringArrayOfstringArrayOfArrayOfArrayOfdoublejpFD8EcM findWayResult;

    /**
     * Obtient la valeur de la propriété findWayResult.
     * 
     * @return
     *     possible object is
     *     {@link ValueTupleOfstringArrayOfstringArrayOfArrayOfArrayOfdoublejpFD8EcM }
     *     
     */
    public ValueTupleOfstringArrayOfstringArrayOfArrayOfArrayOfdoublejpFD8EcM getFindWayResult() {
        return findWayResult;
    }

    /**
     * Définit la valeur de la propriété findWayResult.
     * 
     * @param value
     *     allowed object is
     *     {@link ValueTupleOfstringArrayOfstringArrayOfArrayOfArrayOfdoublejpFD8EcM }
     *     
     */
    public void setFindWayResult(ValueTupleOfstringArrayOfstringArrayOfArrayOfArrayOfdoublejpFD8EcM value) {
        this.findWayResult = value;
    }

}
