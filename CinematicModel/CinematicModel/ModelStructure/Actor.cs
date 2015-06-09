using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CinematicModel
{
    public class Actor
    {
        
        private Dictionary<string, AnimationMapping> animationMappings;

        [XmlAttribute(AttributeName="name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName="id")]
        public int Id { get; set; }

        [XmlAttribute(AttributeName = "model")]
        public string Model { get; set; }

        [XmlArray("animationNames")]
        [XmlArrayItem("animationName")]
        public List<string> Animations { get; set; }

        [XmlArray(ElementName = "animationMappings")]
        [XmlArrayItem(ElementName = "animationMapping")]
        public List<AnimationMapping> AnimationMappings { get; set; }

        public Actor()
        {
            animationMappings = new Dictionary<string, AnimationMapping>();
        }

        public AnimationMapping FindAnimationMapping(string animateActionName)
        {
            AnimationMapping mapping;
            if (animationMappings.TryGetValue(animateActionName,out mapping))
            {
                return mapping;
            }
            mapping = AnimationMappings.Find(x => x.AnimateActionName == animateActionName);
            animationMappings.Add(animateActionName, mapping);
            return mapping;
        }
    }
}
