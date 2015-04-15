using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel
{
    
    public class RotateAction : FireBoltAction
    {
        [XmlAttribute(AttributeName = "endTickParamName")]
        public string EndTickParamName { get; set; }

        [XmlAttribute(AttributeName = "destinationParamName")]
        public string DestinationParamName { get; set; }

    }
}
