using UnityEngine;
using System.Collections;
using Impulse.v_0_1;
using ImpulsePlan = Impulse.v_0_1.Plan;
using System.IO;
using CinematicModel.ModelStructure;

namespace Assets.scripts{
    public class ActorActionFactory  {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="storyPlanName">this is the text in the name attribute of the 
        /// story plan to load...this sucks</param>
        /// <param name="cinematicModel"></param>
        /// <returns></returns>
        public static ActorActionQueue CreateStoryActions(string storyPlanName, string cinematicModel)
        {
            ActorActionQueue aaq = new ActorActionQueue();

            //load impulse
            string storyPlanXml = File.ReadAllText(storyPlanName + ".xml");
            ImpulsePlan.LoadFromString(storyPlanName);
            ImpulsePlan storyPlan = ImpulsePlan.GetPlan(storyPlanName);
            
            //load cinematic model
            CinematicModel cm = Parser.Parse(cinematicModel);
            
            //generate some actions
            return aaq;
        }

    }
}
