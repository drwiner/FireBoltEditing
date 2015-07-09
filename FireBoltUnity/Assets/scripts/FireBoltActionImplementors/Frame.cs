using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oshmirto;
using UnityEngine;
using LN.Utilities;

namespace Assets.scripts
{
    public class Frame : IFireBoltAction
    {
        private float startTick, endTick;
        private string actorName;
        private List<Framing> framings;
        private string subjectName;

        private static readonly float FRAMING_STEP_SIZE = 2.5f;

        private FramingType framingType;
        private Translate translate;
        private Rotate rotate;
        private bool initialized = false;

        public Frame(float startTick, float endTick, string actorName, List<Framing> framings, Translate translate, RotateRelative rotate)
        {
            this.startTick = startTick;
            this.endTick = endTick;
            this.actorName = actorName;
            this.framings = framings;
            this.translate = translate;
            this.rotate = rotate;
        }

        public bool Init()
        {
            //if (initialized) return true;

            GameObject actor = GameObject.Find(actorName);
            if (actor == null) return false;

            subjectName = framings[0].FramingTarget;
            GameObject subject = GameObject.Find(subjectName);
            if (subject == null) return false;

            Vector3 newActorPosition;

            //find appropriate height
            Renderer renderer = subject.GetComponentInChildren<Renderer>();
            newActorPosition = renderer.bounds.center;
            
            //apply framing
            newActorPosition -= new Vector3(FRAMING_STEP_SIZE, 0, FRAMING_STEP_SIZE);

            //update translate dest
            translate.SetDestination(new Vector3Nullable(newActorPosition.x,newActorPosition.y,newActorPosition.z));
            //update rotate dest
            //rotate.SetTargetDegrees(Quaternion.LookRotation((subject.transform.position - newActorPosition).normalized).eulerAngles.y);

            initialized = true;
            return initialized;
        }

        public void Execute()
        {
            //intentionally left blank
        }

        public void Stop()
        {
            //intentionally left blank
        }

        public void Undo()
        {
            //you can't undo it!!!!!!
        }

        public void Skip()
        {
            //skipping has no meaning.  init is fired anyway
        }


        public float StartTick()
        {
            return startTick;
        }

        public float EndTick()
        {
            return endTick;
        }
    }
}
