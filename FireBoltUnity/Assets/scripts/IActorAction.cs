using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.scripts
{
    public interface IActorAction {        
        void Init();
	    void Execute();
        void Stop();
        float StartTick();
        float? EndTick();
    }

}
