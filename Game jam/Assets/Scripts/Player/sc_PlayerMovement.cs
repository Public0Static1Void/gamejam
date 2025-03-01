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
    public bool moving = false;

    // Slide variables
    public bool slide = false;
    private bool can_slide = true;
    public Vector3 slide_camera_offset;
    [HideInInspector]
    public Vector3 camera_original_position;
    private CameraRotation cameraRotation;

    [Header("References")]
    public Image stamina_image;
    
    public Rigidbody rb;

    public Vector2 dir;

    private Vector3 start_position;

    private float slide_timer = 0;


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

        rb.freezeRotation = true;

        camera_original_position = Camera.main.transform.localPosition;

        cameraRotation = Camera.main.gameObject.GetComponent<CameraRotation>();
    }

    private void FixedUpdate()
    {
        if (!canMove) return;
        #region Sprint
        if (sprinting)
        {
            if (current_stamina <= 0) /// Cancela el sprint si no tienes stamina
            {
                target_speed = speed;
                sprinting = false;
            }

            if (!slide)
            {
                if (stamina_image.color.a < 0.5f) /// Cantidad de transparencia de stamina
                {
                    Color col = new Color(stamina_image.color.r, stamina_image.color.g, stamina_image.color.b, stamina_image.color.a + Time.deltaTime);
                    stamina_image.color = col;
                }
                stamina_image.fillAmount = 1 - (1 - (current_stamina / max_stamina));

                current_stamina -= Time.deltaTime;

                current_speed = Mathf.Lerp(current_speed, target_speed, (speed * 0.075f) * Time.deltaTime); /// Suma de velocidad
            }

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

        #region Slide
        if (slide)
        {
            dir = new Vector2(dir.x, 1); /// Fija la dirección hacia adelante

            Camera.main.transform.localPosition = Vector3.Lerp(Camera.main.transform.localPosition, slide_camera_offset, Time.deltaTime * 10); /// Movimiento de la cámara
            current_speed -= Time.deltaTime * ((0.5f * current_speed + target_speed)); /// Pérdida de velocidad
            if (current_speed <= 0)
            {
                current_speed = target_speed;
                slide = false;
            }
        }
        else if (Camera.main.transform.position != camera_original_position)
        {
            Camera.main.transform.localPosition = Vector3.Lerp(Camera.main.transform.localPosition, camera_original_position, Time.deltaTime * 10);
        }
        #endregion

        if (moving || slide)
        {
            Vector3 d = ((transform.forward * dir.y) + (transform.right * dir.x)) * current_speed * Time.fixedDeltaTime;
            Vector3 with_y_speed = new Vector3(d.x, rb.velocity.y, d.z);
            rb.velocity = with_y_speed;
        }
        else if (!moving || !slide)
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }

    private void Update()
    {
        if (transform.position.y < 0)
            transform.position = start_position;

        if (slide || slide_timer > 0)
        {
            slide_timer += Time.deltaTime;
            if (slide_timer > 0.75f)
            {
                can_slide = true;
                slide_timer = 0;
            }
        }
    }

    public void Sprint(InputAction.CallbackContext con)
    {
        if (con.performed)
        {
            if (dir.y != 0 || slide || (sprinting && !moving))
            {
                sprinting = !sprinting;
                if (sprinting)
                    target_speed = speed * 1.5f;
            }
        }
    }

    public void Slide(InputAction.CallbackContext con)
    {
        // Compueba si puede hacer el slide y está encima de algo
        if (con.performed && can_slide && Physics.Raycast(transform.position, Vector2.down, transform.localScale.y / 2 + 0.5f))
        {
            can_slide = false;
            slide = true;
            if (!sprinting)
                current_speed = target_speed * 7;
            else
                current_speed = target_speed * 2;
        }
        if (con.canceled)
        {
            current_speed = target_speed;
            slide = false;
            if (!moving)
                dir = Vector2.zero;
        }
    }

    public void Move(InputAction.CallbackContext con)
    {
        if (rb == null) return;
        if (con.performed)
        {
            moving = true;
        }
        else if (con.canceled)
        {
            moving = false;
        }

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

    private void OnCollisionEnter(Collision other)
    {
        if (slide && other.gameObject.CompareTag("Enemy")) // Mientras se desliza empujará a los enemigos
        {
            Vector3 dir = (other.transform.position - transform.position).normalized; /// Calcula la dirección entre tú y el enemigo
            Vector3 force_dir = new Vector3(dir.x, 0.5f, dir.z);

            other.gameObject.GetComponent<EnemyFollow>().AddForceToEnemy(force_dir * (current_speed * 0.02f));
        }
    }
}