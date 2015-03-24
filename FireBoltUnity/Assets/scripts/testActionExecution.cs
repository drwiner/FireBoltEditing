using UnityEngine;
using System.Collections;

namespace Assets.scripts
{
    public class testActionExecution : MonoBehaviour
    {

        public GameObject someGameObject;

        void Start()
        {

            playMecanim newPlayAnim = new playMecanim(someGameObject, "hacking");
            //playLegacy anotherPlayAnim = new playLegacy(someGameObject, "animName");

            actionExecution.play(newPlayAnim);
            //actionExecution.play(anotherPlayAnim);
        }

    }
}
