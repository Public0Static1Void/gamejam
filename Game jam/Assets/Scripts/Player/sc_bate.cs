using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class sc_bate : MonoBehaviour
{

    public Transform player; // Referencia al jugador
    public float swingSpeed = 5000;  // Velocidad del swing
    public float maxSwingAngle = 90f; // Ángulo máximo de swing

    public bool isSwinging = false;
    private float currentRotation = 0f;

    public Animator anim;

    public AudioClip clip_swing, clip_hit;

    private AudioSource audioSource;


    private void Update()
    {
        if (isSwinging)
        {
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

            // Check if animation is done (normalizedTime >= 1)
            if (stateInfo.IsName("Swing") && stateInfo.normalizedTime >= 0.8f)
            {
                anim.SetBool("swing", false);
                isSwinging = false;
            }
        }
    }

    public void OnSwing(InputAction.CallbackContext con)
    {
        if (con.performed && !isSwinging)
        {
            isSwinging = true;
            anim.SetBool("swing", true);

            SoundManager.instance.InstantiateSound(clip_swing, transform.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && isSwinging)
        {
            Vector3 dir = (other.transform.position - player.position).normalized;
            Vector3 forceDir = new Vector3(dir.x, 0.5f, dir.z);
            other.GetComponent<EnemyFollow>().AddForceToEnemy(forceDir * 10f);
            if (audioSource == null)
            {
                audioSource = SoundManager.instance.InstantiateSound(clip_hit, transform.position);
            }
            else if (!audioSource.isPlaying)
            {
                audioSource = SoundManager.instance.InstantiateSound(clip_hit, transform.position);
            }
        }
    }
}