using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.scripts
{
    public interface IActorAction {
        long StartTick();
        long EndTick();
	    void Execute();
        void Stop();
        void Init();
    }

}
