using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM=CinematicModel;
using UnityEditor;

namespace Assets.scripts
{
    public class Create : IFireBoltAction
    {
        float startTick;
        string actorName,modelName;
        Vector3 position;
		GameObject actor;

        public static bool ValidForConstruction(string actorName, string modelName)
        {
            if (string.IsNullOrEmpty(actorName) || string.IsNullOrEmpty(modelName))
                return false;
            return true;
        }

        public Create(float startTick, string actorName, string modelName, Vector3 position) 
        {
            this.startTick = startTick;
            this.actorName = actorName;
            this.modelName = modelName;
            this.position = position;
			this.actor = null;
        }

        public bool Init()
        {
			if (actor != null)
			{
				actor.SetActive(true);
				actor.transform.position = position;
				actor.transform.rotation = Quaternion.identity;
				return true;
			}
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Models/" + modelName);
            //GameObject model = Resources.Load<GameObject>("Models/" + modelName);
            if (model == null)
            {
                Debug.LogError(string.Format("could not find model[{0}] to create",modelName));
                return false;
            }
            actor = GameObject.Instantiate(model, position, Quaternion.identity) as GameObject;
            actor.name = actorName;
            return true;
        }

		public void Undo()
		{
            Debug.Log ("Undo create");
			if (actor != null)
			    actor.SetActive (false);
		}

        public void Skip()
        {
            // nothing to skip
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

        public float EndTick()
        {
            return startTick;
        }
    }
}
