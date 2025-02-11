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

    [Header("References")]
    public List<RectTransform> UIElements_to_move;

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

            switch (Random.Range(0, 7))
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
                case 5:
                    y -= amount * Time.deltaTime;
                    break;
                case 6:
                    x -= amount * Time.deltaTime;
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

        /// Rotación del jugador y la cámara
        player.rotation = Quaternion.Euler(0, x, 0);
        transform.rotation = Quaternion.Euler(-y, x, 0);

        /// Movimiento de los elementos de la UI con la cámara
        for (int i = 0; i < UIElements_to_move.Count; i++)
        {
            Vector2 input = inp.normalized;
            UIElements_to_move[i].anchoredPosition = new Vector3(
                    Mathf.Lerp(UIElements_to_move[i].anchoredPosition.x, -input.x * UIElements_to_move[i].rect.width * UIElements_to_move[i].localScale.x, Time.deltaTime * (cameraSpeed * 0.005f)),
                    Mathf.Lerp(UIElements_to_move[i].anchoredPosition.y, -input.y * UIElements_to_move[i].rect.height * UIElements_to_move[i].localScale.y, Time.deltaTime * (cameraSpeed * 0.005f))
                );
        }
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
