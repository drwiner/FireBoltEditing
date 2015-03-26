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

        private void init()
        {
            GameObject actor = new GameObject(actorName);
            actor.AddComponent<MeshRenderer>();
            actor.AddComponent<MeshFilter>();
            GameObject modelMesh = Resources.Load(modelName) as GameObject;
            actor.GetComponent<MeshFilter>().mesh = modelMesh.GetComponent<MeshFilter>().sharedMesh;
            actor.transform.position = position;
        }
        private void execute()
        {
            //nothing to execute
        }
        private void stop()
        {
            //nothing to end
        }
    }
}
