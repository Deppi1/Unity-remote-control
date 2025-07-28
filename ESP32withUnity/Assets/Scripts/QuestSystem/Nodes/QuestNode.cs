using System;
using System.Collections.Generic;
using UnityEngine;



namespace DiveQuestSystem
{
    [Serializable]
    public class QuestNode
    {
        public List<QuestTransition> Transitions = new List<QuestTransition>();
        public int NodeID;



        public QuestNode(int nodeID)
        {
            NodeID = nodeID;
        }


        public List<QuestTransition> GetTransitionsList()
        {
            return Transitions;
        }


        public void AddTransition(QuestTransition transition)
        {
            Transitions.Add(transition);
        }


        public virtual void Initialize()
        {
            foreach (var transition in Transitions)
            {
                transition.Initialize();
            }
        }
        

        public virtual void Uninitialize()
        {
            if (Transitions == null) return;
            
            foreach (var transition in Transitions)
            {
                transition.Uninitialize();
            }
        }
    }
}