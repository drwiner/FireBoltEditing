using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using CinematicModel.ModelStructure;

namespace CinematicModel
{
    [XmlRoot(ElementName="cinematicModel")]
    public class CinematicModel
    {
        [XmlArray(ElementName = "domainActions")]
        [XmlArrayItem(ElementName = "domainAction")]
        public List<DomainAction> DomainActions { get; set; }

        [XmlArray(ElementName = "actors")]
        [XmlArrayItem(ElementName = "actor")]
        public List<Actor> Actors { get; set; }

        [XmlArray(ElementName = "animationMappings")]
        [XmlArrayItem(ElementName = "animationMapping")]
        public List<AnimationMapping> AnimationMappings { get; set; }

        //TODO querying
        public AnimationDescription FindAnimationDescription(string actorName, string action, string actionParamId)
        {
            //assuming uniqueness of names.  probably should be doing validation somewhere...perhaps with an xsd?
            int actorId = (from a in Actors where string.Equals(a.Name, actorName, StringComparison.OrdinalIgnoreCase) select a.Id).First();
            int actionId = (from a in DomainActions where string.Equals(a.Name, action, StringComparison.OrdinalIgnoreCase) select a.Id).First();

            AnimationDescription ad = (from am in AnimationMappings 
                                       where am.ActionId == actionId && am.ActorId == actorId &&
                                       string.Equals(am.ActionParamId, actionParamId, StringComparison.OrdinalIgnoreCase) 
                                       select am.AnimationDescription).First<AnimationDescription>();

            return ad;
            
        }

    }
}
