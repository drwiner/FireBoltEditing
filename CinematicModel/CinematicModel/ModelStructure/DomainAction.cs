using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel
{
    public class DomainAction
    {
        [XmlAttribute(AttributeName="name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName="id")]
        public int Id { get; set; }

        [XmlArray(ElementName = "params")]
        [XmlArrayItem(ElementName = "param")]
        public List<DomainActionParameter> Params { get; set; }

        [XmlArray("createActions")]
        [XmlArrayItem("createAction")]
        public List<CreateAction> CreateActions { get; set; }

        [XmlArray("destroyActions")]
        [XmlArrayItem("destroyAction")]
        public List<DestroyAction> DestroyActions { get; set; }

        [XmlArray("moveActions")]
        [XmlArrayItem("moveAction")]
        public List<MoveAction> MoveActions { get; set; }

        [XmlArray("rotateActions")]
        [XmlArrayItem("rotateAction")]
        public List<RotateAction> RotateActions { get; set; }

        [XmlArray("animateActions")]
        [XmlArrayItem("animateAction")]
        public List<AnimateAction> AnimateActions { get; set; }

    }
}
