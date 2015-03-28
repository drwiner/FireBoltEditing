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
        public int ActionParamId { get; set; }

        [XmlAttribute(AttributeName = "actorId")]
        public int ActorId { get; set; }

        [XmlAttribute(AttributeName = "animationId")]
        public int AnimationId { get; set; }

        [XmlElement(ElementName= "animationProperties")]
        public AnimationProperties AnimationProperties { get; set; } 


    }
}
