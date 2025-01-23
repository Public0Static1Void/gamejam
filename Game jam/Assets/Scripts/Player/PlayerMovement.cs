using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement instance { get; private set; }
    public float speed;
    public bool canMove = true;
    
    private Rigidbody rb;

    private Vector2 dir;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!canMove) return;
        rb.velocity = new Vector3(dir.x * speed * Time.deltaTime, rb.velocity.y, dir.y * speed * Time.deltaTime);
    }

    public void Move(InputAction.CallbackContext con)
    {
        if (rb == null) return;

        dir = con.ReadValue<Vector2>();
    }
}