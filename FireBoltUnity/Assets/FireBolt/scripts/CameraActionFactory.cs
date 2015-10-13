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

                    discourseActionList.Add(new ShotFragmentInit(fragmentStartTime, fragmentEndTime, cameraRig, fragment.Anchor, fragment.Height,
                        fragment.Lens, fragment.FStop, fragment.Framings, fragment.Direction, fragment.Angle, fragment.FocusPosition));

                    float movementStartTime = fragmentStartTime + 1; //force moves to sort after inits
                    foreach (var movement in fragment.CameraMovements)
                    {
                        switch (movement.Type) 
                        {
                            case CameraMovementType.Dolly :
                                switch (movement.Directive)
                                {
                                    case(CameraMovementDirective.With):
                                        discourseActionList.Add(new TranslateRelative(movement.Subject, movementStartTime, fragmentEndTime, cameraRig, false, true, false));
                                        break;
                                    case(CameraMovementDirective.To):
                                        Vector2 destination;
                                        if (movement.Subject.TryParsePlanarCoords(out destination))
                                        {
                                            discourseActionList.Add(new Translate(movementStartTime, fragmentEndTime, cameraRig,
                                                                                Vector3.zero, new Vector3Nullable(destination.x,null,destination.y)));
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
                                        discourseActionList.Add(new Translate(movementStartTime, fragmentEndTime, cameraRig,
                                                                            Vector3.zero, new Vector3Nullable(null, float.Parse(movement.Subject), null)));
                                        break;
                                }
                                break;
                            case CameraMovementType.Pan:
                                switch (movement.Directive)
                                {
                                    case CameraMovementDirective.With:
                                        discourseActionList.Add(new RotateRelative(movement.Subject, movementStartTime, fragmentEndTime, cameraRig,
                                                                                 true, false, true));
                                        break;
                                    case CameraMovementDirective.To:
                                        discourseActionList.Add(new Rotate(movementStartTime, fragmentEndTime, cameraRig, new Vector3Nullable(null, float.Parse(movement.Subject), null)));
                                        break;
                                }
                                break;
                            case CameraMovementType.Tilt:
                                switch(movement.Directive)
                                {
                                    case CameraMovementDirective.With: // will this co-execute with pan-with? not currently
                                        discourseActionList.Add(new RotateRelative(movement.Subject, movementStartTime, fragmentEndTime, cameraRig,
                                                                                 false, true, true));
                                        break;
                                    case CameraMovementDirective.To:
                                        discourseActionList.Add(new Rotate(movementStartTime, fragmentEndTime, cameraRig, new Vector3Nullable(float.Parse(movement.Subject), null, null)));
                                        break;
                                }
                                break;
                            case CameraMovementType.Focus:
                                switch (movement.Directive)
                                {
                                    case CameraMovementDirective.With:
                                        discourseActionList.Add(new Focus(movementStartTime, fragmentEndTime, cameraName, movement.Subject, true));
                                        break;
                                }
                                break;
                        }
                    }
                    // Shake it off
                    discourseActionList.Add(new Shake(movementStartTime, fragmentEndTime, cameraName, fragment.Shake));

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
