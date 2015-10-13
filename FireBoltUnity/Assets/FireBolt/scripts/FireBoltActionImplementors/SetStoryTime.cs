using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.scripts
{    
    public class SetStoryTime : FireBoltAction
    {
        private float storyTimeOffset, previousStoryTimeOffset;
        
        public SetStoryTime(float storyTimeOffset, float previousStoryTimeOffset, float startTick, float endTick) :
            base(startTick, endTick)
        {
            this.storyTimeOffset = storyTimeOffset;
            this.previousStoryTimeOffset = previousStoryTimeOffset;            
        }

        public override bool Init()
        {                     
            return true;
        }

        public override void Execute()
        {
            if (Mathf.Abs(ElPresidente.Instance.CurrentStoryTime - (ElPresidente.Instance.CurrentDiscourseTime + storyTimeOffset)) > ElPresidente.MILLIS_PER_FRAME)
            {
                ElPresidente.Instance.goToStoryTime(ElPresidente.Instance.CurrentDiscourseTime + storyTimeOffset);
            }
        }

        public override void Stop()
        {
            //nothin
        }

        public override void Undo()
        {
            Debug.Log(string.Format("set story time[{0}] ", startTick + previousStoryTimeOffset));
            ElPresidente.Instance.goToStoryTime(startTick + previousStoryTimeOffset);
        }

        public override void Skip()
        {
            Debug.Log(string.Format("set story time[{0}] ", endTick + storyTimeOffset));
            ElPresidente.Instance.goToStoryTime(endTick + storyTimeOffset);
        }
    }
}
