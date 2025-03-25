using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class sc_bate : MonoBehaviour
{

    public Transform player; // Referencia al jugador
    public float swingSpeed = 5000;  // Velocidad del swing
    public float maxSwingAngle = 90f; // Ángulo máximo de swing

    private bool isSwinging = false;
    private float currentRotation = 0f;

    private void Update()
    {
        if (isSwinging)
        {
            float rotationAmount = swingSpeed * Time.deltaTime;
            currentRotation += rotationAmount;

            if (currentRotation >= maxSwingAngle)
            {
                currentRotation = 0f;
                isSwinging = false;
            }
            else
            {
                transform.RotateAround(player.position, Vector3.up, rotationAmount);
            }
        }
    }

    public void OnSwing(InputAction.CallbackContext con)
    {
        if (con.performed && !isSwinging)
        {
            isSwinging = true;
            currentRotation = 0f; // Reinicia el contador de rotación
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && isSwinging)
        {
            Vector3 dir = (other.transform.position - transform.position).normalized;
            Vector3 forceDir = new Vector3(dir.x, 0.5f, dir.z);
            other.GetComponent<Rigidbody>().AddForce(forceDir * 10f, ForceMode.Impulse);
        }
    }
}
