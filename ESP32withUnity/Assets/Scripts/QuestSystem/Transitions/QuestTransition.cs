using System;
using System.Collections.Generic;
using DIVE_Utilities;
using HintsSystem;
using UnityEngine.Events;


namespace DiveQuestSystem
{
    public class QuestTransition
    {
        public event Action<QuestTransition> TransitionConditionCompleted;
        public QuestNode NextNode; // The node to which this transition leads
        public IInteractableParams TransitionParams;
        public IInteractable Target;
        public bool IsCompleted = false;
        private UnityAction _callback;

        
        
        public QuestTransition(IInteractableParams parameters, IInteractable target, QuestNode nextNode, UnityAction callback)
        {
            NextNode = nextNode;
            _callback = callback;
            Target = target;
            TransitionParams = parameters;
        }


        protected virtual void NotifyAboutTransitionCompleted()
        {
            TransitionConditionCompleted?.Invoke(this);
            if (_callback == null) return;
            
            _callback();
        }


        public virtual void Initialize()
        {
            if (Target == null) return;
            if ((Target as IInitializable) != null)
            {
                (Target as IInitializable).Initialize();
            }
            
            Target.ChangeStateEvent += CheckParams;
        }


        public virtual void Uninitialize()
        {
            if (Target == null) return;
            if ((Target as IInitializable) != null)
            {
                (Target as IInitializable).Uninitialize();
            }
            
            Target.ChangeStateEvent -= CheckParams;
        }


        private void CheckParams(IInteractableParams param)
        {
            if (TransitionParams.Equal(param))
            {
                IsCompleted = true;
                NotifyAboutTransitionCompleted();
            }
            else
            {
                IsCompleted = false;
            }
        }
    }
}