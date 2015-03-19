using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel.ModelStructure
{
    //[XmlElement(ElementName="animationMapping")]
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

        [XmlAttribute(AttributeName = "loopAnimation")]
        public bool LoopAnimation { get; set; }

        /// <summary>
        /// percentage through the effector's duration to fire 
        /// this affected character's animation
        /// </summary>
        [XmlAttribute(AttributeName = "effectorTimeOffset")]
        public int EffectorTimeOffset { get; set; }

        [XmlArray(ElementName = "createdObjects")]
        [XmlArrayItem(ElementName = "createdObject")]
        public List<CreatedObject> CreatedObjects { get; set; }
    }
}
