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
        private static readonly string cameraName = "Pro Cam";
        private static readonly string cameraRig = "Rig";
        public static uint endDiscourseTime = 0;
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

        public static DiscourseActionList CreateCameraActions(AStory<UintV, UintT, IIntervalSet<UintV, UintT>> story, string cameraPlanPath)
        {
            DiscourseActionList discourseActionList = new DiscourseActionList();
            CameraPlan cameraPlan = Parser.Parse(cameraPlanPath);
            enqueueCameraActions(cameraPlan, discourseActionList);
            return discourseActionList;
        }

        private static void enqueueCameraActions(CameraPlan cameraPlan, DiscourseActionList discourseActionList)
        {
            uint currentDiscourseTime = 0;
            float previousStoryTimeOffset = 0;
            foreach (Block block in cameraPlan.Blocks)
            {
                float blockStartTime = Single.MaxValue;
                float blockEndTime = Single.MinValue;
                foreach (var fragment in block.ShotFragments)
                {
                    uint fragmentStartTime = currentDiscourseTime++;
                    uint fragmentEndTime = fragmentStartTime + fragment.Duration;
                    
                    if (fragmentStartTime < blockStartTime) //assumes same time scale for discourse and story
                        blockStartTime = fragmentStartTime;                                     
                    if (fragmentEndTime > blockEndTime)
                        blockEndTime = fragmentEndTime;

                    discourseActionList.Add(new ShotFragmentInit(fragmentStartTime, fragmentEndTime, 
                        cameraRig, fragment.Anchor, fragment.Height, fragment.Lens, fragment.FStop, 
                        fragment.Angle, fragment.FocusPosition));

                    //Vector2 futurePosition;
                    //if (fragment.Anchor.TryParsePlanarCoords(out futurePosition))
                    //{
                    //    discourseActionList.Add(new Translate(fragmentStartTime, fragmentStartTime,
                    //                                        cameraRig, Vector3.zero, new Vector3Nullable(futurePosition.x,null,futurePosition.y), true));

                    //    // Check if the fragment has a low, medium, or high angle defined.
                    //    if (!fragment.Angle.IsNull()) 
                    //        // If so, add a new angle object to the action list.
                    //        discourseActionList.Add(new Angle(fragmentStartTime, fragmentStartTime, cameraRig, 
                    //                                    new Vector2(futurePosition.x, futurePosition.y), fragment.Angle.Target, fragment.Angle.AngleSetting));
                    //}

                    //the world is not ready for framings
                    //else if(fragment.Framings[0] != null &&
                    //        fragment.Framings[0].FramingType != FramingType.None && 
                    //        fragment.Framings[0].FramingType != FramingType.Angle)
                    //{
                    //    //TODO extend to support multiple framings when caclulating
                    //    //defer calculations to execution time....
                    //    Translate t = new Translate(fragmentStartTime, fragmentStartTime, cameraName, Vector3.zero, new Vector3Nullable(0,0,0), true);//translate stub to fill in at frame init
                    //    RotateRelative r = new RotateRelative(fragment.Framings[0].FramingTarget, fragmentStartTime, fragmentStartTime, cameraName, true, false, true); //rotate stub to fill in at frame init
                    //    cameraActionQueue.Add(new Frame(fragmentStartTime, fragmentStartTime, cameraName, fragment.Framings, t, r));
                    //    cameraActionQueue.Add(r);
                    //    cameraActionQueue.Add(t);
                    //}

                    foreach (var movement in fragment.CameraMovements)
                    {
                        switch (movement.Type) //TODO why do some movements use cameraName directly and some use rig?  rig should be the thing we use always
                        {
                            case CameraMovementType.Dolly :
                                switch (movement.Directive)
                                {
                                    case(CameraMovementDirective.With):
                                        discourseActionList.Add(new TranslateRelative(movement.Subject, fragmentStartTime, fragmentEndTime, cameraName, false, true, false));
                                        break;
                                    case(CameraMovementDirective.To):
                                        Vector2 destination;
                                        if (movement.Subject.TryParsePlanarCoords(out destination))
                                        {
                                            discourseActionList.Add(new Translate(fragmentStartTime, fragmentEndTime, cameraName,
                                                                                Vector3.zero, new Vector3Nullable(destination.x,null,destination.y),true));
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
                                        discourseActionList.Add(new Translate(fragmentStartTime, fragmentEndTime, cameraName,
                                                                            Vector3.zero, new Vector3Nullable(null, float.Parse(movement.Subject), null), true));
                                        break;
                                }
                                break;
                            case CameraMovementType.Pan:
                                switch (movement.Directive)
                                {
                                    case CameraMovementDirective.With:
                                        discourseActionList.Add(new RotateRelative(movement.Subject, fragmentStartTime, fragmentEndTime, cameraRig,
                                                                                 true, false, true));
                                        break;
                                    case CameraMovementDirective.To:
                                        discourseActionList.Add(new Rotate(fragmentStartTime, fragmentEndTime, cameraName, float.Parse(movement.Subject)));
                                        break;
                                }
                                break;
                            case CameraMovementType.Tilt:
                                switch(movement.Directive)
                                {
                                    case CameraMovementDirective.With: // will this co-execute with pan-with? not currently
                                        discourseActionList.Add(new RotateRelative(movement.Subject, fragmentStartTime, fragmentEndTime, cameraRig,
                                                                                 false, true, true));
                                        break;
                                    case CameraMovementDirective.To:
                                        //cameraActionQueue.Add(new Rotate(fragmentStartTime, fragmentEndTime, ))
                                        break;
                                }
                                break;
                            case CameraMovementType.Focus:
                                switch (movement.Directive)
                                {
                                    case CameraMovementDirective.With:
                                        discourseActionList.Add(new Focus(fragmentStartTime, fragmentEndTime, cameraName, movement.Subject, true));
                                        break;
                                }
                                break;
                        }
                    }


                    
                    //if (fragment.Framings.Count > 0 && fragment.Framings[0].FramingType == FramingType.Angle)
                    //{
                    //    float rotation = 0f;
                    //    if(float.TryParse(fragment.Framings[0].FramingTarget, out rotation))
                    //        discourseActionList.Add(new Rotate(fragmentStartTime, fragmentStartTime, cameraName, rotation));
                    //}
                    //else
                    //{
                    //    //TODO handle calculating actor target facing
                    //}

                    //// Lens change
                    //ushort lensNumber;
                    //if (lenses.TryGetValue(fragment.Lens, out lensNumber))
                    //{
                    //    discourseActionList.Add(new LensChange(fragmentStartTime, fragmentEndTime, cameraName, lensNumber));
                    //}
                    //else
                    //{
                    //    Debug.LogError(string.Format("lens [{0}] for cameraPlan interval [{1}-{2}] is invalid",fragment.Lens,fragmentStartTime,fragmentEndTime));
                    //}
                        

                    //// FStop change
                    //ushort fStopNumber;
                    //if (fStops.TryGetValue(fragment.FStop, out fStopNumber))
                    //{
                    //    discourseActionList.Add(new FStop(fragmentStartTime, fragmentEndTime, cameraName, fStopNumber));
                    //}
                    //else
                    //{
                    //    Debug.LogError(string.Format("f-stop [{0}] for cameraPlan interval [{1}-{2}] is invalid", fragment.FStop, fragmentStartTime, fragmentEndTime));
                    //}                      

                    //// Focus Change
                    //if(fragment.FocusPosition !=null)
                    //    discourseActionList.Add(new Focus(fragmentStartTime, fragmentEndTime, cameraName, fragment.FocusPosition));

                    //// Shake it off
                    //discourseActionList.Add(new Shake(fragmentStartTime, fragmentEndTime, cameraName, fragment.Shake));

                    currentDiscourseTime = fragmentEndTime;
                }
                if (block.StoryTime.HasValue)
                {
                    float currentStoryTimeOffset = block.StoryTime.Value - blockStartTime;
                    discourseActionList.Add(new SetStoryTime(currentStoryTimeOffset, previousStoryTimeOffset, blockStartTime, blockEndTime));
                    previousStoryTimeOffset = block.StoryTime.Value;
                }
            }
            discourseActionList.EndDiscourseTime = currentDiscourseTime;
        }




    }
}
