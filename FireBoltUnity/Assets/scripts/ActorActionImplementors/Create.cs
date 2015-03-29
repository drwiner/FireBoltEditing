using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.scripts
{
    public class Create : ActionDecorator
    {
        string actorName,modelName;
        Vector3 position;
        public Create(float startTick, string actorName, string modelName, Vector3 position) : 
            base(startTick, null, null)
        {
            this.actorName = actorName;
            this.modelName = modelName;
            this.position = position;
        }

        public override void init()
        {
            //Quaternion rotation = Quaternion.Euler(new Vector3(90, 0, 0));
            GameObject actor = GameObject.Instantiate(Resources.Load(modelName),position,Quaternion.identity) as GameObject; 
            actor.name = actorName;
        }
        public override void execute()
        {
            //nothing to execute
        }
        public override void stop()
        {
            //nothing to end
        }
    }
}
