using UnityEngine;
using System.Collections;
using Impulse.v_0_1;
using System.IO;
using CM = CinematicModel;
using System;
using System.Collections.Generic;
using System.Linq;

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
                //check for domain action the cinematic model knows about
                CM.DomainAction domainAction = getStoryDomainAction(step);
                if(domainAction == null) continue;
                
                enqueueCreateActions(step, domainAction, aaq);
                enqueueAnimateActions(step, domainAction, aaq);
            
            }
            
            return aaq;
        }

        private static void enqueueAnimateActions(StructuredStep step, CM.DomainAction domainAction, ActorActionQueue aaq)
        {
            string actorName = null;
            float startTick=0;
            float? endTick = null;
            foreach(var param in step.Parameters)
            {
                if (string.Equals(param.Name, "actor", StringComparison.OrdinalIgnoreCase)) //TODO this belongs in an XSLT that's knowledge engineered for each domain
                {                    
                    actorName = (string)param.Value;
                }
                if (string.Equals(param.Name, "start-tick", StringComparison.OrdinalIgnoreCase)) //TODO this belongs in an XSLT that's knowledge engineered for each domain
                {                    
                    startTick = (int)param.Value;
                }
                if (string.Equals(param.Name, "end-tick", StringComparison.OrdinalIgnoreCase)) //TODO this belongs in an XSLT that's knowledge engineered for each domain
                {                    
                    endTick = (int)param.Value;
                }
            }
            if (actorName == null)
            {
                Debug.Log("story plan actorName not set for action[" + domainAction.Name + "]");
                return;
            }
            CM.AnimationInstance ai = cm.FindAnimationInstance(actorName, domainAction.Name, "actor");
            if(ai == null)
            {
                Debug.Log("cinematic model animation instance undefined for actor["+ actorName+"] action["+domainAction.Name+"] paramName[actor]");     
                return;
            }
            aaq.Add(new AnimateMecanim(startTick, endTick, actorName, ai.AnimationName));
        }

        private static void enqueueCreateActions(StructuredStep step, CM.DomainAction domainAction, ActorActionQueue aaq)
        {
            foreach (CM.CreateAction ca in domainAction.CreateActions)
            {
                float startTick = 0 ;
                string actorName = null;
                string modelName = null;
                foreach (CM.DomainActionParameter domainActionParameter in domainAction.Params) 
                {
                    if (domainActionParameter.Id == ca.StartTickParamId)
                    {
                        startTick = Convert.ToInt32((from xImpulseStepParam in step.Parameters //TODO a little sketchy on the type conversion here...what do i want to do with it?
                                                    where string.Equals(xImpulseStepParam.Name,domainActionParameter.Name,StringComparison.OrdinalIgnoreCase) 
                                                    select xImpulseStepParam.Value).FirstOrDefault());
                    }
                    else if (domainActionParameter.Id == ca.ActorNameParamId)
                    {
                        actorName = Convert.ToString((from xImpulseStepParam in step.Parameters //TODO a little sketchy on the type conversion here...what do i want to do with it?
                                                    where string.Equals(xImpulseStepParam.Name,domainActionParameter.Name,StringComparison.OrdinalIgnoreCase) 
                                                    select xImpulseStepParam.Value).FirstOrDefault());
                    }
                }
                Create create = new Create(startTick,actorName,modelName, new Vector3());
            }


            List<CM.Actor> createdObjects = cm.FindCreatedObjects(domainAction.Name);
            foreach(CM.Actor createdObject in createdObjects)
            {
                float startTick = 0;
                if (step.Has_int("start-tick"))
                {
                    startTick = step.Get_int("start-tick").Value;
                }
                aaq.Add(new Create(startTick, createdObject.Name, createdObject.Model, Vector3.zero));
            }
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
