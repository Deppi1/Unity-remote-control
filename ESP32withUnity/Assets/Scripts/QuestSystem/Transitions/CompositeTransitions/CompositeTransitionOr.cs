using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;


namespace DiveQuestSystem
{
    public class CompositeTransitionOr : BaseCompositeTransition
    {
        public CompositeTransitionOr(List<QuestTransition> subtransitions, IInteractableParams parameters, IInteractable target, QuestNode NextNode, UnityAction callback) 
        : base (subtransitions, parameters, target, NextNode, callback){}


        public override void OnOneOfTransitionCompleted(QuestTransition inCompletedTransition)
        {
            NotifyAboutTransitionCompleted();
        }
    }
}