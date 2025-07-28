using System.Collections.Generic;
using UnityEngine.Events;
using DIVE_Utilities;



namespace DiveQuestSystem
{
    public class CompositeTransitionAnd : BaseCompositeTransition
    {
        public CompositeTransitionAnd(List<QuestTransition> subtransitions, IInteractableParams parameters, IInteractable target, QuestNode NextNode, UnityAction callback) 
        : base (subtransitions, parameters, target, NextNode, callback){}


        public override void OnOneOfTransitionCompleted(QuestTransition inCompletedTransition)
        {
            foreach (var subtransition in Subtransitions)
            {
                if(!subtransition.IsCompleted) return;
            }
            NotifyAboutTransitionCompleted();
        }
    }
}