﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.scripts
{
    public interface IActorAction {
        long startTick();
        long endTick();
	    void Execute();
        void Stop();
        void Init();
    }

}