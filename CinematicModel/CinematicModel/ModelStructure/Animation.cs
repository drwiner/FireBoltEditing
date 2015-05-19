using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using CinematicModel.ModelStructure;

namespace CinematicModel
{
    public class Animation
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "fileName")]
        public string FileName { get; set; }

        [XmlAttribute("duration")]
        public int Duration { get; set; }

        [XmlArray("AnimationIndices")]
        [XmlArrayItem("AnimationIndex")]
        public List<AnimationIndex> AnimationIndices { get; set; }
    }
}
