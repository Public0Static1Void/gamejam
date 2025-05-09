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

    private void OnTriggerEnter(Collider other)
    {
        if (target_tag != "")
        {
            if (other.CompareTag(target_tag))
            {
                triggerEnterEvent.Invoke();
            }
        }
        else
        {
            triggerEnterEvent.Invoke();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (target_tag != "")
        {
            if (other.CompareTag(target_tag))
            {
                triggerExitEvent.Invoke();
            }
        }
        else
        {
            triggerExitEvent.Invoke();
        }
    }
}
