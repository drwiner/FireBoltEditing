using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CinematicModel
{
    public class AnimationInstance
    {
        private Animation animation;
        private AnimationProperties animationProperties;

        public AnimationInstance(Animation a, AnimationProperties ap)
        {
            this.animation = a;
            this.animationProperties = ap;
        }

        public string AnimationName 
        {
            get { return animation.Name; }
        }
        public int Duration 
        {
            get { return animation.Duration; }
        }
        public bool LoopAnimation
        {
            get { return animationProperties.LoopAnimation; }
        }
        public int EffectorTimeOffset
        {
            get { return animationProperties.EffectorTimeOffset; }
        }

    }
}
