using UnityEngine;
using System.Collections;

namespace Assets.scripts
{
    public class testActionExecution : MonoBehaviour
    {

        public GameObject someGameObject;

        void Start()
        {

            AnimateMecanim newPlayAnim = new AnimateMecanim(someGameObject, "hacking");
            //playLegacy anotherPlayAnim = new playLegacy(someGameObject, "animName");

            //actionExecution.play(anotherPlayAnim);
        }

    }
}
