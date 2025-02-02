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
    
    public Rigidbody rb;

    private Vector2 dir;

    private float y_fix = 0;

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

        Vector3 d = ((transform.forward * dir.y) + (transform.right * dir.x)) * speed * Time.fixedDeltaTime;

        rb.velocity = d;
    }

    private void Update()
    {
        if (transform.position.y < 0)
            transform.position = new Vector3(transform.position.x, 2, transform.position.z);
        else if (transform.position.y > 2)
            transform.position = new Vector3(transform.position.x, 1.5f, transform.position.z);
    }

    public void Move(InputAction.CallbackContext con)
    {
        if (rb == null) return;

        dir = con.ReadValue<Vector2>();
    }
}