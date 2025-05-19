using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting.Dependencies.NCalc;
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
    public bool disable_enter_events = false;
    public UnityEvent triggerEnterEvent;
    [Space(10)]
    public bool disable_exit_events = false;
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
    public void SetEnter(bool value)
    {
        disable_enter_events = value;
    }
    public void SetExit(bool value)
    {
        disable_exit_events = value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!keep_executing || !can_execute || disable_enter_events) return;

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
        if (!keep_executing || !can_execute || disable_exit_events) return;

        if (target_tag != "")
        {
            if (other.CompareTag(target_tag))
            {
                triggerExitEvent.Invoke();
                if (execute_one_time)
                    keep_executing = false;

                disable_enter_events = false;
            }
        }
        else
        {
            triggerExitEvent.Invoke();
            if (execute_one_time)
                keep_executing = false;

            disable_enter_events = false;
        }
    }
}
