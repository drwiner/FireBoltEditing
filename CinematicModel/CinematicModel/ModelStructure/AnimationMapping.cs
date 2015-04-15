using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel
{
    public class AnimationMapping
    {
        [XmlAttribute(AttributeName = "actionName")]
        public string ActionName { get; set; }

        [XmlAttribute(AttributeName = "actionParamName")]
        public string ActionParamName { get; set; }

        [XmlAttribute(AttributeName = "actorName")]
        public string ActorName { get; set; }

        [XmlAttribute(AttributeName = "animationId")]
        public int AnimationId { get; set; }

        [XmlElement(ElementName= "animationProperties")]
        public AnimationProperties AnimationProperties { get; set; } 


    }
}
