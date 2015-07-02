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
            Vector3 previousPosition = Vector3.zero;
            foreach (Block block in cameraPlan.Blocks)
            {

                foreach (var fragment in block.ShotFragments)
                {
                    Vector3 currentPosition;
                    if (fragment.Anchor.TryParsePlanarCoords(out currentPosition))
                    {
                        cameraActionQueue.Add(new Translate(fragment.StartTime, fragment.StartTime,
                                                            "Main Camera", previousPosition, currentPosition, true));
                        previousPosition = currentPosition;
                    }
                    else
                    {
                        //TODO handle camera position calculation
                    }
                    foreach (var movement in fragment.CameraMovements)
                    {
                        switch (movement.Type)
                        {
                            case CameraMovementType.Dolly :
                                if(movement.Directive == CameraMovementDirective.With)
                                {
                                    cameraActionQueue.Add(new TranslateRelative(movement.Subject, fragment.StartTime, fragment.EndTime, "Main Camera"));
                                }
                                
                                break;
                        }
                    }


                    float rotation = 0f;
                    if (fragment.Framings.Count > 0 && float.TryParse(fragment.Framings[0].FramingTarget, out rotation))
                    {
                        cameraActionQueue.Add(new Rotate(fragment.StartTime, fragment.StartTime, "Main Camera", rotation));
                    }
                    else
                    {
                        //TODO handle calculating actor target facing
                    }

                }
            }
        }
    }
}
