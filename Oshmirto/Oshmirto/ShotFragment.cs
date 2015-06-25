using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Oshmirto
{
    [Serializable]
    public class ShotFragment
    {
        [XmlAttribute("anchor")]
        public string Anchor { get; set; }

        [XmlAttribute("framingTarget")]
        public string FramingTarget { get; set; }

        [XmlAttribute("startTime")]
        public float StartTime { get; set; }
    }
}
