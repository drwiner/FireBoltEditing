using UnityEngine;
using System.Collections;

namespace Assets.scripts
{
    public class testActionExecution : MonoBehaviour
    {

        public GameObject someGameObject;

        void Start()
        {

            PlayMecanim newPlayAnim = new PlayMecanim(someGameObject, "hacking");
            //playLegacy anotherPlayAnim = new playLegacy(someGameObject, "animName");

            //actionExecution.play(anotherPlayAnim);
        }

    }
}
