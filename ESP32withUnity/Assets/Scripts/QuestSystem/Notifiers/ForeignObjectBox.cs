using System;
using System.Collections.Generic;
using UnityEngine;
using DIVE_Utilities;



namespace DiveQuestSystem
{
    [RequireComponent(typeof(Collider))]
    public class ForeignObjectBox : MonoBehaviour, IInteractable
    {
        private List<GameObject> _foreignObjects = new List<GameObject>();
        private List<GameObject> _allForeignObjects = new List<GameObject>();
        public event Action<IInteractableParams> AllThingsRemoved;



        public void AddObjects(List<GameObject> inObjectsList)
        {
            _foreignObjects.AddRange(inObjectsList);
            _allForeignObjects.AddRange(inObjectsList);
        }
        
        
        public void AddObjects(GameObject inObjectsList)
        {
            _foreignObjects.Add(inObjectsList);
            _allForeignObjects.Add(inObjectsList);
        }


        private void OnTriggerEnter(Collider other)
        {
            foreach (var item in _foreignObjects)
            {
                if (item == other.gameObject)
                {
                    _foreignObjects.Remove(item);
                    if (_foreignObjects.Count <= 0)
                    {
                        AllThingsRemoved?.Invoke(new InteractableEmptyParam());
                        GetComponent<Collider>().enabled = false;
                    }
                    return;
                }
            }
        }


        private void OnTriggerExit(Collider other) 
        {
            foreach (var item in _allForeignObjects)
            {
                if (item == other.gameObject)
                {
                    _foreignObjects.Add(other.gameObject);
                }
            }
        }


        public void Execute(object param)
        {
            Collider collider = GetComponent<Collider>();
            foreach (GameObject item in _foreignObjects)
            {
                item.transform.position = collider.transform.position;
            }
        }


        public Action<IInteractableParams> ChangeStateEvent
        {
            get
            {
                return AllThingsRemoved;
            }
            set
            {
                AllThingsRemoved = value;
                if (_foreignObjects.Count <= 0)
                {
                    AllThingsRemoved?.Invoke(new InteractableEmptyParam());
                }
            }
        }


        public List<GameObject> AllForeignObjects
        {
            get
            {
                return _allForeignObjects;
            }
        }
    }
}