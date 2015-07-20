using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel
{
    
    public class AnimateAction : FireBoltAction
    {
        public AnimateAction()
        {
            Effector = false;
        }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("effector")]
        [DefaultValue(false)]
        public bool Effector { get; set; }
    }
}
