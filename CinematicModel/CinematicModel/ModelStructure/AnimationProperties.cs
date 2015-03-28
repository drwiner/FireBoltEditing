using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel
{
    public class AnimationProperties
    {
        [XmlAttribute(AttributeName = "loopAnimation")]
        public bool LoopAnimation { get; set; }

        /// <summary>
        /// percentage through the effector's duration to fire 
        /// this affected character's animation
        /// </summary>
        [XmlAttribute(AttributeName = "effectorTimeOffset")]
        public int EffectorTimeOffset { get; set; }

        //TODO add location to look up animation whether it's on the model or not

    }
}
