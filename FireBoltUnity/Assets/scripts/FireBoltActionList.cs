using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LN.Utilities.Collections;

namespace Assets.scripts
{
    public class FireBoltActionList : SortedSet<IFireBoltAction>
    {
        private class ActorActionComparer : IComparer<IFireBoltAction>
        {
            public int Compare(IFireBoltAction x, IFireBoltAction y)
            {
                if (x.StartTick() > y.StartTick())
                {
                    return 1;
                }
                else if (x.StartTick() == y.StartTick())
                {
                    if (x is Create) return -1;

                    else if (y is Create) return 1;                        
                }
                return -1;
            }
        }
        public FireBoltActionList()
            : base(new ActorActionComparer())
        {
            NextActionIndex = 0;
        }

        /// <summary>
        /// pointer to the next action from the queue
        /// </summary>
        public int NextActionIndex { get; set; }
    }
}
