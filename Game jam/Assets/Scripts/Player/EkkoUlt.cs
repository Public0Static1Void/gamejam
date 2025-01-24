using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EkkoUlt : MonoBehaviour
{
    public static EkkoUlt instance { get; private set; }
    [Header("Stats")]
    public float period_time;
    public int damage;
    public float explosion_radius;

    public List<Vector3> playerPositions;
    private Vector3 return_position;

    [HideInInspector]
    public bool mark_placed, return_to_pos;

    private float timer = 0;

    [Header("References")]
    public GameObject follower_model;
    public Transform returnMark;
    public UnityEngine.UI.Image countdown_player;
    public LayerMask enemy_layer;
    [Header("Sonidos")]
    public AudioClip return_clip;
    public AudioClip explosion_clip, funny_explosion_clip, tictac_clip;
    public GameObject explosion_particle;

    private PlayerLife playerLife;

    private UnityEngine.UI.Image orb_cooldown;

    private int current_checkpoint = 0;

    private float last_distance = 0;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }
    void Start()
    {
        playerPositions.Add(transform.position);
        follower_model.transform.position = transform.position;

        orb_cooldown = Instantiate(countdown_player.gameObject, countdown_player.transform.parent).GetComponent<UnityEngine.UI.Image>();
        orb_cooldown.enabled = false;

        playerLife = GetComponent<PlayerLife>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !mark_placed) { 
            StartCoroutine(WaitToReturn());
        }

        if (mark_placed)
        {
            countdown_player.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            countdown_player.fillAmount += Time.deltaTime / period_time;
            orb_cooldown.fillAmount += Time.deltaTime / period_time;
        }

        if (timer < period_time * 0.05f)
        {
            timer += Time.deltaTime;
        }
        else
        {
            if (playerPositions.Count > 100)
            {
                playerPositions.Clear();
                playerPositions.Add(transform.position);
                current_checkpoint = 0;
            }
            if (playerPositions.Count > 0 && transform.position != playerPositions[playerPositions.Count - 1])
                playerPositions.Add(transform.position);
            timer = 0;
        }

        // Follower logic
        if (playerPositions.Count > 1 && current_checkpoint < playerPositions.Count)
        {
            if (Vector3.Distance(follower_model.transform.position, playerPositions[current_checkpoint]) <= 2)
            {
                ChangeFollowerObjective();
            }
            else
            {
                //follower_model.transform.position = Vector3.Lerp(follower_model.transform.position, playerPositions[current_checkpoint], (PlayerMovement.instance.speed * 0.0025f) * Time.deltaTime);
                follower_model.transform.Translate(follower_model.transform.forward * ((Vector3.Distance(follower_model.transform.position, playerPositions[current_checkpoint]) + PlayerMovement.instance.speed) * 0.013f) * Time.deltaTime, Space.World);
            }
        }
        Debug.Log(Vector3.Distance(transform.position, follower_model.transform.position));
        if (Vector3.Distance(transform.position, follower_model.transform.position) > 25 || current_checkpoint >= playerPositions.Count)
        {
            playerPositions.Clear();
            playerPositions.Add(transform.position);
            current_checkpoint = 0;
            follower_model.transform.position = transform.position;
        }
        if (return_to_pos)
        {
            if (Vector3.Distance(transform.position, return_position) > 1)
            {
                transform.position = Vector3.Lerp(transform.position, return_position, Time.deltaTime * 10);
            }
            else
            {
                // --- Aquí es cuando el jugador vuelve totalmente a la posición inicial ------------------------------------------------------------------

                GameObject part = Instantiate(explosion_particle);
                part.transform.position = return_position;
                Destroy(part, 5); ///Instancia las partículas

                if (SoundManager.instance.funnySounds)
                {
                    SoundManager.instance.InstantiateSound(funny_explosion_clip, transform.position, funny_explosion_clip.length);
                }
                else
                {
                    SoundManager.instance.InstantiateSound(explosion_clip, transform.position, explosion_clip.length);
                }
                Collider[] colls = Physics.OverlapSphere(transform.position, explosion_radius, enemy_layer);
                if (colls.Length > 0)
                {
                    foreach (Collider coll in colls)
                    {
                        coll.GetComponent<Rigidbody>().AddExplosionForce(damage, transform.position, explosion_radius);
                        coll.GetComponent<EnemyLife>().Damage(damage);
                        playerLife.Damage(-1);
                    }
                }
                transform.position = return_position;
                GetComponent<Rigidbody>().useGravity = true;
                GetComponent<SphereCollider>().isTrigger = false;
                GetComponent<Rigidbody>().isKinematic = false;
                PlayerMovement.instance.canMove = true;
                return_to_pos = false;
            }
        }
    }

    public void ChangeFollowerObjective()
    {
        if (current_checkpoint < playerPositions.Count - 2)
            current_checkpoint++;

        last_distance = Vector3.Distance(playerPositions[current_checkpoint], follower_model.transform.position);
        Vector3 dir = playerPositions[current_checkpoint] - follower_model.transform.position;
        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
        follower_model.transform.rotation = rot;
    }

    public IEnumerator WaitToReturn()
    {
        mark_placed = true;
        countdown_player.fillAmount = 0;
        orb_cooldown.fillAmount = 0;
        countdown_player.enabled = true;
        orb_cooldown.enabled = true;

        // Activa los followers
        returnMark.transform.gameObject.SetActive(true);

        return_position = follower_model.transform.position;
        returnMark.position = return_position;

        orb_cooldown.transform.position = return_position;

        /// Sonido de tic tac
        SoundManager.instance.InstantiateSound(tictac_clip, return_position, period_time);

        yield return new WaitForSeconds(period_time); /// Espera del código

        SoundManager.instance.PlaySound(return_clip); // haz que suene el sonido para volver

        // Evita que el jugador se mueva
        PlayerMovement.instance.canMove = false;
        PlayerMovement.instance.rb.velocity = Vector3.zero;

        GetComponent<SphereCollider>().isTrigger = true; /// Evita que cambie el rumbo por colisiones
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().isKinematic = true;

        return_to_pos = true;

        countdown_player.enabled = false;
        orb_cooldown.enabled = false;

        // Desactiva los followers
        returnMark.transform.gameObject.SetActive(false);

        mark_placed = false;
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
        period_time -= 1;
        if (period_time <= 2) period_time = 2;
    }
    public void IncreaseDamage()
    {
        damage += 1;
    }
    public void IncreaseExplosionRange()
    {
        explosion_radius += 0.5f;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, explosion_radius);
    }
}