using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;

[CustomEditor(typeof(sc_ConditionExecuter))]
public class sc_ConditionExecuterEditor : Editor
{
    private ReorderableList unityEventList;
    private ReorderableList boolEventList;
    private SerializedProperty events_list;

    private void OnEnable()
    {
        unityEventList = InitList("all_die_events");
        boolEventList = InitList("bool_events");
    }
    private ReorderableList InitList(string list_name)
    {
        events_list = serializedObject.FindProperty(list_name);
        ReorderableList event_list = new ReorderableList(serializedObject, events_list, true, true, true, true);

        event_list.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, $"{list_name} Events");
        };

        event_list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = event_list.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, element, new GUIContent($"Event {index + 1}"), true);
        };

        event_list.elementHeightCallback = (int index) =>
        {
            var element = event_list.serializedProperty.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(element, true) + 4;
        };

        return event_list;
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        sc_ConditionExecuter script = (sc_ConditionExecuter)target;

        // Dibuja el desplegable del enum
        script.conditionType = (sc_ConditionExecuter.ConditionType)EditorGUILayout.EnumPopup("Select condition", script.conditionType);
        GUILayout.Space(20);

        switch (script.conditionType)
        {
            default:
                EditorGUILayout.LabelField("Select a condition", EditorStyles.boldLabel);
                break;
            case sc_ConditionExecuter.ConditionType.GAMEOBJECTS:
                DrawGameObjecttList("GameObjects to die", script.objects_to_die);
                GUILayout.Space(10);
                unityEventList.DoLayoutList();
                break;
            case sc_ConditionExecuter.ConditionType.BOOL:
                script.xd = EditorGUILayout.Toggle("xd", script.xd);
                boolEventList.DoLayoutList();
                break;
        }


        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
            EditorUtility.SetDirty(target);
    }

    private void DrawGameObjecttList(string label, List<GameObject> list)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        int toRemove = -1;
        for (int i = 0; i < list.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            string name = list[i] != null ? list[i].name : $"Object {i + 1}";

            list[i] = (GameObject)EditorGUILayout.ObjectField(name, list[i], typeof(GameObject), true);

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                toRemove = i;
            }
            EditorGUILayout.EndHorizontal();
        }

        if (toRemove >= 0)
        {
            list.RemoveAt(toRemove);
        }
        if (GUILayout.Button("+"))
        {
            list.Add(null);
        }
    }
}
public class sc_ConditionExecuter : MonoBehaviour
{
    public enum ConditionType { GAMEOBJECTS, BOOL }

    public ConditionType conditionType;

    public List<GameObject> objects_to_die = new List<GameObject>();
    public List<UnityEvent> all_die_events = new List<UnityEvent>();

    public bool xd;
    public List<UnityEvent> bool_events = new List<UnityEvent>();


    private void Update()
    {
        switch (conditionType)
        {
            case ConditionType.GAMEOBJECTS:
                int dead_amount = 0;
                for (int i = 0; i < objects_to_die.Count; i++)
                {
                    if (objects_to_die[i] == null)
                    {
                        dead_amount++;
                    }
                }
                if ((objects_to_die.Count <= 0 || (dead_amount > 0 && dead_amount >= objects_to_die.Count)) && all_die_events.Count > 0)
                {
                    for (int i = 0; i < all_die_events.Count; i++)
                    {
                        all_die_events[i].Invoke();
                    }

                    objects_to_die.Clear();
                    all_die_events.Clear();
                }
                break;
            case ConditionType.BOOL:
                if (xd)
                {

                }
                break;
        }
    }

    public void SetBool(bool value)
    {
        xd = value;
    }
}