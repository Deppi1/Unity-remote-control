namespace DiveQuestSystem
{
    public class InteractableEmptyParam : IInteractableParams
    {
        public bool Equal(IInteractableParams param)
        {
            if (param == null) return false;
            if (param.GetType() == GetType()) return true;
            
            return false;
        }
    }
}