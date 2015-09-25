using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.scripts
{    
    public class SetStoryTime : IFireBoltAction
    {
        private float storyTimeOffset, previousStoryTimeOffset;
        private float startTick, endTick;
        
        public SetStoryTime(float storyTimeOffset, float previousStoryTimeOffset, float startTick, float endTick)
        {
            this.storyTimeOffset = storyTimeOffset;
            this.previousStoryTimeOffset = previousStoryTimeOffset;
            this.startTick = startTick;
            this.endTick = endTick;
        }

        public bool Init()
        {                     
            return true;
        }

        public void Execute()
        {
            if (Mathf.Abs(ElPresidente.Instance.CurrentStoryTime - (ElPresidente.Instance.CurrentDiscourseTime + storyTimeOffset)) > ElPresidente.MILLIS_PER_FRAME)
            {
                ElPresidente.Instance.goToStoryTime(ElPresidente.Instance.CurrentDiscourseTime + storyTimeOffset);
            }
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
            Debug.Log(string.Format("set story time[{0}] ", startTick + previousStoryTimeOffset));
            ElPresidente.Instance.goToStoryTime(startTick + previousStoryTimeOffset);
        }

        public void Skip()
        {
            Debug.Log(string.Format("set story time[{0}] ", endTick + storyTimeOffset));
            ElPresidente.Instance.goToStoryTime(endTick + storyTimeOffset);
        }
    }
}
