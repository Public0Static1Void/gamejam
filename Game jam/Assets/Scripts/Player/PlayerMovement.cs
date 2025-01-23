using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public float speed;
    
    private Rigidbody rb;

    private Vector2 dir;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector3(dir.x * speed * Time.deltaTime, rb.velocity.y, dir.y * speed * Time.deltaTime);
    }

    public void Move(InputAction.CallbackContext con)
    {
        if (rb == null) return;

        dir = con.ReadValue<Vector2>();
    }
}