using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EkkoUlt : MonoBehaviour
{
    [Header("Stats")]
    public float period_time;
    public int damage;
    public float explosion_radius;

    public List<Vector3> playerPositions;
    private Vector3 return_position;


    private bool mark_placed, return_to_pos;

    private float timer = 0;

    [Header("References")]
    public GameObject follower_model;
    public Transform returnMark;
    public UnityEngine.UI.Image countdown_player;
    public LayerMask enemy_layer;
    public AudioClip return_clip;
    public AudioClip explosion_clip, funny_explosion_clip;

    private UnityEngine.UI.Image orb_cooldown;

    private int current_checkpoint = 0;
    void Start()
    {
        playerPositions.Add(transform.position);
        follower_model.transform.position = transform.position;

        orb_cooldown = Instantiate(countdown_player.gameObject, countdown_player.transform.parent).GetComponent<UnityEngine.UI.Image>();
        orb_cooldown.enabled = false;
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

        if (timer < 0.5f)
        {
            timer += Time.deltaTime;
        }
        else
        {
            if (playerPositions.Count > 100)
            {
                playerPositions.Clear();
                playerPositions.Add(transform.position);
            }
            if (playerPositions.Count > 0 && transform.position != playerPositions[playerPositions.Count - 1])
                playerPositions.Add(transform.position);
            timer = 0;
        }

        // Follower logic
        if (playerPositions.Count > 1 && current_checkpoint < playerPositions.Count)
        {
            if (Vector3.Distance(follower_model.transform.position, playerPositions[current_checkpoint]) <= 1)
            {
                ChangeFollowerObjective();
            }
            else
            {
                //follower_model.transform.position = Vector3.Lerp(follower_model.transform.position, playerPositions[current_checkpoint], (PlayerMovement.instance.speed * 0.0025f) * Time.deltaTime);
                follower_model.transform.Translate(follower_model.transform.forward * (PlayerMovement.instance.speed * 0.01f) * Time.deltaTime, Space.World);
            }
        }

        if (Vector3.Distance(follower_model.transform.position, transform.position) > 50)
        {
            playerPositions.Clear();
            playerPositions.Add(transform.position);
            follower_model.transform.position = transform.position;
        }

        if (return_to_pos)
        {
            if (Vector3.Distance(transform.position, return_position) > 0.25f)
            {
                transform.position = Vector3.Lerp(transform.position, return_position, Time.deltaTime * 10);
            }
            else
            {
                // Aquí es cuando el jugador vuelve totalmente a la posición inicial ---------------------------
                if (SoundManager.instance.funnySounds)
                {
                    SoundManager.instance.PlaySound(funny_explosion_clip);
                }
                else
                {
                    SoundManager.instance.PlaySound(explosion_clip);
                }
                Collider[] colls = Physics.OverlapSphere(transform.position, explosion_radius, enemy_layer);
                if (colls.Length > 0)
                {
                    foreach (Collider coll in colls)
                    {
                        coll.GetComponent<Rigidbody>().AddExplosionForce(damage, transform.position, explosion_radius);
                        coll.GetComponent<EnemyLife>().Damage(damage);
                    }
                }
                transform.position = return_position;
                PlayerMovement.instance.canMove = true;
                return_to_pos = false;
            }
        }
    }

    public void ChangeFollowerObjective()
    {
        if (current_checkpoint < playerPositions.Count - 2)
            current_checkpoint++;


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

        yield return new WaitForSeconds(period_time);

        SoundManager.instance.PlaySound(return_clip); // haz que suene el sonido para volver

        // Evita que el jugador se mueva
        PlayerMovement.instance.canMove = false;
        PlayerMovement.instance.rb.velocity = Vector3.zero;

        return_to_pos = true;

        countdown_player.enabled = false;
        orb_cooldown.enabled = false;

        // Desactiva los followers
        returnMark.transform.gameObject.SetActive(false);

        mark_placed = false;
    }
}
