using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using LN.Utilities;

namespace CinematicModel
{
    [XmlRoot(ElementName="cinematicModel")]
    public class CinematicModel
    {
        private Dictionary<Tuple<string, string>, AnimationMapping> animationMappings;
        private Dictionary<string, Actor> actors;
        private Dictionary<string, Animation> animations;

        public CinematicModel()
        {
            animationMappings = new Dictionary<Tuple<string, string>, AnimationMapping>();
            actors = new Dictionary<string, Actor>();
            animations = new Dictionary<string, Animation>();
        }

        [XmlAttribute("millisPerTick")]
        public uint MillisPerTick { get; set; }

        [XmlAttribute("domainDistancePerEngineDistance")]
        public float DomainDistancePerEngineDistance { get; set; }

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
            Tuple<string,string> mappingKey = new Tuple<string,string>(actorName,animateActionName);
            AnimationMapping animationMapping;
            if(animationMappings.TryGetValue(mappingKey,out animationMapping))
            {
                return animationMapping;
            }
            animationMapping = AnimationMappings.Find(x => x.ActorName == actorName && x.AnimateActionName == animateActionName);
            animationMappings.Add(mappingKey,animationMapping);
            return animationMapping;
        }

        public Animation FindAnimation(string animationName)
        {
            Animation animation;
            if (animations.TryGetValue(animationName,out animation))
            {
                return animation;
            }
            animation = Animations.Find(x => x.Name == animationName);
            animations.Add(animationName, animation);
            return animation;
        }

        public Actor FindActor(string actorName)
        {
            Actor actor;
            if (actors.TryGetValue(actorName, out actor))
            {
                return actor;
            }
            actor = Actors.Find(x => x.Name == actorName);
            actors.Add(actorName,actor);
            return actor;
        }

    }
}
