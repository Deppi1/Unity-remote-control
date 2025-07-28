using System;



namespace DiveQuestSystem
{
    public interface IInteractable
    {
        public Action<IInteractableParams> ChangeStateEvent{get; set;}
        public void Execute(object arg); // implement this in class that need to test
    }
}
