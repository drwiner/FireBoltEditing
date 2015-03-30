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
        public AnimationInstance FindAnimationInstance(string actorName, string actionName, string actionParamName)
        {
            //assuming uniqueness of names.  probably should be doing validation somewhere...perhaps with an xsd?
            Actor actor = (from a in Actors where string.Equals(a.Name, actorName, StringComparison.OrdinalIgnoreCase) select a).FirstOrDefault();
            if (actor == null) { throw new Exception("actor[" + actorName + "] not found."); }

            DomainAction action = (from a in DomainActions where string.Equals(a.Name, actionName, StringComparison.OrdinalIgnoreCase) select a).FirstOrDefault();
            if (action == null) { throw new Exception("action[" + actionName + "] not found."); }

            DomainActionParameter dap = (from p in action.Params where string.Equals(p.Name, actionParamName, StringComparison.OrdinalIgnoreCase) select p).FirstOrDefault();
            //if (dap == null) { throw new Exception("action parameter[actionName=\"" + actionName+ "\",actionParamName=\""+actionParamName+"\"] not found."); }
            if (dap == null) return null; //we didn't find the sought action parameter on this action

            AnimationMapping animationMapping = (from am in AnimationMappings 
                                       where am.ActionId == action.Id && 
                                             am.ActorId == actor.Id &&
                                             am.ActionParamId == dap.Id
                                       select am).FirstOrDefault<AnimationMapping>();
            if (animationMapping == null) return null; //does not have to be a mapping, do not throw exceptions here

            Animation animation = (from anim in actor.Animations where anim.Id == animationMapping.AnimationId select anim).FirstOrDefault<Animation>();
            if (animation == null) { throw new Exception("animation[actorName=\"" + actorName + "\",animationId=\"" + animationMapping.AnimationId+ "\"] not found."); }

            //build a container and return
            return new AnimationInstance(animation,animationMapping.AnimationProperties);
            
        }

        public List<Actor> FindCreatedObjects(string actionName)
        {
            List<Actor> actors = new List<Actor>();
            List<CreatedObject> createdObjects = null;
            foreach (DomainAction da in DomainActions)
            {
                if (string.Equals(da.Name, actionName, StringComparison.OrdinalIgnoreCase))
                {
                    //createdObjects = da.CreatedObjects;
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
