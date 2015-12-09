using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.scripts
{
    public abstract class FireBoltAction {

        protected float startTick;
        protected float endTick;

        public FireBoltAction(float startTick, float endTick)
        {
            this.startTick = startTick;
            this.endTick = endTick;
        }

        public float StartTick()
        {
            return startTick;
        }
        public float EndTick()
        {
            return endTick;
        }

        public virtual bool Init() { return false; }
        public virtual void Execute(float currentTime) { }
        public virtual void Stop() { }
        public virtual void Undo() { }
        public virtual void Skip() { }
    }

}
