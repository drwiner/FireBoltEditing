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
        //TODO speedify by adding caching to lookup methods...initial effort done in cinematic model
        //TODO concatenate looping subsequent animations on the same character into a single action?  would require even longer preprocessing...
        //TODO does rotate work with dota logs now?
        //add binary search to the aaq to support animation concat
        private static CM.CinematicModel cm;
        private static AStory<UintV, UintT, IIntervalSet<UintV, UintT>> story;
        private static readonly char[] uniqueActorIdentifierSeparators = { ':' };

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
                enqueueAnimateActions(storyAction, domainAction, effectingAnimation, aaq);
                enqueueDestroyActions(storyAction, domainAction, effectingAnimation, aaq);
                enqueuetranslateActions(storyAction, domainAction, effectingAnimation, aaq);
                enqueueRotateActions(storyAction, domainAction, effectingAnimation, aaq);            
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

        private static void enqueueRotateActions(IStoryAction<UintT> storyAction, CM.DomainAction domainAction, 
                                                 CM.Animation effectingAnimation, ActorActionQueue aaq)
        {
            foreach (CM.RotateAction ra in domainAction.RotateActions)
            {
                float startTick = 0;
                float endTick = 0;
                string actorName = null;
                float targetDegrees=0;
                //Vector3 targetVector = Vector3.zero;
                foreach (CM.DomainActionParameter domainActionParameter in domainAction.Params)
                {
                    if (domainActionParameter.Name == ra.ActorNameParamName)
                    {
                        if (!getActorName(storyAction, domainActionParameter, out actorName))
                        {
                            break;
                        }
                    }
                    else if (domainActionParameter.Name == ra.DestinationParamName)
                    {
                        IActionProperty targetOrientation;
                        if (storyAction.TryGetProperty(domainActionParameter.Name, out targetOrientation))
                        {
                            targetDegrees = (float)(double)targetOrientation.Value.Value;                           
                        }
                        else
                        {
                            Debug.LogError("orientation not set for stepId[" + storyAction.Name + "]");
                        }
                    }
                }
                startTick = getStartTick(storyAction, ra, effectingAnimation);
                endTick = getEndTick(storyAction, ra, effectingAnimation, startTick);
                if (Rotate.ValidForConstruction(actorName))
                {
                    aaq.Add(new Rotate(startTick, endTick, actorName, targetDegrees));
                }                
            }
        }

        private static void enqueuetranslateActions(IStoryAction<UintT> storyAction, CM.DomainAction domainAction, 
                                               CM.Animation effectingAnimation, ActorActionQueue aaq)
        {
            foreach (CM.TranslateAction ta in domainAction.TranslateActions)
            {
                float startTick = 0;
                float endTick = 0;
                string actorName = null;
                Vector3 destination = Vector3.zero;
                foreach (CM.DomainActionParameter domainActionParameter in domainAction.Params)
                {
                    if (domainActionParameter.Name == ta.DestinationParamName)
                    {
                        IActionProperty coord;
                        if (storyAction.TryGetProperty(domainActionParameter.Name, out coord))
                        {

                            destination = ((Coordinate2D)coord.Value.Value).ToVector3(cm.DomainDistancePerEngineDistance);
                           
                        }
                        else
                        {
                            Debug.LogError("destination not set for stepId[" + storyAction.Name + "]");
                        }
                    }
                    else if (domainActionParameter.Name == ta.ActorNameParamName)
                    {
                        if (!getActorName(storyAction, domainActionParameter, out actorName))
                        {
                            break;
                        }
                    }
                }
                startTick = getStartTick(storyAction,ta,effectingAnimation);
                endTick = getEndTick(storyAction, ta, effectingAnimation, startTick);
                if (Translate.ValidForConstruction(actorName))
                {
                    aaq.Add(new Translate(startTick, endTick, actorName, destination));
                }
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
                    CM.Actor effectorActor = cm.FindActor(effectorActorName);
                    if (effectorActor == null)
                    {
                        Debug.Log(string.Format("effector actor [{0}] undefined for step[{1}]",effectorActorName,storyAction.Name));
                        return null;
                    }
                    effectorAnimationMapping = effectorActor.FindAnimationMapping(effectorAnimateAction.Name);
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

        private static void enqueueAnimateActions(IStoryAction<UintT> storyAction, CM.DomainAction domainAction, CM.Animation effectingAnimation, ActorActionQueue aaq)
        {            
            foreach(CM.AnimateAction animateAction in domainAction.AnimateActions)
            {
                string actorName = null;
                float startTick = 0;
                float endTick = 0;
                CM.AnimationMapping animMapping = null;
                CM.Animation animation = null;
                foreach(CM.DomainActionParameter domainActionParameter in domainAction.Params)
                {
                    if (domainActionParameter.Name == animateAction.ActorNameParamName)
                    {
                        if (getActorName(storyAction, domainActionParameter, out actorName))
                        {
                            string homogenousActorName = actorName.Split(uniqueActorIdentifierSeparators)[0];
                            CM.Actor actor = cm.FindActor(actorName);
                            if (actor == null)
                            {
                                Debug.Log(string.Format("actor[{0}] undefined",actorName));
                                break;
                            }
                            animMapping = actor.FindAnimationMapping(animateAction.Name);
                            if (animMapping == null)
                            {
                                Debug.Log("cinematic model animation instance undefined for actor[" +
                                    homogenousActorName + "] animateAction[" + animateAction.Name + "]");
                                break;
                            }
                            animation = cm.FindAnimation(animMapping.AnimationName);
                            if (animation == null)
                            {
                                Debug.Log(string.Format("animation name [{0}] undefined",animMapping.AnimationName));
                                break;
                            }
                        }
                    }
                }
                startTick = getStartTick(storyAction, animateAction, effectingAnimation);
                endTick = getEndTick(storyAction, animateAction, effectingAnimation, startTick);
                if (AnimateMecanim.ValidForConstruction(actorName, animation))
                {
                    aaq.Add(new AnimateMecanim(startTick, endTick, actorName, animation.FileName, animMapping.LoopAnimation));
                }
            }
        }

        private static float getStartTick(IStoryAction<UintT> storyAction, CM.FireBoltAction fireBoltAction, CM.Animation effectingAnimation)
        {
            float startTick = 0;
            startTick = storyAction.Time.Start.ToMillis(cm.MillisPerTick);
            startTick += getEffectorAnimationOffset(effectingAnimation, fireBoltAction);
            return startTick;
        }

        private static float getEndTick(IStoryAction<UintT> storyAction, CM.FireBoltAction fireBoltAction, CM.Animation effectingAnimation, float startTick)
        {
            float endTick = storyAction.Time.End.ToMillis(cm.MillisPerTick);
            if (fireBoltAction.MaxDuration.HasValue &&
                fireBoltAction.MaxDuration.Value < storyAction.Time.End - storyAction.Time.Start)
            {
                endTick = startTick + fireBoltAction.MaxDuration.Value;
            }
            return endTick;
        }

        private static float getEffectorAnimationOffset(CM.Animation effectingAnimation, CM.FireBoltAction fireBoltAction)
        {
            float offset = 0;
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
                            string homogenousActorName = actorName.Split(uniqueActorIdentifierSeparators)[0]; //handle numerically specified actor copies, looking up model for the base-name
                            if (!getActorModel(homogenousActorName, out modelName))
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
                startTick = getStartTick(storyAction, ca, effectingAnimation);                
                if(Create.ValidForConstruction(actorName,modelName))
                {
                    aaq.Add(new Create(startTick, actorName, modelName, destination));
                }                
            }
        }

        private static void enqueueDestroyActions(IStoryAction<UintT> storyAction, CM.DomainAction domainAction, 
                                                    CM.Animation effectingAnimation, ActorActionQueue aaq)
        {
            foreach (CM.DestroyAction da in domainAction.DestroyActions)
            {
                float startTick = 0;
                string actorName = null;
                foreach (CM.DomainActionParameter domainActionParameter in domainAction.Params)
                {
                    if (domainActionParameter.Name == da.ActorNameParamName)
                    {
                        if (!getActorName(storyAction, domainActionParameter, out actorName))
                        {
                            break;
                        }
                    }
                }
                startTick = getStartTick(storyAction,da,effectingAnimation);
                if (Destroy.ValidForConstruction(actorName))
                {
                    aaq.Add(new Destroy(startTick, actorName));
                }
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
            Debug.Log("begin story plan xml load");
            var xml = Impulse.v_1_336.Xml.Story.LoadFromFile(storyPlanPath);
            Debug.Log("end story plan xml load");
            var factory = Impulse.v_1_336.StoryParsingFactories.GetUnsignedIntergerIntervalFactory();
            Debug.Log("begin story plan parse");
            story = factory.ParseStory(xml, false);//TODO true!
            Debug.Log("end story plan parse");
        }

        private static CM.CinematicModel loadCinematicModel(string cinematicModelPath)
        {
            return CM.Parser.Parse(cinematicModelPath);
        }

        
    }
}
