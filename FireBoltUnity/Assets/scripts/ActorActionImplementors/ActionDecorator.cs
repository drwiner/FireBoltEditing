using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.scripts.ActorActionImplementors
{
    public abstract class ActionDecorator : IActorAction
    {
        private IActorAction interiorAction;
        
        public ActionDecorator(IActorAction iaa)
        {
            this.interiorAction = iaa;
        }



        public long StartTick()
        {
            throw new NotImplementedException();
        }

        public long endTick()
        {
            throw new NotImplementedException();
        }

        public void Execute()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Init()
        {
            throw new NotImplementedException();
        }
    }
}
