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
                enqueueDestroyActions(step, domainAction, aaq);
                enqueueMoveActions(step, domainAction, aaq);
                enqueueRotateActions(step, domainAction, aaq);
            
            }
            
            return aaq;
        }

        private static void enqueueRotateActions(StructuredStep step, CM.DomainAction domainAction, ActorActionQueue aaq)
        {
            foreach (CM.RotateAction ra in domainAction.RotateActions)
            {
                float startTick = 0;
                float endTick = 0;
                string actorName = null;
                string destinationString = null;
                Vector3 destination = Vector3.zero;
                foreach (CM.DomainActionParameter domainActionParameter in domainAction.Params)
                {
                    if (domainActionParameter.Name == ra.StartTickParamName)
                    {
                        startTick = Convert.ToInt32((from xImpulseStepParam in step.Parameters
                                                     where xImpulseStepParam.Name == domainActionParameter.Name
                                                     select xImpulseStepParam.Value).FirstOrDefault());
                    }
                    else if (domainActionParameter.Name == ra.ActorNameParamName)
                    {
                        actorName = (from xImpulseStepParam in step.Parameters
                                     where xImpulseStepParam.Name == domainActionParameter.Name
                                     select xImpulseStepParam.Value as string).FirstOrDefault();
                        if (actorName == null)
                        {
                            Debug.LogError("actorName not set for stepId[" + step.ID + "]");
                        }
                    }
                    else if (domainActionParameter.Name == ra.EndTickParamName)
                    {
                        endTick = Convert.ToInt32((from xImpulseStepParam in step.Parameters
                                                   where xImpulseStepParam.Name == domainActionParameter.Name
                                                   select xImpulseStepParam.Value).FirstOrDefault());
                        if (endTick < .001)
                        {
                            Debug.LogError("endTick not set or 0 for stepId[" + step.ID + "]");
                        }
                    }
                    else if (domainActionParameter.Name == ra.DestinationParamName)
                    {
                        destinationString = (from xImpulseStepParam in step.Parameters
                                             where xImpulseStepParam.Name == domainActionParameter.Name
                                             select xImpulseStepParam.Value as string).FirstOrDefault();
                        if (destinationString == null)
                        {
                            Debug.LogError("destination not set or 0 for stepId[" + step.ID + "]");
                        }
                        //TODO validate string format
                        destination = destinationString.ParseVector3();
                    }
                }
                endTick = ra.MaxDuration.HasValue ? startTick + ra.MaxDuration.Value : endTick;
                aaq.Add(new Rotate(startTick, endTick, actorName, destination));
            }
        }

        private static void enqueueMoveActions(StructuredStep step, CM.DomainAction domainAction, ActorActionQueue aaq)
        {
            foreach (CM.MoveAction ma in domainAction.MoveActions)
            {
                float startTick = 0;
                float endTick = 0;
                string actorName = null;
                string destinationString = null;
                Vector3 destination = Vector3.zero;
                foreach (CM.DomainActionParameter domainActionParameter in domainAction.Params)
                {
                    if (domainActionParameter.Name == ma.StartTickParamName)
                    {
                        startTick = Convert.ToInt32((from xImpulseStepParam in step.Parameters
                                                     where xImpulseStepParam.Name == domainActionParameter.Name
                                                     select xImpulseStepParam.Value).FirstOrDefault());
                    }
                    else if (domainActionParameter.Name == ma.ActorNameParamName)
                    {
                        actorName = (from xImpulseStepParam in step.Parameters
                                     where xImpulseStepParam.Name == domainActionParameter.Name
                                     select xImpulseStepParam.Value as string).FirstOrDefault();
                        if (actorName == null)
                        {
                            Debug.LogError("actorName not set for stepId[" + step.ID + "]");
                        }                        
                    }
                    else if(domainActionParameter.Name == ma.EndTickParamName)
                    {
                        endTick = Convert.ToInt32((from xImpulseStepParam in step.Parameters
                                                   where xImpulseStepParam.Name == domainActionParameter.Name
                                                   select xImpulseStepParam.Value).FirstOrDefault());
                        if (endTick < .001)
                        {
                            Debug.LogError("endTick not set or 0 for stepId[" + step.ID + "]");
                        }
                    }
                    else if(domainActionParameter.Name == ma.DestinationParamName)
                    {
                        destinationString = (from xImpulseStepParam in step.Parameters
                                       where xImpulseStepParam.Name == domainActionParameter.Name
                                       select xImpulseStepParam.Value as string).FirstOrDefault();
                        if(destinationString == null)
                        {
                            Debug.LogError("destination not set or 0 for stepId[" + step.ID + "]");
                        }
                        //TODO validate string format
                        destination = destinationString.ParseVector3();
                    }
                }
                endTick = ma.MaxDuration.HasValue ? startTick + ma.MaxDuration.Value : endTick;
                aaq.Add(new Translate(startTick, endTick, actorName, destination));
            }
        }



        private static void enqueueAnimateActions(StructuredStep step, CM.DomainAction domainAction, ActorActionQueue aaq)
        {
            foreach(CM.AnimateAction aa in domainAction.AnimateActions)
            {
                string actorName = null;
                float startTick = 0;
                float endTick = 0;
                CM.AnimationInstance ai = null;
                foreach(CM.DomainActionParameter domainActionParameter in domainAction.Params)
                {
                    if (domainActionParameter.Name == aa.StartTickParamName)
                    {
                        startTick = Convert.ToInt32((from xImpulseStepParam in step.Parameters
                                                     where xImpulseStepParam.Name == domainActionParameter.Name
                                                     select xImpulseStepParam.Value).FirstOrDefault());
                    }
                    else if (domainActionParameter.Name == aa.EndTickParamName)
                    {
                        endTick = Convert.ToInt32((from xImpulseStepParam in step.Parameters
                                                   where xImpulseStepParam.Name == domainActionParameter.Name
                                                   select xImpulseStepParam.Value).FirstOrDefault());
                        if (endTick < .001)
                        {
                            Debug.LogError("endTick not set or 0 for stepId[" + step.ID + "]");
                        }
                    }
                    else if (domainActionParameter.Name == aa.ActorNameParamName)
                    {
                        actorName = (from xImpulseStepParam in step.Parameters
                                     where xImpulseStepParam.Name == domainActionParameter.Name
                                     select xImpulseStepParam.Value as string).FirstOrDefault();
                        if (actorName == null)
                        {
                            Debug.LogError("actorName not set for stepId[" + step.ID + "]");
                            return;
                        }
                        else
                        {
                            ai = cm.FindAnimationInstance(actorName, domainAction.Name, domainActionParameter.Name);
                            if (ai == null)
                            {
                                Debug.Log("cinematic model animation instance undefined for actor[" + 
                                    actorName + "] action[" + domainAction.Name + "] paramName[" + domainActionParameter.Name + "]");
                                return;
                            }
                        }
                    }
                }
                endTick = aa.MaxDuration.HasValue ? startTick + aa.MaxDuration.Value : endTick;
                aaq.Add(new AnimateMecanim(startTick, endTick, actorName, ai.AnimationName, ai.LoopAnimation));
            }
        }

        private static void enqueueCreateActions(StructuredStep step, CM.DomainAction domainAction, ActorActionQueue aaq)
        {
            foreach (CM.CreateAction ca in domainAction.CreateActions)
            {
                float startTick = 0;
                string actorName = null;
                string modelName = null;
                foreach (CM.DomainActionParameter domainActionParameter in domainAction.Params) 
                {
                    if (domainActionParameter.Name == ca.StartTickParamName)
                    {
                        startTick = Convert.ToInt32((from xImpulseStepParam in step.Parameters 
                                                    where xImpulseStepParam.Name == domainActionParameter.Name
                                                    select xImpulseStepParam.Value).FirstOrDefault());                        
                    }
                    else if (domainActionParameter.Name == ca.ActorNameParamName)
                    {
                        actorName = (from xImpulseStepParam in step.Parameters 
                                                    where xImpulseStepParam.Name == domainActionParameter.Name
                                                    select xImpulseStepParam.Value as string).FirstOrDefault();
                        if (actorName == null)
                        {
                            Debug.Log("actorName not set for stepId[" + step.ID + "]");
                        }
                        else //actorName is defined, we can look up a model
                        {
                            modelName = (from actor in cm.Actors 
                                             where string.Equals(actor.Name,actorName,StringComparison.OrdinalIgnoreCase)
                                             select actor.Model).FirstOrDefault<string>();
                            if(modelName == null)
                            {
                                Debug.Log("model name for actor[" + actorName + "] not found in cinematic model.");
                            }
                        }
                    }
                }
                aaq.Add(new Create(startTick,actorName,modelName, new Vector3()));
            }
        }

        private static void enqueueDestroyActions(StructuredStep step, CM.DomainAction domainAction, ActorActionQueue aaq)
        {
            foreach (CM.DestroyAction da in domainAction.DestroyActions)
            {
                float startTick = 0;
                string actorName = null;
                foreach (CM.DomainActionParameter domainActionParameter in domainAction.Params)
                {
                    if (domainActionParameter.Name == da.StartTickParamName)
                    {
                        startTick = Convert.ToInt32((from xImpulseStepParam in step.Parameters
                                                     where xImpulseStepParam.Name == domainActionParameter.Name
                                                     select xImpulseStepParam.Value).FirstOrDefault());
                    }
                    else if (domainActionParameter.Name == da.ActorNameParamName)
                    {
                        actorName = (from xImpulseStepParam in step.Parameters
                                     where xImpulseStepParam.Name == domainActionParameter.Name
                                     select xImpulseStepParam.Value as string).FirstOrDefault();
                        if (actorName == null)
                        {
                            Debug.Log("actorName not set for stepId[" + step.ID + "]");
                        }
                    }
                }
                aaq.Add(new Destroy(startTick, actorName));
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
