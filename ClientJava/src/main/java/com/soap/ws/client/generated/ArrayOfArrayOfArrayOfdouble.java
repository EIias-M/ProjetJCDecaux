
package com.soap.ws.client.generated;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Classe Java pour ArrayOfArrayOfArrayOfdouble complex type.
 * 
 * <p>Le fragment de schéma suivant indique le contenu attendu figurant dans cette classe.
 * 
 * <pre>
 * &lt;complexType name="ArrayOfArrayOfArrayOfdouble"&gt;
 *   &lt;complexContent&gt;
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType"&gt;
 *       &lt;sequence&gt;
 *         &lt;element name="ArrayOfArrayOfdouble" type="{http://schemas.microsoft.com/2003/10/Serialization/Arrays}ArrayOfArrayOfdouble" maxOccurs="unbounded" minOccurs="0"/&gt;
 *       &lt;/sequence&gt;
 *     &lt;/restriction&gt;
 *   &lt;/complexContent&gt;
 * &lt;/complexType&gt;
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "ArrayOfArrayOfArrayOfdouble", propOrder = {
    "arrayOfArrayOfdouble"
})
public class ArrayOfArrayOfArrayOfdouble {

    @XmlElement(name = "ArrayOfArrayOfdouble", nillable = true)
    protected List<ArrayOfArrayOfdouble> arrayOfArrayOfdouble;

    /**
     * Gets the value of the arrayOfArrayOfdouble property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the arrayOfArrayOfdouble property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getArrayOfArrayOfdouble().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link ArrayOfArrayOfdouble }
     * 
     * 
     */
    public List<ArrayOfArrayOfdouble> getArrayOfArrayOfdouble() {
        if (arrayOfArrayOfdouble == null) {
            arrayOfArrayOfdouble = new ArrayList<ArrayOfArrayOfdouble>();
        }
        return this.arrayOfArrayOfdouble;
    }

}
