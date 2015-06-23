using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.scripts
{
    public class KeyFrame
    {
        /// <summary>
        /// time in execution when keyframe was captured
        /// </summary>
        float time;

        /// <summary>
        /// all actors currently active in the hierarchy at keyframe time
        /// </summary>
        List<ActiveActor> activeActors;

        /// <summary>
        /// actions in the executing actions queue when keyframe was captured
        /// </summary>
        List<IActorAction> executingActions;

        /// <summary>
        /// time in execution when keyframe was captured
        /// </summary>
        public float Time { get { return time; } }


        /// <summary>        
        /// </summary>
        /// <param name="currentTime">time in execution when keyframe was captured</param>
        public KeyFrame(float currentTime)
        {
            this.time = currentTime;
            activeActors = new List<ActiveActor>();
            executingActions = new List<IActorAction>();
        }


    }
}
