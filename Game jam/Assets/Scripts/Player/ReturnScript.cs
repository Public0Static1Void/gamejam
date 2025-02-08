using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ReturnScript : MonoBehaviour
{
    public static ReturnScript instance;
    [Header("Stats")]
    public float return_speed;
    public float max_time;
    public float explosion_range;
    public int damage;
    public float cooldown_time;

    [Header("References")]
    public LayerMask enemyMask;
    public UnityEvent ability;

    [Header("Sonidos")]
    public AudioClip return_clip;
    public AudioClip explosion_clip, funny_explosion_clip, tictac_clip;
    public GameObject explosion_particle;

    [Header("Positions record")]
    public List<Vector3> past_positions;
    public List<Vector2> past_rotations;
    public List<Quaternion> q_rotations;

    private bool returning = false, cooldown = false;
    private float timer = 0, cooldown_timer = 0;
    private int current_point = 0;

    private CameraRotation cameraRotation;

    private PlayerLife playerLife;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }
    void Start()
    {
        cameraRotation = Camera.main.GetComponent<CameraRotation>();
        playerLife = GetComponent<PlayerLife>();

        past_positions = new List<Vector3>
        {
            transform.position
        };
        past_rotations = new List<Vector2>
        {
            new Vector2(cameraRotation.x, cameraRotation.y)
        };
        q_rotations = new List<Quaternion>
        {
            transform.rotation
        };
    }

    void Update()
    {
        if (returning && past_positions.Count > 0 && !cooldown)
        {
            if (Vector3.Distance(transform.position, past_positions[current_point]) > 0.1f)
            {
                Vector3 dir = (past_positions[current_point] - transform.position).normalized;
                transform.Translate(dir * (return_speed * 0.75f + Vector3.Distance(transform.position, past_positions[current_point])) * Time.deltaTime, Space.World); /// Mueve al jugador en la dirección a su anterior posición
                
                cameraRotation.x = past_rotations[current_point].x; /// Cambia la rotación del script de la cámara, para que no haga cosas raras al acabar la transición
                cameraRotation.y = past_rotations[current_point].y;

                transform.rotation = Quaternion.Lerp(transform.rotation, q_rotations[current_point], Time.deltaTime * return_speed);
                Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, Quaternion.Euler(-cameraRotation.y, cameraRotation.x, 0), Time.deltaTime * return_speed * 0.5f);
            }
            else
            {
                if (current_point > 0)
                {
                    current_point--;
                }
                else
                {
                    // El jugador ha llegado al último punto
                    ClearReturnLists();


                    #region DamageToEnemies
                    if (SoundManager.instance.funnySounds)
                    {
                        SoundManager.instance.InstantiateSound(funny_explosion_clip, transform.position, funny_explosion_clip.length);
                    }
                    else
                    {
                        SoundManager.instance.InstantiateSound(explosion_clip, transform.position, explosion_clip.length);
                    }
                    Collider[] colls = Physics.OverlapSphere(transform.position, explosion_range, enemyMask);
                    if (colls.Length > 0)
                    {
                        GameManager.gm.ShakeController(1, 1, 3);

                        foreach (Collider coll in colls)
                        {
                            Vector3 dir = (coll.transform.position - transform.position).normalized;
                            Rigidbody enemy_rb = coll.GetComponent<Rigidbody>();
                            enemy_rb.isKinematic = false;
                            enemy_rb.AddForce(dir * (damage * 2.5f), ForceMode.VelocityChange);
                            coll.GetComponent<EnemyLife>().Damage(damage);
                            playerLife.Damage(-1);
                        }
                    }
                    #endregion


                    playerLife.Invulnerable(); /// Hace que el jugador sea invulnerable cuando acaba de llegar
                    PlayerMovement.instance.canMove = true;
                    GetComponent<Rigidbody>().isKinematic = false;
                    GetComponent<Collider>().isTrigger = false;

                    returning = false;
                    cooldown = true;
                    timer = 0;
                }
            }
        }
        else if (cooldown) // Cuenta atrás del cooldown
        {
            cooldown_timer += Time.deltaTime;
            if (cooldown_timer > cooldown_time)
            {
                cooldown = false;
                cooldown_timer = 0;
            }
        }
        else
        {
            if (timer > max_time / 10)
            {
                if (past_positions.Count > 0)
                {
                    if (past_positions.Count >= 10)
                    {
                        past_positions.RemoveAt(0);
                        past_rotations.RemoveAt(0);
                        q_rotations.RemoveAt(0);
                    }
                    past_positions.Add(transform.position);
                    past_rotations.Add(new Vector2(cameraRotation.x, cameraRotation.y));
                    q_rotations.Add(transform.rotation);
                }
                timer = 0;
            }
            else
            {
                timer += Time.deltaTime;
            }
        }
    }

    public void ReturnToLastPosition(InputAction.CallbackContext con)
    {
        if (con.performed && !cooldown && !returning) // Se ha pulsado espacio y no está en cooldown
        {
            returning = true;
            GetComponent<Rigidbody>().isKinematic = true; /// Deshabilita el rigidbody y las colisiones para evitar problemas al volver
            GetComponent<Collider>().isTrigger = true;
            PlayerMovement.instance.canMove = false; /// Evita que el jugador pueda moverse mientras vuelve
            current_point = past_positions.Count - 1;

            if (ability != null) /// Ejecuta la habilidad del player
            {
                ability.Invoke();
            }
        }
    }

    private void ClearReturnLists()
    {
        past_positions.Clear();
        past_rotations.Clear();
        q_rotations.Clear();

        past_positions.Add(transform.position);
        past_rotations.Add(new Vector2(cameraRotation.x, cameraRotation.y));
        q_rotations.Add(transform.rotation);
    }

    public void RandomUpgrade()
    {
        int rand = Random.Range(0, 2);
        switch (rand)
        {
            case 0:
                GameManager.gm.ShowText("Period decreased!");
                DecreasePeriod();
                break;
            case 1:
                GameManager.gm.ShowText("Damage increased!");
                IncreaseDamage();
                break;
            case 2:
                GameManager.gm.ShowText("Explosion range increased!");
                IncreaseExplosionRange();
                break;
        }
    }
    public void DecreasePeriod()
    {
        max_time -= 1;
        if (max_time <= 2) max_time = 2;
    }
    public void IncreaseDamage()
    {
        damage += 1;
    }
    public void IncreaseExplosionRange()
    {
        explosion_range += 0.5f;
    }



    private void OnDrawGizmos()
    {
        for (int i = 0; i < past_positions.Count; i++)
        {
            Gizmos.DrawWireSphere(past_positions[i], 1);
        }
    }
}