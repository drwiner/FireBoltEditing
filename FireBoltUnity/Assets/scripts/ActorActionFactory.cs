using UnityEngine;
using System.Collections;
using Impulse.v_0_1;
using System.IO;
using CM = CinematicModel;
using System;
using System.Collections.Generic;

namespace Assets.scripts
{
    public class ActorActionFactory
    {
        private static CM.CinematicModel cm;
        private static StructuredPlan storyPlan;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storyPlanPath">path to the story plan to load</param>
        /// <param name="cinematicModelPath">path to the cinematic model to load</param>
        /// <returns></returns>
        public static ActorActionQueue CreateStoryActions(string storyPlanPath, string cinematicModelPath)
        {
            ActorActionQueue aaq = new ActorActionQueue();            
            storyPlan = loadStructuredImpulsePlan(storyPlanPath);                    
            cm = loadCinematicModel(cinematicModelPath);
            
            //generate some actions for the steps
            foreach(StructuredStep step in storyPlan.Steps.Values)
            {
                ActionDecorator action = null;

                //check for domain action the cinematic model knows about
                CM.DomainAction domainAction = getStoryDomainAction(step);
                if(domainAction == null) continue;
                
                //check if it made something new in the world
                List<CM.Actor> createdObjects  = cm.FindCreatedObjects(domainAction.Name);
                if(createdObjects.Count > 0)
                {
                    //need to create everything in the list

                    float startTick = 0;
                    if(step.Has_int("start-tick")){//abstractify this "start-tick" to extract from plan somehow?  seems hard for little gain.  plans should have ticks named as ticks
                        startTick = step.Get_int("start-tick").Value;
                    }
                    
                    action = new Create(startTick,createdObjects[0].Name,createdObjects[0].Model,Vector3.zero);
                }
                if (action != null)
                {
                    //add it to the queue
                    aaq.Add(action);
                }
            }
            
            return aaq;
        }

        private static CM.DomainAction getStoryDomainAction(StructuredStep step)
        {
            CM.DomainAction matchedAction = null;
            //check if the step action is in the domain of cinematic model
            foreach(CM.DomainAction  domainAction in cm.DomainActions)
            {
                if(string.Equals(domainAction.Name,step.Name,StringComparison.OrdinalIgnoreCase))
                {
                    matchedAction = domainAction;
                }
            }
            return matchedAction;
        }

        private static StructuredPlan loadStructuredImpulsePlan(string storyPlanPath){
            string storyPlanName = Path.GetFileNameWithoutExtension(storyPlanPath);
            try 
            { 
                Impulse.v_0_1.StructuredPlan.LoadFromPlan(Impulse.v_0_1.Plan.Load(storyPlanPath));

            }catch (Exception e){
                Debug.Log("Exception loading impulse plan: " + e.Message);
            }
            return Impulse.v_0_1.StructuredPlan.CurrentPlan;
        }

        private static CM.CinematicModel loadCinematicModel(string cinematicModelPath)
        {
            return CM.Parser.Parse(cinematicModelPath);
        }
    }
}
