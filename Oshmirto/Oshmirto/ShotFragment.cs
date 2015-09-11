using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Oshmirto
{
    [Serializable]
    public class ShotFragment
    {

        public ShotFragment()
        {
            Lens = "35mm";
            FStop = "22";
            Shake = 0f;
        }

        [XmlAttribute("anchor")]
        public string Anchor { get; set; }

        [XmlArray("framings")]
        [XmlArrayItem("framing")]
        public List<Framing> Framings { get; set; } 

        [XmlAttribute("duration")]
        public uint Duration { get; set; }

        [XmlArray("movements")]
        [XmlArrayItem("movement")]
        public List<CameraMovement> CameraMovements { get; set; }

        [XmlAttribute("lens")]
        [DefaultValue("35mm")]
        public string Lens { get; set; }

        [XmlAttribute("f-stop")]
        [DefaultValue("22")]
        public string FStop { get; set; }

        [XmlAttribute("focus")]
        public string FocusPosition{ get; set; }

        [XmlAttribute("shake")]
        [DefaultValue(0f)]
        public float Shake { get; set; }
    }
}
