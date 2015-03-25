using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel
{
    public class AnimationMapping
    {
        [XmlAttribute(AttributeName = "actionId")]
        public int ActionId { get; set; }

        [XmlAttribute(AttributeName = "actionParamId")]
        public string ActionParamId { get; set; }

        [XmlAttribute(AttributeName = "actorId")]
        public int ActorId { get; set; }

        [XmlElement(ElementName= "animationDescription")]
        public AnimationDescription AnimationDescription { get; set; } 

        [XmlArray(ElementName = "createdObjects")]
        [XmlArrayItem(ElementName = "createdObject")]
        public List<CreatedObject> CreatedObjects { get; set; }
    }
}
