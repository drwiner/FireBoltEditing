using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Oshmirto
{
    [Serializable]
    public class Block
    {
        [XmlArray("shotFragments")]
        [XmlArrayItem("shotFragment")]
        public List<ShotFragment> ShotFragments { get; set; }
    }
}
