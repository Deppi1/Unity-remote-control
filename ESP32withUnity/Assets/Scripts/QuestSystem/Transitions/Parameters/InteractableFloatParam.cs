using System;



namespace DiveQuestSystem
{
    public class InteractableFloatParam : IInteractableParams
    {
        public float Param;



        public InteractableFloatParam(float finalState)
        {
            Param = finalState;
        }


        public bool Equal(IInteractableParams param)
        {
            InteractableFloatParam temp = param as InteractableFloatParam;
            if (temp == null) return false;
            if (Math.Abs(temp.Param - Param) <= .5f) return true;
            
            return false;
        }
    }
}