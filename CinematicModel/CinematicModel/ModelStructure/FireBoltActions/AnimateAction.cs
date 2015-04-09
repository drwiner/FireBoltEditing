using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel
{
    
    public class AnimateAction : FireBoltAction
    {
        [XmlAttribute(AttributeName = "endTickParamId")]
        public int EndTickParamId { get; set; }

    }
}
