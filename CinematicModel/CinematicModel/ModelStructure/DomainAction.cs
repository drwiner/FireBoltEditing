using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel
{
    public class DomainAction
    {
        [XmlAttribute(AttributeName="name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName="id")]
        public int Id { get; set; }

        [XmlAttribute(AttributeName="paramCount")]
        public int ParamCount { get; set; }

        [XmlArray(ElementName = "params")]
        [XmlArrayItem(ElementName = "param")]
        public List<DomainActionParameter> Params { get; set; }

        [XmlArray(ElementName = "createdObjects")]
        [XmlArrayItem(ElementName = "createdObject")]
        public List<CreatedObject> CreatedObjects { get; set; }

    }
}
