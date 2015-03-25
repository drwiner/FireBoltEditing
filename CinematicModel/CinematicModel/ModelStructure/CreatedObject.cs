using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel
{
    public class CreatedObject
    {
        [XmlAttribute(AttributeName = "actorId")]
        public int ActorId { get; set; }

        /// <summary>
        /// percentage through the parent animation to create the new object
        /// </summary>
        [XmlAttribute(AttributeName = "animationTimeOffset")]
        public int AnimationTimeOffset { get; set; }

        /// <summary>
        /// number of millis to bring new object to full opacity
        /// </summary>
        [XmlAttribute(AttributeName = "fadeInTime")]
        public int fadeInTime { get; set; }

        //TODO relative location needs some enumeration
        [XmlAttribute(AttributeName = "relativeLocation")]
        public int RelativeLocation { get; set; }
    }
}
