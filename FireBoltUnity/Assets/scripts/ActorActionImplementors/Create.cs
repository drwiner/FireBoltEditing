using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM=CinematicModel;

namespace Assets.scripts
{
    public class Create : IActorAction
    {
        float startTick;
        string actorName,modelName;
        Vector3 position;
        public Create(float startTick, string actorName, string modelName, Vector3 position) 
        {
            this.startTick = startTick;
            this.actorName = actorName;
            this.modelName = modelName;
            this.position = position;
        }

        public void Init()
        {
            GameObject actor = GameObject.Instantiate(Resources.Load(modelName), position, Quaternion.identity) as GameObject;
            actor.name = actorName;
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
