using UnityEngine;
using System;
using DIVE_Utilities;



namespace DiveQuestSystem
{
    [RequireComponent(typeof(BoxCollider))]
    public class PlayerInZoneNotifier : MonoBehaviour, IInteractable
    {
        private Action<IInteractableParams> OnStateChanged;



        private void OnTriggerEnter(Collider other) 
        {
            if (other.CompareTag("Player")) 
            {
                OnStateChanged?.Invoke(new InteractableEmptyParam());
            }
        }

        private void OnTriggerStay(Collider other) 
        {
            if (other.CompareTag("Player")) 
            {
                OnStateChanged?.Invoke(new InteractableEmptyParam());
            }
        }

        public void Disable()
        {
            GetComponent<BoxCollider>().enabled = false;
            enabled = false;
        }
        
        
        public void Enable()
        {
            GetComponent<BoxCollider>().enabled = true;
            enabled = true;
        }


        public void Execute(object arg)
        {
            OnStateChanged?.Invoke(new InteractableEmptyParam());
        }


        public Action<IInteractableParams> ChangeStateEvent
        {
            get
            {
                return OnStateChanged;
            }
            set
            {
                OnStateChanged = value;
            }
        }
    }
}