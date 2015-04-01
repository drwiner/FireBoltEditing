using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel
{
    
    public class FireBoltAction  
    {
        [XmlAttribute(AttributeName = "startTickParamId")]
        public int StartTickParamId { get; set; }


    }
}
