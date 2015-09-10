using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.scripts
{
    public class SetStoryTime : IFireBoltAction
    {
        private float targetStoryTime;
        private float returnStoryTime;
        private float startTick, endTick;
        
        public SetStoryTime(float targetStoryTime, float startTick, float endTick)
        {
            this.targetStoryTime = targetStoryTime;
            this.startTick = startTick;
            this.endTick = endTick;
        }

        public bool Init()
        {
            returnStoryTime = ElPresidente.Instance.getCurrentStoryTime();
            Skip();
            return true;
        }

        public void Execute()
        {
            //nothin
        }

        public void Stop()
        {
            //nothin
        }

        public float StartTick()
        {
            return startTick;
        }

        public float EndTick()
        {
            return endTick;
        }

        public void Undo()
        {
            Debug.Log(string.Format("set story time[{0}] ", returnStoryTime));
            ElPresidente.Instance.goToStoryTime(returnStoryTime);
        }

        public void Skip()
        {
            Debug.Log(string.Format("set story time[{0}] ", targetStoryTime));
            ElPresidente.Instance.goToStoryTime(targetStoryTime);
        }
    }
}
