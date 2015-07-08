using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using LN.Utilities;

namespace Assets.scripts
{
    public class TranslateRelative : Translate
    {
        private string trackedActorName;
        private GameObject trackedActor;
        private Vector3 trackedPositionInit;
        private Vector3 trackedPositionLast;
        private bool[] dimensionLock = {false,false,false};

        public TranslateRelative(string trackedActorName, float startTick, float endTick, string actorName, bool xLock, bool yLock, bool zLock) :
            base(startTick, endTick, actorName, Vector3.zero, new Vector3Nullable(null,null,null),true)
        {
            this.trackedActorName = trackedActorName;
            dimensionLock[0] = xLock;
            dimensionLock[1] = yLock;
            dimensionLock[2] = zLock;
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

            Vector3 actorPosition = this.actor.transform.position;
            actorPosition.x += dimensionLock[0]? 0 : move.x;
            actorPosition.y += dimensionLock[1]? 0 : move.y;
            actorPosition.z += dimensionLock[2]? 0 : move.z;
            this.actor.transform.position = actorPosition;

            trackedPositionLast = trackedPositionCurrent;
        }


        //assumes this action sorts after the move that it mirrors.  fails when we track over multiple actor moves.  
        //should implement sorting on executing actions to put relative movements last || just fix this so it's not so janky
        public override void Skip() 
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
