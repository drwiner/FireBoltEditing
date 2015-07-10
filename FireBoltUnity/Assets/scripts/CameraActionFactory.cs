using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using LN.Utilities;
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
        private static readonly string cameraName = "Main Camera";
        public static FireBoltActionList CreateCameraActions(AStory<UintV, UintT, IIntervalSet<UintV, UintT>> story, string cameraPlanPath)
        {
            cameraActionQueue = new FireBoltActionList();
            CameraPlan cameraPlan = Parser.Parse(cameraPlanPath);
            enqueueCameraActions(cameraPlan, cameraActionQueue);
            return cameraActionQueue;
        }

        private static void enqueueCameraActions(CameraPlan cameraPlan, FireBoltActionList cameraActionQueue)
        {
            foreach (Block block in cameraPlan.Blocks)
            {
                foreach (var fragment in block.ShotFragments)
                {
                    Vector3 futurePosition;
                    if (fragment.Anchor.TryParsePlanarCoords(out futurePosition))
                    {
                        cameraActionQueue.Add(new Translate(fragment.StartTime, fragment.StartTime,
                                                            cameraName, Vector3.zero, new Vector3Nullable(futurePosition.x,null,futurePosition.z), true));

                    }
                    else if(fragment.Framings[0] != null &&
                            fragment.Framings[0].FramingType != FramingType.None && 
                            fragment.Framings[0].FramingType != FramingType.Angle)
                    {
                        //TODO extend to support multiple framings when caclulating
                        //defer calculations to execution time....
                        Translate t = new Translate(fragment.StartTime, fragment.StartTime, cameraName, Vector3.zero, new Vector3Nullable(0,0,0), true);//translate stub to fill in at frame init
                        RotateRelative r = new RotateRelative(fragment.Framings[0].FramingTarget, fragment.StartTime, fragment.StartTime, cameraName, true, false, true); //rotate stub to fill in at frame init
                        cameraActionQueue.Add(new Frame(fragment.StartTime, fragment.StartTime, cameraName, fragment.Framings, t, r));
                        cameraActionQueue.Add(r);
                        cameraActionQueue.Add(t);
                    }


                    foreach (var movement in fragment.CameraMovements)
                    {
                        switch (movement.Type)
                        {
                            case CameraMovementType.Dolly :
                                switch (movement.Directive)
                                {
                                    case(CameraMovementDirective.With):
                                        cameraActionQueue.Add(new TranslateRelative(movement.Subject, fragment.StartTime, fragment.EndTime, cameraName, false, true, false));
                                        break;
                                    case(CameraMovementDirective.To):
                                        Vector3 destination;
                                        if (movement.Subject.TryParsePlanarCoords(out destination))
                                        {
                                            cameraActionQueue.Add(new Translate(fragment.StartTime, fragment.EndTime, cameraName,
                                                                                Vector3.zero, new Vector3Nullable(destination.x,null,destination.z),true));
                                        }
                                        break;
                                }                               
                                break;
                            case CameraMovementType.Crane :
                                switch (movement.Directive)
                                {
                                    case CameraMovementDirective.With:
                                        break;
                                    case CameraMovementDirective.To:
                                        cameraActionQueue.Add(new Translate(fragment.StartTime, fragment.EndTime, cameraName,
                                                                            Vector3.zero, new Vector3Nullable(null, float.Parse(movement.Subject), null), true));
                                        break;
                                }
                                break;
                            case CameraMovementType.Pan:
                                switch (movement.Directive)
                                {
                                    case CameraMovementDirective.With:
                                        cameraActionQueue.Add(new RotateRelative(movement.Subject, fragment.StartTime, fragment.EndTime, cameraName,
                                                                                 true, false, true));
                                        break;
                                    case CameraMovementDirective.To:
                                        cameraActionQueue.Add(new Rotate(fragment.StartTime, fragment.EndTime, cameraName, float.Parse(movement.Subject)));
                                        break;
                                }
                                break;
                        }
                    }


                    float rotation = 0f;
                    if (fragment.Framings.Count > 0 && float.TryParse(fragment.Framings[0].FramingTarget, out rotation))
                    {
                        cameraActionQueue.Add(new Rotate(fragment.StartTime, fragment.StartTime, cameraName, rotation));
                    }
                    else
                    {
                        //TODO handle calculating actor target facing
                    }



                    // Lens change
                    int lens = lensMMtoIndex(fragment.Lens);
                    if (lens < 0)
                        Debug.Log("Lens does not exist");
                    else
                        cameraActionQueue.Add(new LensChange(fragment.StartTime, fragment.StartTime, "Main Camera", lens));

                    // FStop change
                    int fstop = fStopToIndex(fragment.FStop);
                    if (lens < 0)
                        Debug.Log("Lens does not exist");
                    else
                        cameraActionQueue.Add(new FStop(fragment.StartTime, fragment.EndTime, "Main Camera", fstop));


                    // Focus Change
                    cameraActionQueue.Add(new Focus(fragment.StartTime, fragment.EndTime, "Main Camera", fragment.FocusPosition));

                    // Shake it off
                    cameraActionQueue.Add(new Shake(fragment.StartTime, fragment.EndTime, "Main Camera", fragment.Shake));
                }
            }
        }

        private static int lensMMtoIndex(int lensNum)
        {
            if (lensNum == 12) return 0;
            if (lensNum == 14) return 1;
            if (lensNum == 16) return 2;
            if (lensNum == 18) return 3;
            if (lensNum == 21) return 4;
            if (lensNum == 25) return 5;
            if (lensNum == 27) return 6;
            if (lensNum == 32) return 7;
            if (lensNum == 35) return 8;
            if (lensNum == 40) return 9;
            if (lensNum == 50) return 10;
            if (lensNum == 65) return 11;
            if (lensNum == 75) return 12;
            if (lensNum == 100) return 13;
            if (lensNum == 135) return 14;
            if (lensNum == 150) return 15;
            if (lensNum == 180) return 16;
            else
            {
                return -1;
            }
        }

        private static int fStopToIndex(float fstop)
        {
            if (fstop == 1.4) return 0;
            if (fstop == 2) return 1;
            if (fstop == 2.8) return 2;
            if (fstop == 4) return 3;
            if (fstop == 5.6) return 4;
            if (fstop == 8) return 5;
            if (fstop == 11) return 6;
            if (fstop == 16) return 7;
            if (fstop == 22) return 8;
     
            else
            {
                return -1;
            }
        }

    }
}
