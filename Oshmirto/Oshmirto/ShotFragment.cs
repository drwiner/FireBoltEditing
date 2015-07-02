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

        [XmlArray("framings")]
        [XmlArrayItem("framing")]
        public List<Framing> Framings { get; set; } //TODO parse enum

        [XmlAttribute("startTime")]
        public float StartTime { get; set; }

        [XmlAttribute("endTime")]
        public float EndTime { get; set; }

        [XmlArray("movements")]
        [XmlArrayItem("movement")]
        public List<CameraMovement> CameraMovements { get; set; }
    }
}
