using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel
{
    
    public class FireBoltAction  
    {
        [XmlAttribute(AttributeName = "actorNameParamName")]
        public string ActorNameParamName { get; set; }

        [XmlAttribute(AttributeName = "startTickParamName")]
        public string StartTickParamName { get; set; }

        [XmlIgnore]
        public int? MaxDuration { get; set; }

        /// <summary>
        /// this property exists to make parsing the nullable int work.  
        /// use MaxDuration when querying the FireBolt Action
        /// </summary>
        [XmlAttribute("maxDuration")]
        public string MaxDurationAsText
        {
            get { return (MaxDuration.HasValue) ? MaxDuration.ToString() : null; }
            set { MaxDuration = !string.IsNullOrEmpty(value) ? int.Parse(value) : default(int?); }
        }
    }
}
