using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel
{
    
    public class CreateAction : FireBoltAction
    {
        [XmlAttribute(AttributeName = "actorNameParamId")]
        public int ActorNameParamId { get; set; }

        [XmlAttribute(AttributeName = "originParamId")]
        public int OriginParamId { get; set; }


    }
}
