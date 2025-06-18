using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class sc_bate : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public LayerMask enemy_Layer;

    public bool isSwinging = false;

    public Animator anim;
    private TrailRenderer trailRenderer;

    public ParticleSystem ps_Hit;

    private MeshRenderer mesh_r;

    [Header("Sonido")]
    public AudioClip clip_swing, clip_hit, clip_charged, clip_PlasticHit;

    private AudioSource audioSource;

    public float z_rot;
    [Header("Stats")]
    public bool canSwing = true;

    private List<string> hitted_gameobjects = new List<string>();

    public string attack_name = "anim_player_bate_attack";

    private float timer = 0, swing_timer = 0;
    private bool cooldown = false;

    CameraRotation cam_rot;

    private Rigidbody rb;

    public bool charge_attack;
    private float charge_timer = 0;

    private bool big_attack = false;

    private void Start()
    {
        cam_rot = CameraRotation.instance;
        rb = PlayerMovement.instance.gameObject.GetComponent<Rigidbody>();

        mesh_r = GetComponent<MeshRenderer>();

        trailRenderer = GetComponentInChildren<TrailRenderer>();

        anim.SetBool("has_bat", canSwing);
    }

    private void Update()
    {
        if (charge_attack)
        {
            charge_timer += Time.deltaTime;
            anim.speed = charge_timer;
            if (!big_attack && charge_timer > 1.5f)
            {
                SoundManager.instance.InstantiateSound(clip_charged, transform.position, 0.5f, null, true, Random.Range(0.75f, 1.25f));
                StartCoroutine(ColorPulse());
                big_attack = true;

            }
        }
        if (isSwinging)
        {
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

            // Mira si la animación ha terminado
            swing_timer += Time.deltaTime;
            if (swing_timer >= 0.75f)
            {
                Time.timeScale = 1;
                anim.SetBool("bat_attack", false);
                anim.SetBool("bat_charged", false);
                trailRenderer.emitting = false;

                hitted_gameobjects.Clear();

                cooldown = true;
                isSwinging = false;
                big_attack = false;

                swing_timer = 0;
            }
        }
        else if (cooldown)
        {
            timer += Time.deltaTime;
            if (timer > 0.25f)
            {
                cooldown = false;
                timer = 0;
            }
        }
    }

    public void OnSwing(InputAction.CallbackContext con)
    {
        if (!cooldown && con.performed && gameObject.activeSelf && canSwing && !charge_attack && !isSwinging && PlayerMovement.instance.current_stamina - 1 > 0)
        {
            charge_attack = true;
            big_attack = false;
            anim.SetBool("bat_charge", true);
        }

        if (!cooldown && charge_attack && con.canceled)
        {
            charge_attack = false;
            //big_attack = false;

            charge_timer = 0;
            anim.SetBool("bat_charge", false);
            isSwinging = true;

            Swing();
        }
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            AttackEnemy(other);
        }
        else if (isSwinging && other.CompareTag("Box"))
        {
            Rigidbody rbs = other.GetComponent<Rigidbody>();
            if (rbs != null)
            {
                SoundManager.instance.InstantiateSound(clip_PlasticHit, other.transform.position);

                Vector3 dir = (other.transform.position - transform.position).normalized;
                rbs.AddForce(dir * (big_attack ? 20 : 5), ForceMode.Impulse);
                if (big_attack)
                {
                    Collider[] colls = Physics.OverlapSphere(transform.position, 2);
                    for (int i = 0; i < colls.Length; i++)
                    {
                        if (colls[i].tag == "Box")
                        {
                            colls[i].GetComponent<Rigidbody>().AddExplosionForce(20, transform.position, 2);
                            SoundManager.instance.InstantiateSound(clip_PlasticHit, colls[i].transform.position, 0.75f, null, true, Random.Range(0.5f, 0.75f));
                        }
                    }
                }


                rb.AddForce(-player.forward * (big_attack ? 3 : 1), ForceMode.Impulse);
            }
        }
    }

    void Swing()
    {
        isSwinging = true;
        charge_attack = false;
        charge_timer = 0;

        if (big_attack)
            anim.SetBool("bat_charged", true);
        anim.SetBool("bat_attack", true);


        trailRenderer.emitting = true;
        anim.speed = 1;

        Color col = PlayerMovement.instance.stamina_image.color;
        PlayerMovement.instance.stamina_image.color = new Color(col.r, col.g, col.b, 1);

        PlayerMovement.instance.ChangeStaminaValue(PlayerMovement.instance.current_stamina - (big_attack ? 3 : 1));
        if (PlayerMovement.instance.current_stamina < 0)
            PlayerMovement.instance.current_stamina = 0;

        SoundManager.instance.InstantiateSound(clip_swing, transform.position);


        Collider[] colls = Physics.OverlapSphere(player.position + player.forward, 2, enemy_Layer);
        if (colls.Length > 0)
        {
            for (int i = 0; i < colls.Length; i++)
            {
                if (colls[i] != null)
                    AttackEnemy(colls[i]);
            }
        }
    }

    /// <summary>
    /// Ataca al collider indicado
    /// </summary>
    void AttackEnemy(Collider other)
    {
        if (!isSwinging || hitted_gameobjects.Contains(other.transform.parent != null ? other.transform.parent.name : other.name)) return;

        float knockback = 2;
        if (big_attack)
        {
            StartCoroutine(SlowTime());
            knockback = 4;
        }
        rb.AddForce(-player.forward * knockback, ForceMode.Acceleration);

        Vector3 dir = (other.transform.position - player.position).normalized;
        Vector3 forceDir = new Vector3(dir.x, Random.Range(0.25f, 0.5f), dir.z);

        Destroy(Instantiate(ps_Hit, other.ClosestPoint(transform.position), Quaternion.LookRotation(-dir)), 5);

        float force = 8.5f;
        if (big_attack)
            force = 12;
        other.GetComponent<EnemyFollow>().AddForceToEnemy(forceDir * force);
        other.GetComponent<Rigidbody>().AddTorque(forceDir * (force / 4));
        int damage = big_attack ? (int)(ReturnScript.instance.damage * 1.5f) : (int)(ReturnScript.instance.damage * 0.5f);
        other.GetComponent<EnemyLife>().Damage(damage);

        ScoreManager.instance.InstantiateText("-" + (ReturnScript.instance.damage * 0.5f).ToString("F0"), Camera.main.transform.position + Camera.main.transform.forward * 0.25f, dir, 65, 3, Color.red);

        if (big_attack)
        {
            GameManager.gm.ShakeController(0.15f, 1, 0.05f);

            CameraRotation.instance.ShakeCamera(0.5f, 1);
            PlayerMovement.instance.rb.AddForce(-player.forward * ReturnScript.instance.damage * 2, ForceMode.Impulse);
        }
        else
        {
            GameManager.gm.ShakeController(0.1f, 0.05f, 1);

            CameraRotation.instance.ShakeCamera(0.15f, 1);
            PlayerMovement.instance.rb.AddForce(-player.forward * ReturnScript.instance.damage, ForceMode.Impulse);
        }
        

        // Random pitch para variar el sonido de golpeo
        float rand_pitch = Random.Range(0.75f, 1.26f);
        if (big_attack)
            rand_pitch = Random.Range(0.25f, 0.5f);

        if (audioSource == null)
        {
            audioSource = SoundManager.instance.InstantiateSound(clip_hit, transform.position, 0.5f, null, true, rand_pitch);
        }
        else if (!audioSource.isPlaying)
        {
            audioSource = SoundManager.instance.InstantiateSound(clip_hit, transform.position, 0.5f, null, true, rand_pitch);
        }

        hitted_gameobjects.Add(other.transform.parent != null ? other.transform.parent.name : other.name);
    }

    public void SetSwing(bool value)
    {
        canSwing = value;
        anim.SetBool("has_bat", canSwing);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(player.position + transform.forward, 1);
    }

    private IEnumerator SlowTime()
    {
        yield return new WaitForSecondsRealtime(0.25f);
        Time.timeScale = 0.1f;
        
        while (Time.timeScale < 1)
        {
            Time.timeScale += Time.unscaledDeltaTime;
            yield return null;
        }
        Time.timeScale = 1;
    }

    private IEnumerator ColorPulse()
    {
        float blend = 0;

        while (charge_attack)
        {
            while (blend < 1)
            {
                blend += Time.deltaTime * 10;
                mesh_r.material.SetFloat("_BlendAmount", blend);

                yield return null;
            }
            while (blend > 0)
            {
                blend -= Time.deltaTime * 10;
                if (blend < 0) blend = 0;
                mesh_r.material.SetFloat("_BlendAmount", blend);

                yield return null;
            }
        }
        mesh_r.material.SetFloat("_BlendAmount", 0);
    }
}