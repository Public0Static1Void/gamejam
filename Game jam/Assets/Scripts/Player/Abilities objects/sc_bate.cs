using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class sc_bate : MonoBehaviour
{

    public Transform player;

    public LayerMask enemy_Layer;

    public bool isSwinging = false;

    public Animator anim;

    public AudioClip clip_swing, clip_hit;

    private AudioSource audioSource;

    public float z_rot;

    public bool canSwing = true;

    private List<string> hitted_gameobjects = new List<string>();

    public string attack_name = "anim_player_bate_attack";

    private float timer = 0;
    private bool cooldown = false;

    CameraRotation cam_rot;

    private Rigidbody rb;

    private void Start()
    {
        cam_rot = CameraRotation.instance;
        rb = PlayerMovement.instance.gameObject.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (isSwinging)
        {
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

            // Mira si la animación ha terminado
            if (stateInfo.IsName(attack_name) && stateInfo.normalizedTime >= 0.8f)
            {
                anim.SetBool("bat_attack", false);

                hitted_gameobjects.Clear();

                cooldown = true;
                isSwinging = false;
            }

            Collider[] colls = Physics.OverlapSphere(player.position, 2, enemy_Layer);
            if (colls.Length > 0)
            {
                for (int i = 0; i < colls.Length; i++)
                {
                    AttackEnemy(colls[i]);
                }
            }
        }
        else if (cooldown)
        {
            timer += Time.deltaTime;
            if (timer > 0.5f)
            {
                cooldown = false;
                timer = 0;
            }
        }
    }

    public void OnSwing(InputAction.CallbackContext con)
    {
        if (!cooldown && con.performed && gameObject.activeSelf && canSwing && !isSwinging && PlayerMovement.instance.current_stamina - PlayerMovement.instance.max_stamina * 0.1f > 0)
        {
            isSwinging = true;
            anim.SetBool("bat_attack", true);

            Color col = PlayerMovement.instance.stamina_image.color;
            PlayerMovement.instance.stamina_image.color = new Color(col.r, col.g, col.b, 1);

            PlayerMovement.instance.ChangeStaminaValue(PlayerMovement.instance.current_stamina - 1);
            if (PlayerMovement.instance.current_stamina < 0)
                PlayerMovement.instance.current_stamina = 0;

            SoundManager.instance.InstantiateSound(clip_swing, transform.position);

            rb.AddForce(-player.forward * 2, ForceMode.Acceleration);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            AttackEnemy(other);
        }
    }

    /// <summary>
    /// Ataca al collider indicado
    /// </summary>
    void AttackEnemy(Collider other)
    {
        if (!isSwinging || hitted_gameobjects.Contains(other.transform.parent != null ? other.transform.parent.name : other.name)) return;

        Vector3 dir = (other.transform.position - player.position).normalized;
        Vector3 forceDir = new Vector3(dir.x, 0.5f, dir.z);

        other.GetComponent<EnemyFollow>().AddForceToEnemy(forceDir * 8.5f);
        other.GetComponent<Rigidbody>().AddTorque(forceDir * 2);
        other.GetComponent<EnemyLife>().Damage((int)(ReturnScript.instance.damage * 0.5f));

        ScoreManager.instance.InstantiateText("-" + (ReturnScript.instance.damage * 0.5f).ToString("F0"), Camera.main.transform.position + Camera.main.transform.forward * 0.25f, dir, 65, 3, Color.red);

        GameManager.gm.ShakeController(0.1f, 0.05f, 1);

        CameraRotation.instance.ShakeCamera(0.25f, 0.05f);

        if (audioSource == null)
        {
            audioSource = SoundManager.instance.InstantiateSound(clip_hit, transform.position);
        }
        else if (!audioSource.isPlaying)
        {
            audioSource = SoundManager.instance.InstantiateSound(clip_hit, transform.position);
        }

        hitted_gameobjects.Add(other.transform.parent != null ? other.transform.parent.name : other.name);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(player.position, 1);
    }
}