using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM=CinematicModel;
//using UnityEditor;

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

        public override string ToString ()
        {
            return string.Format ("Create " + actorName);
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
            Debug.Log("init create");
            if (actor != null)
            {
                actor.SetActive(true);
                actor.transform.position = position;
                actor.transform.rotation = Quaternion.identity;
                return true;
            }
            GameObject model = null;
            if (ElPresidente.Instance.GetActiveAssetBundle().Contains(modelName))
            {
                model = ElPresidente.Instance.GetActiveAssetBundle().LoadAsset<GameObject>(modelName);
            }

            if (model == null)
            {
                Debug.LogError(string.Format("could not load asset[{0}] from assetbundle[{1}]", 
                                             modelName, ElPresidente.Instance.GetActiveAssetBundle().name));
                return false;
            }
            actor = GameObject.Instantiate(model, position, Quaternion.identity) as GameObject;
            actor.name = actorName;
            actor.transform.SetParent((GameObject.Find("InstantiatedObjects") as GameObject).transform, true);
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
