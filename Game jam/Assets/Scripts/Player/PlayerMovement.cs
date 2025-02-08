using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement instance { get; private set; }
    public float speed;
    public float current_speed;
    private float target_speed = 0;
    public float fov;
    public bool canMove = true;
    public bool sprinting = false;
    
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
        fov = Camera.main.fieldOfView;
    }

    private void FixedUpdate()
    {
        if (!canMove) return;

        if (sprinting)
        {
            current_speed = Mathf.Lerp(current_speed, target_speed * 1.5f, (speed * 0.05f) * Time.deltaTime);
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, fov + 20, Time.deltaTime * 8);
        }
        else if (current_speed > target_speed || Camera.main.fieldOfView > fov)
        {
            current_speed = Mathf.Lerp(current_speed, target_speed, (speed * 0.05f) * Time.deltaTime);
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, fov, Time.deltaTime * 6);
        }
        Vector3 d = ((transform.forward * dir.y) + (transform.right * dir.x)) * current_speed * Time.fixedDeltaTime;
        Vector3 with_y_speed = new Vector3(d.x, rb.velocity.y, d.z);
        rb.velocity = with_y_speed;
    }

    private void Update()
    {
        if (transform.position.y < 0)
            transform.position = start_position;
    }

    public void Sprint(InputAction.CallbackContext con)
    {
        if (con.performed)
        {
            sprinting = true;
        }
        else
        {
            sprinting = false;
        }
    }

    public void Move(InputAction.CallbackContext con)
    {
        if (rb == null) return;

        dir = con.ReadValue<Vector2>();
        if (dir.y >= 0.7f)
        {
            current_speed = speed;
            target_speed = speed;
        }
        else 
        { 
            current_speed = speed * 0.75f;
            target_speed = current_speed;
        }
    }
}