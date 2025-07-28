using DIVE_Utilities;
using System.Collections.Generic;
using UnityEngine.Events;


namespace DiveQuestSystem
{
    public abstract class BaseCompositeTransition : QuestTransition
    {
        public List<QuestTransition> Subtransitions;
        


        public BaseCompositeTransition(List<QuestTransition> subtransitions, IInteractableParams parameters, IInteractable target, QuestNode NextNode, UnityAction callback) : base (parameters, target, NextNode, callback)
        {
            Subtransitions = new List<QuestTransition>(subtransitions);
        }


        public override void Initialize()
        {
            foreach (var subtransition in Subtransitions)
            {
                subtransition.Initialize();
                subtransition.TransitionConditionCompleted += OnOneOfTransitionCompleted;
            }
        }


        public override void Uninitialize()
        {
            foreach (var subtransition in Subtransitions)
            {
                subtransition.Uninitialize();
                subtransition.TransitionConditionCompleted -= OnOneOfTransitionCompleted;
            }
        }


        public abstract void OnOneOfTransitionCompleted(QuestTransition inCompletedTransition);
    }
}