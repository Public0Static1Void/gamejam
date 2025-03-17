using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class sc_Mine : MonoBehaviour
{
    private int damage;
    public float detection_range;
    public float explosion_range;
    public LayerMask enemy_mask;

    public ParticleSystem explosion_particle;

    private AudioSource audioSource;
    public AudioClip clip_pip, clip_explosion;

    private float timer = 0;


    private void Start()
    {
        // Configura el audio de la mina
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = clip_pip;

        damage = ReturnScript.instance.damage * 2;

        // La mina se pone al nivel del suelo
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit))
        {
            transform.position = hit.point;
        }
    }

    private void Update()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, detection_range, enemy_mask);
        if (colls.Length > 0)
        {
            // Si el jugador está dentro del rango se sacudirá la cámara
            if (Vector3.Distance(transform.position, ReturnScript.instance.transform.position) < explosion_range * 1.5f)
            {
                CameraRotation.instance.ShakeCamera(0.5f, Mathf.Clamp(damage * 0.05f, 0.1f, 1.5f));
                GameManager.gm.ShakeController(0.5f, 0.01f, Mathf.Clamp(damage * 0.05f, 0.5f, 1.5f));
            }
            // Instancia las partículas
            ParticleSystem.ShapeModule shape = explosion_particle.shape;
            shape.radius = explosion_range;
            Instantiate(explosion_particle, transform.position, explosion_particle.transform.rotation);

            SoundManager.instance.InstantiateSound(clip_explosion, transform.position);

            // Daña a los enemigos que estén en la zona y los manda por los aires
            ReturnScript.instance.DamageToEnemies(transform.position, damage, explosion_range, Vector3.up * (damage / 2));

            Destroy(gameObject);
        }

        timer += Time.deltaTime;
        if (timer >= 1)
        {
            audioSource.Play();
            timer = 0;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detection_range);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosion_range);
    }
}
