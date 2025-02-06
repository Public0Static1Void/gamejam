using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraRotation : MonoBehaviour
{
    [SerializeField] private Transform player;
    [Header("Speed")]
    public float cameraSpeed;
    public float x, y;

    Vector2 inp;

    public bool shake = false;
    private float amount, timer = 0, real_timer = 0;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (!PlayerMovement.instance.canMove) return;

        if (shake)
        {
            timer += Time.deltaTime;
            real_timer += Time.deltaTime;
            // Right
            if (timer >= 0.05f)
            {
                amount *= -1;
                timer = 0;
            }
            if (real_timer > 0.5f)
            {
                timer = 0;
                real_timer = 0;
                amount = 0;
                shake = false;
            }

            switch (Random.Range(0, 5))
            {
                case 0:
                    x += amount * Time.deltaTime;
                    break;
                case 1:
                    y += amount * Time.deltaTime;
                    break;
                case 2:
                    x += amount * Time.deltaTime;
                    y += amount * Time.deltaTime;
                    break;
                case 3:
                    x += amount * Time.deltaTime;
                    y -= amount * Time.deltaTime;
                    break;
                case 4:
                    x -= amount * Time.deltaTime;
                    y += amount * Time.deltaTime;
                    break;
            }
        }

        if (inp.x > 1 || inp.x < -1)
        {
            x += Input.GetAxis("Mouse X") * cameraSpeed * Time.deltaTime;
        }
        else
        {
            x += inp.x * cameraSpeed * Time.deltaTime;
        }
        if (inp.y > 1 || inp.y < -1)
        {
            y += Input.GetAxis("Mouse Y") * cameraSpeed * Time.deltaTime;
        }
        else
        {
            y += inp.y * cameraSpeed * Time.deltaTime;
        }

        y = Mathf.Clamp(y, -40, 60);


        player.rotation = Quaternion.Euler(0, x, 0);
        transform.rotation = Quaternion.Euler(-y, x, 0);
    }

    public void ChangeRotation(InputAction.CallbackContext con)
    {
        inp = con.ReadValue<Vector2>();
    }

    public void ShakeCamera(float value)
    {
        amount = value;
        shake = true;
    }
}
