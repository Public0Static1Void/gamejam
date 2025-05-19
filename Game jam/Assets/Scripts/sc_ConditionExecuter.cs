using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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