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
        [XmlAttribute("millisPerTick")]
        public uint MillisPerTick { get; set; }

        [XmlArray(ElementName = "domainActions")]
        [XmlArrayItem(ElementName = "domainAction")]
        public List<DomainAction> DomainActions { get; set; }

        [XmlArray(ElementName = "actors")]
        [XmlArrayItem(ElementName = "actor")]
        public List<Actor> Actors { get; set; }

        [XmlArray(ElementName = "animationMappings")]
        [XmlArrayItem(ElementName = "animationMapping")]
        public List<AnimationMapping> AnimationMappings { get; set; }

        [XmlArray(ElementName = "animations")]
        [XmlArrayItem(ElementName = "animation")]
        public List<Animation> Animations { get; set; }

        public AnimationMapping FindAnimationMapping(string actorName, string animateActionName)
        {
            return AnimationMappings.Find(x => x.ActorName == actorName && x.AnimateActionName == animateActionName);
        }

        public Animation FindAnimation(string animationName)
        {
            return Animations.Find(x => x.Name == animationName);
        }

        public Actor FindActor(string actorName)
        {
            return Actors.Find(x => x.Name == actorName);
        }

    }
}
