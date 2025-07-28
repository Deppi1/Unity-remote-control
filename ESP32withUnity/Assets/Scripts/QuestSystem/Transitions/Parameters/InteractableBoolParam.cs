namespace DiveQuestSystem
{
    public class InteractableBoolParam : IInteractableParams
    {
        public bool Param;



        public InteractableBoolParam(bool finalState)
        {
            Param = finalState;
        }


        public bool Equal(IInteractableParams param)
        {
            InteractableBoolParam valveParams = param as InteractableBoolParam;
            if (valveParams == null) return false;
            
            if (valveParams.Param == Param) return true;

            return false;
        }
    }
}