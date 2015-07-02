using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.scripts
{
    public class TranslateRelative : Translate
    {
        private string trackedActorName;
        private GameObject trackedActor;
        private Vector3 trackedPositionInit;
        private Vector3 trackedPositionLast;

        public TranslateRelative(string trackedActorName, float startTick, float endTick, string actorName) :
            base(startTick, endTick, actorName, Vector3.zero, Vector3.zero, true)
        {
            this.trackedActorName = trackedActorName;
        }

        public override bool Init()
        {
            trackedActor = GameObject.Find(trackedActorName);
            if (trackedActor != null)
            {
                trackedPositionInit = trackedActor.transform.position;
                trackedPositionLast = trackedPositionInit;
                if (base.Init())
                {
                    this.origin = actor.transform.position;
                    return true;
                }                              
            }                
            return false;
        }

        public override void Execute()
        {
            Vector3 trackedPositionCurrent = trackedActor.transform.position;
            Vector3 move = trackedPositionCurrent - trackedPositionLast;
            this.actor.transform.position += move;
            trackedPositionLast = trackedPositionCurrent;
        }

        public override void Skip() //assumes this action sorts after the move that it mirrors
        {
            this.actor.transform.position = trackedActor.transform.position - trackedPositionInit;
        }

        public override void Undo()
        {
            this.actor.transform.position = this.origin;
        }

        public override void Stop()
        {

        }
    }
}
