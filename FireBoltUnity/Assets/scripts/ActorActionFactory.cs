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
        //TODO add support for assigning full numeric extended name to duplicate actors (creeps) so we can move and animate them uniquely
        //TODO speedify by adding caching to lookup methods
        private static CM.CinematicModel cm;
        private static AStory<UintV, UintT, IIntervalSet<UintV, UintT>> story;
        private static readonly char[] uniqueActorIdentifierSeparators = { ' ' };

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

            buildInitialState(aaq);

            //generate FireBolt actions for the steps
            foreach(IStoryAction<UintT> storyAction in story.Actions.Values)
            {
                CM.DomainAction domainAction = getStoryDomainAction(storyAction);
                if(domainAction == null) continue;

                CM.Animation effectingAnimation = getEffectingAnimation(storyAction, domainAction);

                enqueueCreateActions(storyAction, domainAction, effectingAnimation, aaq);
                //enqueueAnimateActions(step, domainAction, effectingAnimation, aaq);
                //enqueueDestroyActions(step, domainAction, effectingAnimation, aaq);
                //enqueueMoveActions(step, domainAction, effectingAnimation, aaq);
                //enqueueRotateActions(step, domainAction, effectingAnimation, aaq);
            
            }
            
            return aaq;
        }



        private static void buildInitialState(ActorActionQueue aaq)
        {
            var interval = new UintT(new UintV(0), new UintV(1));
            var initialPositions = from sentence in story.Sentences
                        where sentence is Predicate
                        let p = (Predicate) sentence
                        where story.IntervalSet.IncludesOrMeetsStartOf<UintV, UintT>((UintT)p.Time, interval) && 
                              p.Name == "at" && 
                              p.Temporal &&
                              p.Time is UintT && 
                              p.Terms[0] is IConstant && 
                              p.Terms[1] is IConstant<Coordinate2D>
                        select new { Actor = p.Terms[0].Name, Location = (p.Terms[1] as IConstant<Coordinate2D>).Value };

            Debug.Log("building init state creations");
            foreach (var initPos in initialPositions)
            {
                Debug.Log(initPos.Actor + ", " + initPos.Location.ToString());
                CM.Actor actor = cm.FindActor(initPos.Actor);
                if(actor == null)
                {
                    Debug.Log("actor [" + initPos.Actor + "] not found in cinematic model.");
                    continue;
                }

                string modelFileName = actor.Model; 
                if (string.IsNullOrEmpty(modelFileName))
                {
                    Debug.Log("model name for actor[" + initPos.Actor + "] not found in cinematic model.");
                    continue;
                }                    
                aaq.Add(new Create(0, initPos.Actor, modelFileName, initPos.Location.ToVector3()));
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
                        if (endTick < float.Epsilon)
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

        private static CM.Animation getEffectingAnimation(IStoryAction<UintT> storyAction, CM.DomainAction domainAction)
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
                    IActionProperty actorNameProperty;
                    if(storyAction.TryGetProperty(domainActionParameter.Name, out actorNameProperty))
                    {
                        effectorActorName = actorNameProperty.Value.Name; 
                    }
                    if (effectorActorName == null)
                    {
                        Debug.LogError("actorName not set for stepId[" + storyAction.Name + "]");
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

        private static bool getActorName(IStoryAction<UintT> storyAction, CM.DomainActionParameter domainActionParameter, out string actorName)
        {
            actorName = null;
            IActionProperty actorNameProperty;
            if (storyAction.TryGetProperty(domainActionParameter.Name, out actorNameProperty))
            {
                actorName = actorNameProperty.Value.Name;
                return true;
            }

            Debug.Log("actorName not set for stepId[" + storyAction.Name + "]");
            return false;
        }

        private static bool getActorModel(string actorName, out string modelFileName)
        {
            modelFileName = null;
            CM.Actor actor = cm.FindActor(actorName);
            if (actor == null)
            {
                Debug.Log("actor[" + actorName + "] not found in cinematic model");
            }
            else
            {                
                if (actor.Model != null)
                {                    
                    modelFileName = actor.Model;
                    return true;
                }
                else
                {
                    Debug.Log("model name for actor[" + actorName + "] not found in cinematic model.");
                }
            }
            return false;
        }

        private static void enqueueCreateActions(IStoryAction<UintT> storyAction, CM.DomainAction domainAction, CM.Animation effectingAnimation, ActorActionQueue aaq )
        {
            foreach (CM.CreateAction ca in domainAction.CreateActions)
            {
                float startTick = 0;
                string actorName = null;
                string modelName = null;
                Vector3 destination = new Vector3();
                foreach (CM.DomainActionParameter domainActionParameter in domainAction.Params)
                {
                    if (domainActionParameter.Name == ca.ActorNameParamName)
                    {
                        if (getActorName(storyAction, domainActionParameter, out actorName))//actorName is defined, we can look up a model
                        {
                            actorName = actorName.Split(uniqueActorIdentifierSeparators)[0];
                            if (!getActorModel(actorName, out modelName))
                            {
                                break;//didn't find all our datas.  give up on this create action and move to the next one
                            }
                        }
                    }
                    else if (domainActionParameter.Name == ca.OriginParamName)
                    {
                        IActionProperty coord;
                        if(storyAction.TryGetProperty(domainActionParameter.Name, out coord))
                        {
                            destination = ((Coordinate2D)coord.Value.Value).ToVector3();
                        }
                        else
                        {
                            Debug.LogError("origin not set for stepId[" + storyAction.Name + "]");
                        }                        
                    }
                }
                startTick = storyAction.Time.Start.ToMillis(cm.MillisPerTick);
                startTick += getEffectorAnimationOffset(effectingAnimation, ca);
                if(!string.IsNullOrEmpty(modelName) && //validate necessary params
                   !string.IsNullOrEmpty(actorName))
                {
                    aaq.Add(new Create(startTick, actorName, modelName, destination));
                }
                
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

        private static void loadStructuredImpulsePlan(string storyPlanPath)
        {
            var xml = Impulse.v_1_336.Xml.Story.LoadFromFile(storyPlanPath);
            var factory = Impulse.v_1_336.StoryParsingFactories.GetUnsignedIntergerIntervalFactory();
            story = factory.ParseStory(xml, false);//TODO true!
        }

        private static CM.CinematicModel loadCinematicModel(string cinematicModelPath)
        {
            return CM.Parser.Parse(cinematicModelPath);
        }
    }
}
