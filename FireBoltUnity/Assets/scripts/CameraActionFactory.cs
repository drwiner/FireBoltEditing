using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Impulse.v_1_336;
using UintT = Impulse.v_1_336.Interval<Impulse.v_1_336.Constants.ValueConstant<uint>, uint>;
using UintV = Impulse.v_1_336.Constants.ValueConstant<uint>;
using Oshmirto;

namespace Assets.scripts
{
    public class CameraActionFactory
    {
        //private static CameraPlan cameraPlan;
        private static FireBoltActionList cameraActionQueue;
        public static FireBoltActionList CreateCameraActions(AStory<UintV, UintT, IIntervalSet<UintV, UintT>> story, string cameraPlanPath)
        {
            cameraActionQueue = new FireBoltActionList();
            CameraPlan cameraPlan = Parser.Parse(cameraPlanPath);
            enqueueCameraActions(cameraPlan, cameraActionQueue);
            return cameraActionQueue;
        }

        private static void enqueueCameraActions(CameraPlan cameraPlan, FireBoltActionList cameraActionQueue)
        {
            foreach (var fragment in cameraPlan.ShotFragments)
            {
                Vector3 position = fragment.Anchor.ParsePlanarCoords();
                cameraActionQueue.Add(new Translate(fragment.StartTime, fragment.StartTime, "Main Camera", position, position, true));

                float rotation = 0f;
                if(fragment.Framings.Count > 0 && float.TryParse(fragment.Framings[0].FramingTarget, out rotation ))
                {
                    cameraActionQueue.Add(new Rotate(fragment.StartTime, fragment.StartTime, "Main Camera", rotation));
                }
                else
                {
                    //TODO handle calculating actor target framing
                }
                
            }
        }
    }
}
