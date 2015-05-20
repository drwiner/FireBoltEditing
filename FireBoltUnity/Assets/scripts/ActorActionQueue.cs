using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LN.Utilities.Collections;

namespace Assets.scripts
{
    public class ActorActionQueue : PriorityQueue<IActorAction>
    {
        private class ActorActionComparer : IComparer<IActorAction>
        {
            public int Compare(IActorAction x, IActorAction y)
            {
                if (x.StartTick() > y.StartTick())
                {
                    return 1;
                }
                return -1;
            }
        }

        public ActorActionQueue()
            : base(new ActorActionComparer())
        {

        }
    }
}
