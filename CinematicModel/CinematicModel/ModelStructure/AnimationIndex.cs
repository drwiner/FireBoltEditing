using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel.ModelStructure
{
    public class AnimationIndex
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("timeOffset")]
        public int TimeOffset { get; set; }
    }
}
