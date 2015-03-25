using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.scripts
{
    //TODO priority on ticks and stuff
    //TODO need a priority queue implementation
    public class ActorActionQueue : Queue<IActorAction>
    {
        public ActorActionQueue()
            : base()
        {

        }
    }
}
