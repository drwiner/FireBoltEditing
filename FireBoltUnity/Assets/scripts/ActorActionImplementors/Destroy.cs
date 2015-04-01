using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM=CinematicModel;

namespace Assets.scripts
{
    /// <summary>
    /// this only marks the object as inactive so it stops rendering.  
    /// we don't actually remove the object from the scene.  
    /// this is going to be problematic when we start using it for projectiles unless 
    /// we check for the existence of an actor already in the scene with the same name
    /// and recycle those game objects...which is do-able...then we have limit 1 of everything
    /// </summary>
    public class Destroy : IActorAction
    {
        float startTick;
        string actorName;
        public Destroy(float startTick, string actorName) 
        {
            this.startTick = startTick;
            this.actorName = actorName;
        }

        public void Init()
        {
            GameObject actor = GameObject.Find(actorName);
            actor.SetActive(false);
        }

        public void Execute()
        {
            //nothing to do
        }

        public void Stop()
        {
            //nothing to stop
        }

        public float StartTick()
        {
            return startTick;
        }

        public float? EndTick()
        {
            return null;
        }
    }
}
