using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel
{
    
    public class RotateAction : FireBoltAction
    {
        [XmlAttribute(AttributeName = "endTickParamId")]
        public int EndTickParamId { get; set; }

        [XmlAttribute(AttributeName = "destinationParamId")]
        public int DestinationParamId { get; set; }

    }
}
