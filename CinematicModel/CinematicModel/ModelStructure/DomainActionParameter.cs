using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel
{
    //[XmlElement(ElementName="param")]
    public class DomainActionParameter
    {
        /// <summary>
        /// correlates positional value of parameter with animations to be played
        /// </summary>
        [XmlAttribute(AttributeName="id")]
        public int Id { get; set; }

        //TODO currently only supports numeric values, should eventually support ranges
        [XmlAttribute(AttributeName="cardinality")]
        public int Cardinality { get; set; }

        /// <summary>
        /// indicates that this parameter effects an animation on 
        /// any affected parameter of the same action
        /// </summary>
        [XmlAttribute(AttributeName="effector")]
        public bool Effector { get; set; }

        /// <summary>
        /// indicates this parameter has an animation triggered by 
        /// an effector parameter of the same action
        /// </summary>
        [XmlAttribute(AttributeName = "affected")]
        public bool Affected { get; set; }
    }
}
