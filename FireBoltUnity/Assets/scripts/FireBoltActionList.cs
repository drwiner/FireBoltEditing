using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LN.Utilities.Collections;

namespace Assets.scripts
{
    public class ActionTypeComparer : IComparer<IFireBoltAction>
    {
        public int Compare(IFireBoltAction x, IFireBoltAction y)
        {
            if (x.Equals(y)) return 0;
            if (x is Destroy) return 1;
            if (y is Destroy) return -1;
            if (x is Create) return -1;
            if (y is Create) return 1;

            if (x is Translate && y is Rotate) return -1;
            if (x is Rotate && y is Translate) return 1;
            return -1;
            //i THINK we should be ok ignoring other sorting orders 
            //so long as rotate and translate occur correctly relative to one another
        }
    }

    public class StartTickComparer : IComparer<IFireBoltAction>
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

    public class FireBoltActionList : SortedSet<IFireBoltAction>
    {
        public FireBoltActionList() :
            base(new StartTickComparer())
        {
            NextActionIndex = 0;
        }

        public FireBoltActionList(IComparer<IFireBoltAction> comparer) :
            base(comparer)
        {
            NextActionIndex = 0;
        }

        /// <summary>
        /// pointer to the next action from the queue
        /// </summary>
        public int NextActionIndex { get; set; }
    }
}
