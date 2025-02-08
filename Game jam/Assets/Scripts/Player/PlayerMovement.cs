using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement instance { get; private set; }
    public float speed;
    public float current_speed;
    public bool canMove = true;
    
    public Rigidbody rb;

    private Vector2 dir;

    private float y_fix = 0;

    private Vector3 start_position;

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

        start_position = transform.position;

        current_speed = speed;
    }

    private void FixedUpdate()
    {
        if (!canMove) return;

        Vector3 d = ((transform.forward * dir.y) + (transform.right * dir.x)) * current_speed * Time.fixedDeltaTime;
        Vector3 with_y_speed = new Vector3(d.x, rb.velocity.y, d.z);
        rb.velocity = with_y_speed;
    }

    private void Update()
    {
        if (transform.position.y < 0)
            transform.position = start_position;
    }

    public void Move(InputAction.CallbackContext con)
    {
        if (rb == null) return;

        dir = con.ReadValue<Vector2>();
        Debug.Log(dir);
        if (dir.y >= 0.7f) current_speed = speed;
        else current_speed = speed * 0.75f;
    }
}