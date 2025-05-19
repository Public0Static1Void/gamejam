using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class sc_TriggerEventExecuter : MonoBehaviour
{
    [Description("")]
    public string target_tag = "";

    public bool can_execute = true;
    public bool execute_one_time = false;
    private bool keep_executing = true;

    [Header("Events")]
    public UnityEvent triggerEnterEvent;
    public UnityEvent triggerExitEvent;

    void Start()
    {
        bool has_trigger = false;
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider coll in colliders)
        {
            if (coll.isTrigger)
            {
                has_trigger = true;
                break;
            }
        }
        if (!has_trigger)
            Debug.LogWarning($"{gameObject.name} dosen't have a trigger collider. sc_TriggerEventExecuter requires it");
    }

    public void SetExecute(bool value)
    {
        can_execute = value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!keep_executing ||!can_execute) return;

        if (target_tag != "")
        {
            if (other.CompareTag(target_tag))
            {
                triggerEnterEvent.Invoke();
                if (execute_one_time)
                    keep_executing = false;
            }
        }
        else
        {
            triggerEnterEvent.Invoke();
            if (execute_one_time)
                keep_executing = false;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (!keep_executing || !can_execute) return;

        if (target_tag != "")
        {
            if (other.CompareTag(target_tag))
            {
                triggerExitEvent.Invoke();
                if (execute_one_time)
                    keep_executing = false;
            }
        }
        else
        {
            triggerExitEvent.Invoke();
            if (execute_one_time)
                keep_executing = false;
        }
    }
}
