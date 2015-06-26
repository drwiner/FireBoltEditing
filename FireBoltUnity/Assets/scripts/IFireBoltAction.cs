using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.scripts
{
    public interface IFireBoltAction {        
        bool Init();
	    void Execute();
        void Stop();
        float StartTick();
        float EndTick();
		void Undo();
        void Skip();
    }

}
