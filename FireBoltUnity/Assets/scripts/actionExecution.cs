using UnityEngine;
using System.Collections;

namespace Assets.scripts

{

    //Invoker Class
    public class actionExecution
    {


        public static void play(IActorAction playCommand)
        {

            playCommand.Execute();
        }

        public static void stop(IActorAction stopCommand)
        {
            stopCommand.Execute();
        }
    }
}

