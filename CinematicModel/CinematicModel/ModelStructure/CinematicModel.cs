using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

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
        public AnimationDescription FindAnimationDescription(string actorName, string actionName, string actionParamName)
        {
            //assuming uniqueness of names.  probably should be doing validation somewhere...perhaps with an xsd?
            int actorId = (from a in Actors where string.Equals(a.Name, actorName, StringComparison.OrdinalIgnoreCase) select a.Id).FirstOrDefault();            
            DomainAction action = (from a in DomainActions where string.Equals(a.Name, actionName, StringComparison.OrdinalIgnoreCase) select a).FirstOrDefault();
            int actionParamId = (from p in action.Params where string.Equals(p.Name, actionParamName, StringComparison.OrdinalIgnoreCase) select p.Id).FirstOrDefault();

            AnimationDescription ad = (from am in AnimationMappings 
                                       where am.ActionId == action.Id && 
                                             am.ActorId == actorId &&
                                             am.ActionParamId == actionParamId
                                       select am.AnimationDescription).FirstOrDefault<AnimationDescription>();
            return ad;
            
        }

        public List<Actor> FindCreatedObjects(string actionName)
        {
            List<Actor> actors = new List<Actor>();
            List<CreatedObject> createdObjects = null;
            foreach (DomainAction da in DomainActions)
            {
                if (string.Equals(da.Name, actionName, StringComparison.OrdinalIgnoreCase))
                {
                    createdObjects = da.CreatedObjects;
                }
            }
            foreach (CreatedObject co in createdObjects)
            {
                foreach(Actor a in Actors)
                {
                    if(a.Id==co.ActorId){
                        actors.Add(a);
                    }
                }
            }
            return actors;
        }

        public Actor FindActor(int id)
        {
            return (from a in Actors where a.Id == id select a).FirstOrDefault();
        }

    }
}
