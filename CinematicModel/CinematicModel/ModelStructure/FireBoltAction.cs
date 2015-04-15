using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel
{
    
    public class FireBoltAction  
    {
        [XmlAttribute(AttributeName = "actorNameParamName")]
        public string ActorNameParamName { get; set; }

        [XmlAttribute(AttributeName = "startTickParamName")]
        public string StartTickParamName { get; set; }
    }
}
