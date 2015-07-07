using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Oshmirto
{
    [XmlType("framingSize")]
    public enum FramingSize
    {
        ExtremeCloseUp,
        CloseUp,
        Waist,
        Full,
        Long,
        ExtremeLong,
        ExtremeLongLong,//or some such
        [XmlEnum("")]
        Angle
    }

    [Serializable]
    public class Framing
    {
        [XmlAttribute("framingSize")]
        public FramingSize FramingSize { get; set; }

        [XmlAttribute("framingTarget")]
        public string FramingTarget { get; set; }
    }
}
