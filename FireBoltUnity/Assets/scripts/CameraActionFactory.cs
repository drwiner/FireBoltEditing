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
        private static FireBoltActionList cameraActionQueue;
        private static readonly string cameraName = "Pro Cam";
        private static readonly string cameraRig = "Rig";
        public static Dictionary<string, ushort> lenses = new Dictionary<string, ushort>() 
        { 
            {"12mm",0}, {"14mm",1}, {"16mm",2}, {"18mm",3}, {"21mm",4}, {"25mm",5}, {"27mm",6}, 
            {"32mm",7}, {"35mm",8}, {"40mm",9}, {"50mm",10}, {"65mm",11}, {"75mm",12}, {"100mm",13},
            {"135mm",14}, {"150mm",15}, {"180mm",16}
        };

        public static Dictionary<string, ushort> fStops = new Dictionary<string, ushort>()
        {
            {"1.4",0}, {"2",1}, {"2.8",2}, {"4",3}, {"5.6",4}, {"8",5}, {"11",6}, {"16",7}, {"22",8}
        };

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
                                                            cameraRig, Vector3.zero, new Vector3Nullable(futurePosition.x,null,futurePosition.z), true, true));

                    }
                    //the world is not ready for framings
                    //else if(fragment.Framings[0] != null &&
                    //        fragment.Framings[0].FramingType != FramingType.None && 
                    //        fragment.Framings[0].FramingType != FramingType.Angle)
                    //{
                    //    //TODO extend to support multiple framings when caclulating
                    //    //defer calculations to execution time....
                    //    Translate t = new Translate(fragment.StartTime, fragment.StartTime, cameraName, Vector3.zero, new Vector3Nullable(0,0,0), true);//translate stub to fill in at frame init
                    //    RotateRelative r = new RotateRelative(fragment.Framings[0].FramingTarget, fragment.StartTime, fragment.StartTime, cameraName, true, false, true); //rotate stub to fill in at frame init
                    //    cameraActionQueue.Add(new Frame(fragment.StartTime, fragment.StartTime, cameraName, fragment.Framings, t, r));
                    //    cameraActionQueue.Add(r);
                    //    cameraActionQueue.Add(t);
                    //}


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
                            case CameraMovementType.Tilt:
                                switch(movement.Directive)
                                {
                                    case CameraMovementDirective.With: // will this co-execute with pan-with?
                                        cameraActionQueue.Add(new RotateRelative(movement.Subject, fragment.StartTime, fragment.EndTime, cameraName,
                                                                                 false, true, false));
                                        break;
                                    case CameraMovementDirective.To:
                                        break;
                                }
                                break;
                            case CameraMovementType.Focus:
                                switch (movement.Directive)
                                {
                                    case CameraMovementDirective.With:

                                        break;
                                }
                                break;
                        }
                    }


                    
                    if (fragment.Framings.Count > 0 && fragment.Framings[0].FramingType == FramingType.Angle)
                    {
                        float rotation = 0f;
                        if(float.TryParse(fragment.Framings[0].FramingTarget, out rotation))
                            cameraActionQueue.Add(new Rotate(fragment.StartTime, fragment.StartTime, cameraName, rotation));
                    }
                    else
                    {
                        //TODO handle calculating actor target facing
                    }

                    // Lens change
                    ushort lensNumber;
                    if (lenses.TryGetValue(fragment.Lens, out lensNumber))
                    {
                        cameraActionQueue.Add(new LensChange(fragment.StartTime, fragment.EndTime, cameraName, lensNumber));
                    }
                    else
                    {
                        Debug.LogError(string.Format("lens [{0}] for cameraPlan interval [{1}-{2}] is invalid",fragment.Lens,fragment.StartTime,fragment.EndTime));
                    }
                        

                    // FStop change
                    ushort fStopNumber;
                    if (fStops.TryGetValue(fragment.FStop, out fStopNumber))
                    {
                        cameraActionQueue.Add(new FStop(fragment.StartTime, fragment.EndTime, cameraName, fStopNumber));
                    }
                    else
                    {
                        Debug.LogError(string.Format("f-stop [{0}] for cameraPlan interval [{1}-{2}] is invalid", fragment.FStop, fragment.StartTime, fragment.EndTime));
                    }                      

                    // Focus Change
                    cameraActionQueue.Add(new Focus(fragment.StartTime, fragment.EndTime, cameraName, fragment.FocusPosition));

                    // Shake it off
                    if(fragment.Shake > 0)
                        cameraActionQueue.Add(new Shake(fragment.StartTime, fragment.EndTime, cameraName, fragment.Shake));
                }
            }
        }




    }
}
