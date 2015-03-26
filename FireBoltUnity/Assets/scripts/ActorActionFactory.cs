using UnityEngine;
using System.Collections;
using Impulse.v_0_1;
using ImpulsePlan = Impulse.v_0_1.Plan;
using System.IO;
using CM = CinematicModel;
using System;

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
            
            ImpulsePlan storyPlan = loadImpulsePlan(storyPlanPath);                    
            CM.CinematicModel cm = loadCinematicModel(cinematicModelPath);
            
            //generate some actions
            return aaq;
        }

        private static ImpulsePlan loadImpulsePlan(string storyPlanPath){
            
                string storyPlanXml = File.ReadAllText(storyPlanPath);
                string storyPlanName = Path.GetFileNameWithoutExtension(storyPlanPath);
                try { 
                    ImpulsePlan.LoadFromString(storyPlanXml);
                }
                catch (Exception e)
                {
                    Debug.Log("Exception loading impulse plan: " + e.Message);
                }
            return ImpulsePlan.GetPlan(storyPlanName);
        }

        private static CM.CinematicModel loadCinematicModel(string cinematicModelPath)
        {
            return CM.Parser.Parse(cinematicModelPath);
        }
    }
}
