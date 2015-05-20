using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel
{
    
    public class AnimateAction : FireBoltAction
    {
        [XmlAttribute(AttributeName = "endTickParamName")]
        public string EndTickParamName { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("effector")]
        public bool Effector { get; set; }
    }
}
