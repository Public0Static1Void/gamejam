using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
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
    public float fov_change;

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
    public AudioClip slide_sound;
    private float slide_sprinting_multiplier = 2.1f, slide_walking_multiplier = 7;

    public bool onGround = false;

    [Header("References")]
    public Image stamina_image;
    public AudioSource audio_source;
    public ParticleSystem particle_slide;
    public LayerMask layer_ground;

    private NavMeshAgent player_agent;
    
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

        can_slide = true; /// El jugador puede deslizarse al empezar

        fov = Camera.main.fieldOfView;

        rb.freezeRotation = true;

        camera_original_position = Camera.main.transform.localPosition;

        cameraRotation = Camera.main.gameObject.GetComponent<CameraRotation>();

        player_agent = GetComponent<NavMeshAgent>();
        player_agent.updatePosition = false;
        player_agent.updateRotation = false;
    }

    private void FixedUpdate()
    {
        if (!canMove) return;

        onGround = Physics.Raycast(transform.position, Vector3.down, transform.localScale.y * 2);

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
                    Color col = new Color(stamina_image.color.r, stamina_image.color.g, stamina_image.color.b, stamina_image.color.a + Time.fixedDeltaTime);
                    stamina_image.color = col;
                }
                stamina_image.fillAmount = 1 - (1 - (current_stamina / max_stamina));
                /// Resta de stamina mientras corres
                current_stamina -= Time.fixedDeltaTime;

                // Suma de velocidad
                current_speed = Mathf.Lerp(current_speed, target_speed, (speed * 0.1f) * Time.fixedDeltaTime);
            }

            //Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, fov + fov_change, Time.deltaTime * 8);
        }
        else if (current_speed > target_speed + 0.05f || Camera.main.fieldOfView > fov + 0.05f) /// baja la velocidad y el fov progresivamente
        {
            current_speed = Mathf.Lerp(current_speed, target_speed, (speed * 0.05f) * Time.deltaTime);
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, fov, Time.deltaTime * 6);
        }
        else if (current_stamina < max_stamina) /// Recupera la stamina
        {
            current_stamina += Time.fixedDeltaTime;
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

            /// Pérdida de velocidad
            if (rb.velocity.y >= -0.1f)
            {
                current_speed -= (Time.fixedDeltaTime * ((0.5f * current_speed + target_speed)));
            }
            else // El jugador está descendiendo una pendiente
            {
                current_speed += Time.fixedDeltaTime * ((sprinting ? 0.5f : 5) * current_speed + target_speed);
                if (current_speed > 800) current_speed = 800;
            }

            // El jugador ha perdido bastante velocidad o no está en el suelo
            if (current_speed <= speed * 0.25f || !onGround)
            {
                /// Deja de sonar el sonido de slide y se quita el loop
                SoundManager.instance.PlaySound(null);
                SoundManager.instance.audioSource.volume = 0.5f;
                audio_source.UnPause();

                particle_slide.Stop();

                /// Asegura que el jugador mantiene su posición después de activar el agente
                Vector3 pos = transform.position;
                player_agent.enabled = true;
                transform.position = pos;

                current_speed = target_speed;
                slide = false;
            }

            /// Cambia el volumen según la velocidad
            SoundManager.instance.audioSource.volume = 1 - (1 - (current_speed / speed));
        }
        else if (Camera.main.transform.position != camera_original_position)
        {
            Camera.main.transform.localPosition = Vector3.Lerp(Camera.main.transform.localPosition, camera_original_position, Time.deltaTime * 10);
        }
        #endregion

        // Aplicación del movimiento
        if (moving || slide)
        {
            Vector3 d = ((transform.forward * dir.y) + (transform.right * dir.x)) * current_speed * Time.fixedDeltaTime;
            Vector3 with_y_speed = new Vector3(d.x, rb.velocity.y, d.z);
            rb.velocity = with_y_speed;
        }
        else if (!moving || !slide)
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            if (sprinting) sprinting = false;
        }
    }

    private void Update()
    {
        if (transform.position.y < 0)
            transform.position = start_position;

        if (sprinting || slide)
        {
            fov_change = (current_speed / speed) * 10;

            // Cambio del fov según si corres o no
            float new_fov = Mathf.Lerp(Camera.main.fieldOfView, fov + fov_change, Time.deltaTime * (!sprinting ? 2 : 6));

            // Si el cambio no es mayor a 3 no cambiará el fov para evitar caída de fps
            float limit_cap = 0.5f;
            if (sprinting) limit_cap = 0.1f;

            if (Mathf.Abs(new_fov - Camera.main.fieldOfView) > limit_cap)
            {
                Camera.main.fieldOfView = new_fov;
            }
        }

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
                {
                    target_speed = speed * 1.5f;
                    audio_source.pitch = 1.5f;
                }
                else
                {
                    audio_source.pitch = 1;
                }
            }
        }
    }

    public void Slide(InputAction.CallbackContext con)
    {
        // Compueba si puede hacer el slide, está encima de algo y puede moverse
        if (con.performed)
        {
            player_agent.enabled = false;
            if (can_slide && onGround && canMove)
            {
                can_slide = false;
                slide = true;

                cameraRotation.cameraSpeed = cameraRotation.cameraSpeed_slide; /// La cámara no podrá moverse tanto mientras te deslizas

                SoundManager.instance.PlaySound(slide_sound, true);
                audio_source.Pause();

                particle_slide.Play();

                if (!sprinting)
                    current_speed = target_speed * slide_walking_multiplier;
                else
                    current_speed = target_speed * slide_sprinting_multiplier;
            }
        }
        
        if (con.canceled)
        {
            cameraRotation.cameraSpeed = cameraRotation.cameraSpeed_slide * 5; /// La cámara vuelve a su velocidad original

            // Deja de sonar el sonido de slide y se quita el loop
            SoundManager.instance.PlaySound(null);
            SoundManager.instance.audioSource.volume = 0.5f;

            particle_slide.Stop();

            /// Asegura que el jugador mantiene su posición después de activar el agente
            Vector3 pos = transform.position;
            player_agent.enabled = true;
            transform.position = pos;

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
            audio_source.Play();
        }
        else if (con.canceled)
        {
            moving = false;
            audio_source.Stop();
        }

        dir = con.ReadValue<Vector2>();

        if (slide) return;

        //int target_fov = 20;
        if (dir.y >= 0.7f)
        {
            current_speed = speed;
            //fov_change = target_fov;
        }
        else if (dir.y < 0)
        {
            current_speed = speed * 0.5f;
            //fov_change = target_fov * 0.45f;
        }
        else 
        { 
            current_speed = speed * 0.65f;
            //fov_change = target_fov * 0.6f;
        }
        
        target_speed = current_speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (slide && other.gameObject.CompareTag("Enemy")) // Mientras se desliza empujará a los enemigos
        {
            Vector3 dir = (other.transform.position - transform.position).normalized; /// Calcula la dirección entre tú y el enemigo
            Vector3 force_dir = new Vector3(dir.x, 0.5f, dir.z);

            other.gameObject.GetComponent<EnemyFollow>().AddForceToEnemy(force_dir * (current_speed * 0.02f));
        }
    }
}