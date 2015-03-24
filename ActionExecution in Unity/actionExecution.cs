using UnityEngine;
using System.Collections;

namespace CommandPattern {

//Invoker Class
public class actionExecution  {


	public static void play(ICommand playCommand) {
		
		playCommand.Execute ();
	}

	public static void stop(ICommand stopCommand) {
		stopCommand.Execute ();
	}
}

public interface ICommand {
	 void Execute();
}

}