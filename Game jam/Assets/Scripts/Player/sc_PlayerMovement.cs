using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement instance { get; private set; }
    [Header("Stats")]
    public float speed;
    public float current_speed;
    private float target_speed = 0;
    public float max_stamina;
    public float current_stamina;
    public float fov;
    private float fov_change;
    public bool canMove = true;
    public bool sprinting = false;
    [Header("References")]
    public Image stamina_image;
    
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
        current_stamina = max_stamina;

        current_speed = speed;
        fov = Camera.main.fieldOfView;
    }

    private void FixedUpdate()
    {
        if (!canMove) return;
        #region Sprint
        if (sprinting)
        {
            if (current_stamina <= 0) /// Cancela el sprint si no tienes stamina
                sprinting = false;

            if (stamina_image.color.a < 0.5f) /// Cantidad de transparencia de stamina
            {
                Color col = new Color(stamina_image.color.r, stamina_image.color.g, stamina_image.color.b, stamina_image.color.a + Time.deltaTime);
                stamina_image.color = col;
            }
            stamina_image.fillAmount = 1 - (1 - (current_stamina / max_stamina));

            current_stamina -= Time.deltaTime;
            current_speed = Mathf.Lerp(current_speed, target_speed * 1.5f, (speed * 0.05f) * Time.deltaTime);
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, fov + fov_change, Time.deltaTime * 8);
        }
        else if (current_speed > target_speed + 0.05f || Camera.main.fieldOfView > fov + 0.05f)
        {
            current_speed = Mathf.Lerp(current_speed, target_speed, (speed * 0.05f) * Time.deltaTime);
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, fov, Time.deltaTime * 6);
        }
        else if (current_stamina < max_stamina) /// Recupera la stamina
        {
            current_stamina += Time.deltaTime;
            stamina_image.fillAmount = (current_stamina / max_stamina);
        }
        else /// Esconde el círculo de stamina cuando ya no hagan falta más cálculos
        {
            if (stamina_image.color.a > 0)
            {
                Color col = new Color(stamina_image.color.r, stamina_image.color.g, stamina_image.color.b, stamina_image.color.a - Time.deltaTime);
                stamina_image.color = col;
            }
        }
        #endregion
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
            sprinting = !sprinting;
        }
    }

    public void Move(InputAction.CallbackContext con)
    {
        if (rb == null) return;

        dir = con.ReadValue<Vector2>();
        int target_fov = 20;
        if (dir.y >= 0.7f)
        {
            current_speed = speed;
            fov_change = target_fov;
        }
        else if (dir.y < 0)
        {
            current_speed = speed * 0.5f;
            fov_change = target_fov * 0.45f;
        }
        else 
        { 
            current_speed = speed * 0.75f;
            fov_change = target_fov * 0.7f;
        }
        target_speed = current_speed;
    }
}