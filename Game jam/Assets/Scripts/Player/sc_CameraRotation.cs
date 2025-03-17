using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CameraRotation : MonoBehaviour
{
    public static CameraRotation instance { get; private set; }

    [SerializeField] private Transform player;
    [Header("Speed")]
    public float cameraSpeed;
    [HideInInspector]
    public float cameraSpeed_slide;
    public float x, y;

    [Header("References")]
    public List<RectTransform> UIElements_to_move;
    public Slider ui_sensivity_slider;
    public TMP_Text ui_sensivity_value;
    public Image ui_sensivity_value_image;
    public RectTransform ui_sensivity_handle;

    Vector2 inp;

    public bool shake = false;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        ui_sensivity_slider.value = cameraSpeed;

        EventTrigger trigger = ui_sensivity_slider.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = ui_sensivity_slider.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerUp;
        entry.callback.AddListener((data) => { UI_HideImage(); });
        trigger.triggers.Add(entry);
    }

    void Update()
    {
        if (!PlayerMovement.instance.canMove) return;

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
                    Mathf.Lerp(UIElements_to_move[i].anchoredPosition.x, -input.x * (UIElements_to_move[i].rect.x + (UIElements_to_move[i].rect.width * UIElements_to_move[i].localScale.x) * 0.75f) / 2, Time.deltaTime * (cameraSpeed * 0.005f)),
                    Mathf.Lerp(UIElements_to_move[i].anchoredPosition.y, -input.y * (UIElements_to_move[i].rect.y + (UIElements_to_move[i].rect.height * UIElements_to_move[i].localScale.y) * 0.75f) / 2, Time.deltaTime * (cameraSpeed * 0.005f))
                );
        }
    }

    public void ChangeRotation(InputAction.CallbackContext con)
    {
        inp = con.ReadValue<Vector2>();
    }

    public void ShakeCamera(float duration, float force)
    {
        StartCoroutine(ShakeCameraRoutine(duration, force));
    }
    private IEnumerator ShakeCameraRoutine(float duration, float force)
    {
        shake = true;
        Vector3 original_position = transform.localPosition;
        float timer = 0;
        while (timer < duration)
        {
            if (!PlayerMovement.instance.canMove) break;
            timer += Time.deltaTime;

            float x = Random.Range(-1, 1) * force;
            float y = Random.Range(-1, 1) * force;

            transform.localPosition = Vector3.Lerp(original_position, original_position + new Vector3(x, y, 0), Time.deltaTime);

            yield return null;
        }

        transform.localPosition = original_position;
        shake = false;
    }

    public void SetRotation(float x_rotation, float y_rotation)
    {
        x = x_rotation;
        y = y_rotation;
    }

    public void ChangeCameraSpeed()
    {
        // Cambia la posición del texto y la imagen
        float offset = 5;
        Vector2 pos = new Vector2(ui_sensivity_handle.anchorMax.x * ui_sensivity_handle.rect.width, ui_sensivity_handle.anchorMax.y + offset);
        GameManager.gm.ChangeUIPosition(pos, ui_sensivity_value_image.rectTransform, ui_sensivity_value.rectTransform);

        if (ui_sensivity_value_image.color.a < 1 || ui_sensivity_value.color.a < 1)
        {
            /// Le pone su color sin transparencia
            ui_sensivity_value.color = new Color(ui_sensivity_value.color.r, ui_sensivity_value.color.g, ui_sensivity_value.color.b, 1);
            ui_sensivity_value_image.color = new Color(ui_sensivity_value_image.color.r, ui_sensivity_value_image.color.g, ui_sensivity_value_image.color.b, 1);
        }


        // Se cambia el valor de cameraSpeed por el del slider
        cameraSpeed = ui_sensivity_slider.value;
        cameraSpeed_slide = cameraSpeed / 5;

        ui_sensivity_value.text = cameraSpeed.ToString();
    }

    public void UI_HideImage()
    {
        /// Esconde la imagen con el texto que muestra el valor de cameraSpeed
        StartCoroutine(GameManager.gm.HideImage(2, ui_sensivity_value_image, ui_sensivity_value));
    }
}