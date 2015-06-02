using UnityEngine;
using System.Collections;
using Impulse.v_0_1;
using Impulse.v_1_336;
using Impulse.v_1_336.Sentences;
using Impulse.v_1_336.Intervals;
using Impulse.v_1_336.Constants;
using UintT = Impulse.v_1_336.Interval<Impulse.v_1_336.Constants.ValueConstant<uint>, uint>;
using UintV = Impulse.v_1_336.Constants.ValueConstant<uint>;

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
        private static AStory<Impulse.v_1_336.Constants.ValueConstant<uint>, UintT,
                                    IIntervalSet<Impulse.v_1_336.Constants.ValueConstant<uint>,
                                    UintT>> story;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storyPlanPath">path to the story plan to load</param>
        /// <param name="cinematicModelPath">path to the cinematic model to load</param>
        /// <returns></returns>
        public static ActorActionQueue CreateStoryActions(string storyPlanPath, string cinematicModelPath)
        {
            ActorActionQueue aaq = new ActorActionQueue();            
            loadStructuredImpulsePlan(storyPlanPath);                    
            cm = loadCinematicModel(cinematicModelPath);

            extractInitialState();

            //generate some actions for the steps
            foreach(IStoryAction<UintT> action in story.Actions.Values)
            {
                //check for domain action the cinematic model knows about
                CM.DomainAction domainAction = getStoryDomainAction(action);
                if(domainAction == null) continue;

                CM.Animation effectingAnimation = getEffectingAnimation(action, domainAction);
                ////currently only using timing offsets in animations
                enqueueCreateActions(action, domainAction, effectingAnimation, aaq);
                //enqueueAnimateActions(step, domainAction, effectingAnimation, aaq);
                //enqueueDestroyActions(step, domainAction, effectingAnimation, aaq);
                //enqueueMoveActions(step, domainAction, effectingAnimation, aaq);
                //enqueueRotateActions(step, domainAction, effectingAnimation, aaq);
            
            }
            
            return aaq;
        }



        private static void extractInitialState()
        {
            var interval = new UintT(new UintV(0), new UintV(1));
            var query = from sentence in story.Sentences
                        where sentence is Predicate
                        let p = (Predicate) sentence
                        where story.IntervalSet.IncludesOrMeetsStartOf<UintV, UintT>((UintT)p.Time, interval) && 
                              p.Name == "at" && 
                              p.Temporal &&
                              p.Time is UintT && 
                              p.Terms[0] is IConstant && 
                              p.Terms[1] is IConstant<Coordinate2D>
                        select new { Actor = p.Terms[0].Name, Location = (p.Terms[1] as IConstant<Coordinate2D>).Value };
            foreach (var initPos in query)
            {
                Debug.Log(initPos.Actor + ", " + initPos.Location.ToString());
            }


        }

        private static void enqueueRotateActions(StructuredStep step, CM.DomainAction domainAction, 
                                                 CM.Animation effectingAnimation, ActorActionQueue aaq)
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
                    //look up with tryGetProperty of AStoryAction<...>
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
                            Debug.LogError("destination not set for stepId[" + step.ID + "]");
                        }
                        //TODO validate string format
                        destination = destinationString.ParseVector3();
                    }
                }
                startTick += getEffectorAnimationOffset(effectingAnimation, ra);
                endTick = ra.MaxDuration.HasValue ? startTick + ra.MaxDuration.Value : endTick;
                aaq.Add(new Rotate(startTick, endTick, actorName, destination));
            }
        }

        private static void enqueueMoveActions(StructuredStep step, CM.DomainAction domainAction, 
                                               CM.Animation effectingAnimation, ActorActionQueue aaq)
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
                            Debug.LogError("destination not set for stepId[" + step.ID + "]");
                        }
                        //TODO validate string format
                        destination = destinationString.ParseVector3();
                    }
                }
                startTick += getEffectorAnimationOffset(effectingAnimation, ma);
                endTick = ma.MaxDuration.HasValue ? startTick + ma.MaxDuration.Value : endTick;
                aaq.Add(new Translate(startTick, endTick, actorName, destination));
            }
        }

        private static CM.Animation getEffectingAnimation(IStoryAction<UintT> step, CM.DomainAction domainAction)
        {
            //find effector if any
            CM.AnimateAction effectorAnimateAction = domainAction.AnimateActions.Find(x => x.Effector);
            //didn't find an effector for this domain action...move along; nothing to see here
            if (effectorAnimateAction == null) return null;

            string effectorActorName = null;
            CM.AnimationMapping effectorAnimationMapping = null;
            CM.Animation effectingAnimation = null;
            foreach (CM.DomainActionParameter domainActionParameter in domainAction.Params)
            {
                if (domainActionParameter.Name == effectorAnimateAction.ActorNameParamName)
                {
                    //TODO finish converting to impulse 1.336
                    
                    effectorActorName = (from xImpulseStepParam in step.Parameters
                                 where xImpulseStepParam.Name == domainActionParameter.Name
                                 select xImpulseStepParam.Value as string).FirstOrDefault();
                    if (effectorActorName == null)
                    {
                        Debug.LogError("actorName not set for stepId[" + step.ID + "]");
                        return null;
                    }
                    effectorAnimationMapping = cm.FindAnimationMapping(effectorActorName, effectorAnimateAction.Name);
                    if (effectorAnimationMapping == null)
                    {
                        Debug.Log("cinematic model animation instance undefined for actor[" +
                            effectorActorName + "] action[" + domainAction.Name + "] paramName[" + domainActionParameter.Name + "]");
                        return null;
                    }
                    effectingAnimation = cm.FindAnimation(effectorAnimationMapping.AnimationName);
                }
            }
            return effectingAnimation;
        }

        private static void enqueueAnimateActions(StructuredStep step, CM.DomainAction domainAction, CM.Animation effectingAnimation, ActorActionQueue aaq)
        {
            
            foreach(CM.AnimateAction aa in domainAction.AnimateActions)
            {
                string actorName = null;
                float startTick = 0;
                float endTick = 0;
                CM.AnimationMapping animMapping = null;
                CM.Animation animation = null;
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
                        animMapping = cm.FindAnimationMapping(actorName, aa.Name);
                        if (animMapping == null)
                        {
                            Debug.Log("cinematic model animation instance undefined for actor[" +
                                actorName + "] animateAction[" + aa.Name + "]");
                            return;
                        }
                        animation = cm.FindAnimation(animMapping.AnimationName);
                    }
                }
                startTick += getEffectorAnimationOffset(effectingAnimation, aa);
                endTick = aa.MaxDuration.HasValue ? startTick + aa.MaxDuration.Value : endTick; //TODO encode max duration into the endTick property
                aaq.Add(new AnimateMecanim(startTick, endTick, actorName, animation.FileName, animMapping.LoopAnimation));
            }
        }

        private static float getEffectorAnimationOffset(CM.Animation effectingAnimation, CM.FireBoltAction fireBoltAction)
        {
            float offset = 0;
            //now that we have our parameters filled out, we need to shore up the start and end points of the animations
            //relative to the effecting animation if there is one.
            if (effectingAnimation != null)
            {
                CM.AnimationIndex effectingIndex = effectingAnimation.AnimationIndices.Find(x => x.Name == fireBoltAction.EffectorOffsetIndexName);
                if (effectingIndex != null)
                {
                    offset = effectingIndex.TimeOffset;
                }

            }
            return offset;
        }

        private static void enqueueCreateActions(StructuredStep step, CM.DomainAction domainAction, CM.Animation effectingAnimation, ActorActionQueue aaq )
        {
            foreach (CM.CreateAction ca in domainAction.CreateActions)
            {
                float startTick = 0;
                string actorName = null;
                string modelName = null;
                Vector3 destination = new Vector3();
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
                    else if (domainActionParameter.Name == ca.OriginParamName)
                    {
                        string destinationString = (from xImpulseStepParam in step.Parameters
                                             where xImpulseStepParam.Name == domainActionParameter.Name
                                             select xImpulseStepParam.Value as string).FirstOrDefault();
                        if (destinationString == null)
                        {
                            Debug.LogError("destination not set for stepId[" + step.ID + "]");
                        }
                        //TODO validate string format
                        destination = destinationString.ParseVector3();
                    }
                }
                startTick += getEffectorAnimationOffset(effectingAnimation, ca);
                aaq.Add(new Create(startTick, actorName, modelName, destination));
            }
        }

        private static void enqueueDestroyActions(StructuredStep step, CM.DomainAction domainAction, 
                                                    CM.Animation effectingAnimation, ActorActionQueue aaq)
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
                startTick += getEffectorAnimationOffset(effectingAnimation, da);
                aaq.Add(new Destroy(startTick, actorName));
            }
        }

        private static CM.DomainAction getStoryDomainAction(IStoryAction action)
        {
            //check if the step action is in the domain of cinematic model
            foreach(CM.DomainAction domainAction in cm.DomainActions)
            {
                if(string.Equals(domainAction.Name,action.ActionType.Name,StringComparison.OrdinalIgnoreCase))
                {
                    return domainAction;
                }
            }
            return null;
        }

        private static void loadStructuredImpulsePlan(string storyPlanPath){            
            //try 
            //{ 
                var xml = Impulse.v_1_336.Xml.Story.LoadFromFile(storyPlanPath);
            //}
            //catch (Exception ex)
            //{
                //Debug.LogError("Exception loading impulse plan: " + ex.ToString());
            //}
                var factory = Impulse.v_1_336.StoryParsingFactories.GetUnsignedIntergerIntervalFactory();
                story = factory.ParseStory(xml, false);
        }

        private static CM.CinematicModel loadCinematicModel(string cinematicModelPath)
        {
            return CM.Parser.Parse(cinematicModelPath);
        }
    }
}
