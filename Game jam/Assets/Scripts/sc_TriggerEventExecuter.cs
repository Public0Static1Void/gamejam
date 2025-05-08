using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;

public class sc_TriggerEventExecuter : MonoBehaviour
{
    [Description("")]
    public string target_tag = "";

    [Header("Events")]
    public UnityEvent triggerEnterEvent;
    public UnityEvent triggerExitEvent;


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
