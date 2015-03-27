using UnityEngine;
using System.Collections;
using Impulse.v_0_1;
using System.IO;
using CM = CinematicModel;
using System;
using System.Collections.Generic;

namespace Assets.scripts{
    public class ActorActionFactory  {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="storyPlanPath">path to the story plan to load</param>
        /// <param name="cinematicModelPath">path to the cinematic model to load</param>
        /// <returns></returns>
        public static ActorActionQueue CreateStoryActions(string storyPlanPath, string cinematicModelPath)
        {
            ActorActionQueue aaq = new ActorActionQueue();
            
            StructuredPlan storyPlan = loadStructuredImpulsePlan(storyPlanPath);                    
            CM.CinematicModel cm = loadCinematicModel(cinematicModelPath);
            
            //generate some actions for the steps
            foreach(StructuredStep step in storyPlan.Steps.Values)
            {
                Dictionary<string,string> parameterMap;
                ActionDecorator action;
                foreach(Parameter p in step.){

                }
                
                //check for enter action
                if(string.Equals(step.Name,"enter",StringComparison.OrdinalIgnoreCase))//this is a special case
                {
                   // action = new Create(step)
                }
                //create a Create
                //add it to the queue
            }
            
            return aaq;
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
