using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    public Image cooldown_image;
    public GameObject explosion_particle;

    [Header("Sonidos")]
    public AudioClip return_clip;
    public AudioClip explosion_clip, funny_explosion_clip, tictac_clip, nautilus_explosion;
    public AudioClip cooldown_ready_clip, cooldown_not_ready_clip;

    [Header("Positions record")]
    public List<Vector3> past_positions;
    public List<Vector2> past_rotations;
    public List<Quaternion> q_rotations;

    [HideInInspector]
    public bool returning = false, cooldown = false;
    private float timer = 0, cooldown_timer = 0;
    private int current_point = 0;

    private CameraRotation cameraRotation;

    private PlayerLife playerLife;

    [Header("Colors")]
    public Color color_cooldown_ready;
    public Color color_on_cooldown;

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
            // Calcula la distancia hasta el último punto en un rango de 1 a 0
            cooldown_image.fillAmount = 1 - (1 - (Vector3.Distance(transform.position, past_positions[0]) / Vector3.Distance(past_positions[0], past_positions[past_positions.Count - 1])));
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
                    cooldown_image.fillAmount = 0;
                    #region DamageToEnemy
                    if (SoundManager.instance.funnySounds) /// Sonidos de explosión
                    {
                        SoundManager.instance.InstantiateSound(funny_explosion_clip, transform.position, funny_explosion_clip.length);
                    }
                    else
                    {
                        SoundManager.instance.InstantiateSound(explosion_clip, transform.position, explosion_clip.length);
                    }
                    GameManager.gm.ShakeController(1, 0.5f, 1.5f);
                    DamageToEnemies(transform.position, damage, explosion_range, Vector3.zero);
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
        else
        {
            if (cooldown) // Cuenta atrás del cooldown
            {
                if (cooldown_timer == 0) ClearReturnLists(); /// Vacía la lista de posiciones
                cooldown_timer += Time.deltaTime;
                cooldown_image.fillAmount += Time.deltaTime / cooldown_time; /// Suma la cantidad de fill a la imágen del cooldown
                cooldown_image.color = Color.Lerp(cooldown_image.color, color_on_cooldown, Time.deltaTime * 1.75f); /// Cambia el color de la imágen al de cooldown
                if (cooldown_timer > cooldown_time)
                {
                    SoundManager.instance.PlaySound(cooldown_ready_clip);
                    cooldown = false;
                    cooldown_timer = 0;
                }
            }
            else if (cooldown_image.color != color_cooldown_ready)
            {
                cooldown_image.color = Color.Lerp(cooldown_image.color, color_cooldown_ready, Time.deltaTime * 1.5f); /// Cambia el color de la imágen al normal
            }
            if (timer > max_time / 10) /// Va actualizando las posiciones del jugador cada cierto tiempo
            {
                UpdateReturnList();
                timer = 0;
            }
            else
            {
                timer += Time.deltaTime;
            }
        }
    }

    private void DamageToEnemies(Vector3 origin, int damage_amount, float range, Vector3 dir)
    {
        Collider[] colls = Physics.OverlapSphere(origin, range, enemyMask);
        if (colls.Length > 0)
        {
            foreach (Collider coll in colls)
            {
                Vector3 d = (coll.transform.position - transform.position).normalized;
                dir += d;
                coll.GetComponent<EnemyFollow>().AddForceToEnemy(dir * (damage_amount * 1.25f), ForceMode.Impulse);
                coll.GetComponent<EnemyLife>().Damage(damage_amount);
                playerLife.Damage(-1);
            }
        }
    }

    public void ReturnToLastPosition(InputAction.CallbackContext con)
    {
        if (con.performed) // Se ha pulsado espacio y no está en cooldown
        {
            if (!cooldown && !returning)
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
            else /// El jugador ha terminado la habilidad antes
            {
                past_positions[0] = transform.position;
                current_point = 0;
            }
            if (cooldown)
                SoundManager.instance.PlaySound(cooldown_not_ready_clip);
        }
    }

    private void UpdateReturnList()
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

    #region NautilusR
    public void ExplosionPathAbility(GameObject explosionParticle)
    {
        StartCoroutine(ExplosionPath(explosionParticle));
    }
    IEnumerator ExplosionPath(GameObject explosionParticle)
    {
        List<Vector3> positions = new List<Vector3>(past_positions);
        int explosion_num = positions.Count - 1;
        float explosion_timer = 0;
        while (explosion_num > 0)
        {
            explosion_timer += Time.deltaTime;
            if (explosion_timer > 0.5f)
            {
                Instantiate(explosionParticle, positions[explosion_num], Quaternion.identity);
                DamageToEnemies(positions[explosion_num], (int)(damage * 0.25f), 3, Vector3.up * 2);
                float dist = Vector3.Distance(positions[explosion_num], transform.position);
                if (dist < 10) /// Si el jugador está cerca hará vibrár el mando
                {   
                    if (nautilus_explosion != null) /// Sonido de explosión
                        SoundManager.instance.InstantiateSound(nautilus_explosion, positions[explosion_num], nautilus_explosion.length);
                    GameManager.gm.ShakeController(0.2f + (1 - (dist / 10)), 0.01f, (1 + (1 - (dist / 10))) * 2);
                }
                explosion_num--;
                explosion_timer = 0;
            }

            yield return null;
        }
    }
    #endregion

    private void OnDrawGizmos()
    {
        for (int i = 0; i < past_positions.Count; i++)
        {
            Gizmos.DrawWireSphere(past_positions[i], 1);
        }
    }
}