using UnityEngine;
using System.Collections.Generic;
using System;
using DIVE_Utilities;



namespace DiveQuestSystem
{
    [Serializable]
    public class Quest
    {
        public QuestNode FirstNode = null;
        private QuestNode _currentQuestNode;
        public int LastNodeID = 0;
        public QuestNode PreviousNode = null;
        public event Action<bool> GameOverEvent;
        [HideInInspector] public List<QuestNode> Nodes;


        public void StartSequence()
        {
            if (FirstNode == null)
            {
                FirstNode = Nodes[0];
            }

            _currentQuestNode = FirstNode;
            LastNodeID = Nodes[Nodes.Count - 1].NodeID;
            ActivateCurrentNode();
        }

        public void InitializeNode(QuestNode node)
        {
            node.Initialize();
            foreach (var transition in node.GetTransitionsList())
            {
                transition.TransitionConditionCompleted += ActivateNextNode;
            }
            
        }
        
        public void UninitializeNode(QuestNode node)
        {
            node.Uninitialize();

            foreach (var transition in node.GetTransitionsList())
            {
                transition.TransitionConditionCompleted -= ActivateNextNode;
            }
        }


        private void ActivateNextNode(QuestTransition transition)
        {
            UninitializeNode(_currentQuestNode);
            PreviousNode = _currentQuestNode;
            _currentQuestNode = transition.NextNode;
            ActivateCurrentNode();
        }


        private void ActivateCurrentNode()
        {
            if (_currentQuestNode == null)
            {
                //QuestFinished
                GameOverEvent?.Invoke(true);
                
                return;
            }
            InitializeNode(_currentQuestNode);
        }


        public QuestNode CurrentQuestNode
        {
            get
            {
                return _currentQuestNode;
            }
        }


        public void CreateNodes(int count)
        {
            if (Nodes == null)
            {
                Nodes = new List<QuestNode>();
            }
            for (int i = 0; i <= count; i++)
            {
                Nodes.Add(new QuestNode(i));
            }
        }
    }
}