using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel
{
    //[XmlElement(ElementName="actor")]
    public class Actor
    {
        [XmlAttribute(AttributeName="name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName="id")]
        public int Id { get; set; }

        [XmlAttribute(AttributeName = "model")]
        public string Model { get; set; }
    }
}
