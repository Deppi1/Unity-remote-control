using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DiveQuestSystem
{
    class QuestGroupEditor : Editor
    {
        private Type[] _questImplementations;
        private Type[] _transitionImplementations;

        private int _questImplementationTypeIndex;
        private int _transitionImplementationTypeIndex;

        // public override void OnInspectorGUI()
        // {
        //     QuestStep _questStep = target as QuestStep;

        //     if (_questStep == null)
        //         return;

        //     if (_questImplementations == null || _transitionImplementations == null || GUILayout.Button("Refresh implementations"))
        //     {
        //         _questImplementations = GetImplementations<IQuest>();
        //         _transitionImplementations = GetImplementations<IQuestTransition>();
        //     }

        //     _questImplementationTypeIndex = GetImplementationTypeIndex(
        //         "Quest implementations", _questImplementations, _questImplementationTypeIndex);

        //     if (GUILayout.Button("Create quest instance"))
        //         _questStep.AddQuest(CreateQuest());

        //     _transitionImplementationTypeIndex = GetImplementationTypeIndex(
        //         "Transition implementations", _transitionImplementations, _transitionImplementationTypeIndex);

        //     if (GUILayout.Button("Create transition instance"))
        //         _questStep.SetTransition(CreateTransition());

        //     base.OnInspectorGUI();
        // }

        private static Type[] GetImplementations<T>()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes());

            var interfaceType = typeof(T);

            return types.Where(p => interfaceType.IsAssignableFrom(p) && !p.IsAbstract)
                .Where(impl => !impl.IsSubclassOf(typeof(UnityEngine.Object))).ToArray();
        }

        private int GetImplementationTypeIndex(string popupTitle, Type[] implementations, int currentIndex)
        {
            return EditorGUILayout.Popup(new GUIContent(popupTitle),
                currentIndex, implementations.Select(impl => impl.Name).ToArray());
        }

        private QuestNode CreateQuest()
        {
            return Activator.CreateInstance(_questImplementations[_questImplementationTypeIndex]) as QuestNode;
        }

        private QuestTransition CreateTransition()
        {
            return Activator.CreateInstance(_transitionImplementations[_transitionImplementationTypeIndex]) as QuestTransition;
        }
    }
}
