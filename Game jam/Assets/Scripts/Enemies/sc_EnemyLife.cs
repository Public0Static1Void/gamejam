using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(EnemyFollow))]
public class EnemyLife : MonoBehaviour
{
    public bool invulnerable = false;

    public int hp;
    public int max_hp;

    private Rigidbody rb;

    private EnemyFollow enemyFollow;
    public List<AudioClip> clip_damaged;
    public AudioClip clip_growl, blood_explosion;

    public Texture2D stamp;

    public int probability = 100;

    public List<Texture2D> stamps;

    public GameObject cube;

    [Header("Particles")]
    public ParticleSystem particle_explosion;
    public ParticleSystem particle_hit;

    float random_pitch = 0;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        enemyFollow = GetComponent<EnemyFollow>();

        hp = max_hp;

        random_pitch = Random.Range(0.5f, 1.5f);
        enemyFollow.audioSource.pitch = random_pitch;
        enemyFollow.audioSource.clip = clip_growl;
        enemyFollow.audioSource.loop = true;
        enemyFollow.audioSource.Play();
    }
    public void Damage(int amount)
    {
        Instantiate(particle_hit, transform.position, Quaternion.identity);
        // El enemigo hace un sonido de dañado
        if (enemyFollow.audioSource != null)
        {
            enemyFollow.audioSource.clip = clip_damaged[Random.Range(0, clip_damaged.Count - 1)];
            enemyFollow.audioSource.loop = false;
            enemyFollow.audioSource.Play();
        }

        if (invulnerable) return;

        hp -= amount;

        GameManager.gm.damage_done += amount;

        
        if (hp <= 0)
        {
            GameManager.gm.enemies_killed++;


            // Añade el multiplicador
            ScoreManager.instance.AddMultiplier(0.1f);
            // Suma la score
            ScoreManager.instance.ChangeScore(amount, transform.position, true);
            // Añade xp al morirse
            if (TutorialManager.instance == null)
                AbilitiesSystem.instance.AddXP(max_hp * 0.05f);

            Vector3 dir = transform.position - PlayerMovement.instance.transform.position;
            ScoreManager.instance.InstantiateText($"XP +{max_hp * 0.05f}", transform.position, dir.normalized, 40, 2, Color.cyan);

            // Suma la velocidad si se puede
            sc_Abilities.instance.KillNSpeed(100);
            // Suma estamina si puede
            sc_Abilities.instance.Recharge();

            StartCoroutine(DestroyCooldown());
        }
        StartCoroutine(ActivateKinematic());
    }


    void StampOnGround()
    {
        Vector3 forward = transform.forward;
        Vector3[] dirs =
        {
            Vector3.forward,
            Vector3.back,
            Vector3.right,
            Vector3.left,
            Vector3.up,
            Vector3.down
        };
        GameObject target = null;
        RaycastHit hit = new RaycastHit();

        for (int i = 0; i < dirs.Length; i++)
        {
            if (Physics.Raycast(transform.position, dirs[i], out hit, 5))
            {
                target = hit.transform.gameObject;
                //Destroy(Instantiate(cube, hit.point, Quaternion.identity), 15);

                if (target == null) return;

                Texture2D rand_stamp = stamps[Random.Range(0, stamps.Count)];

                rand_stamp = ConvertToEditable(rand_stamp);

                MeshCollider meshCollider = hit.collider as MeshCollider;
                if (meshCollider == null) return;

                Mesh mesh = meshCollider.sharedMesh;
                Material mat = target.GetComponent<MeshRenderer>().material;

                if (!mat.shader.name.Contains("sh_Stamp")) continue;

                Texture2D mainTex = ConvertToEditable((Texture2D)mat.GetTexture("_DecalTexture"));
                mat.SetTexture("_DecalTexture", mainTex);

                // Triangle info
                int triIndex = hit.triangleIndex;
                int[] triangles = mesh.triangles;
                Vector3[] vertices = mesh.vertices;
                Vector2[] uvs = mesh.uv;

                int i0 = triangles[triIndex * 3 + 0];
                int i1 = triangles[triIndex * 3 + 1];
                int i2 = triangles[triIndex * 3 + 2];

                Transform t = target.transform;
                Vector3 p0 = t.TransformPoint(vertices[i0]);
                Vector3 p1 = t.TransformPoint(vertices[i1]);
                Vector3 p2 = t.TransformPoint(vertices[i2]);

                Vector2 uv0 = uvs[i0];
                Vector2 uv1 = uvs[i1];
                Vector2 uv2 = uvs[i2];

                // Estimate UV-to-world scale in both U and V directions
                float avgWorldDist = (Vector3.Distance(p0, p1) + Vector3.Distance(p1, p2) + Vector3.Distance(p2, p0)) / 3f;
                float avgUvDist = (Vector2.Distance(uv0, uv1) + Vector2.Distance(uv1, uv2) + Vector2.Distance(uv2, uv0)) / 3f;

                float uvPerWorldUnit = avgUvDist / avgWorldDist;
                /*
                float worldEdge = (Vector3.Distance(p0, p1) + Vector3.Distance(p1, p2) + Vector3.Distance(p2, p0)) / 3f;
                float uvEdge = (Vector2.Distance(uv0, uv1) + Vector2.Distance(uv1, uv2) + Vector2.Distance(uv2, uv0)) / 3f;

                float uvPerWorldUnit = uvEdge / worldEdge;
                */

                float worldStampSize = Random.Range(2, 4); /// tamaño del stamp
                worldStampSize *= 2 - (1 - (hit.distance / 5));
                float uvSize = worldStampSize * uvPerWorldUnit;

                // Convierte la UV size a pixeles
                int stampPixelSize = Mathf.RoundToInt(mainTex.width * uvSize * 4);

                int multiplier = 4;
                if (dirs[i] == Vector3.down)
                {
                    multiplier = 1;
                    stampPixelSize /= 2;
                }

                GameManager.gm.StampTexture(mainTex, rand_stamp, hit.textureCoord, stampPixelSize / multiplier, stampPixelSize, Random.Range(0, 360));
            }
        }
    }

    Texture2D ConvertToEditable(Texture2D source)
    {
        // Create a new Texture2D in a compatible format
        Texture2D copy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);

        // Copy pixels from source texture
        RenderTexture tmpRT = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        Graphics.Blit(source, tmpRT);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = tmpRT;

        copy.ReadPixels(new Rect(0, 0, tmpRT.width, tmpRT.height), 0, 0);
        copy.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(tmpRT);

        return copy;
    }


    private IEnumerator DestroyCooldown()
    {
        SkinnedMeshRenderer mesh_r = GetComponentInChildren<SkinnedMeshRenderer>();

        Color[] col = new Color[8];
        for (int i = 0; i < col.Length; i++)
        {
            if (i % 2 != 0)
            {
                col[i] = Color.yellow;
            }
            else
            {
                col[i] = Color.red;
            }
        }

        int curr_color = 0;
        float timer = 0;
        float global_timer = 1;
        while (curr_color < col.Length)
        {
            mesh_r.material.color = Color.Lerp(mesh_r.material.color, col[curr_color], Time.deltaTime * 3);

            transform.Rotate(Random.Range(0, 60) * global_timer * Time.deltaTime, Random.Range(0, 50) * global_timer * Time.deltaTime, Random.Range(0, 50) * global_timer * Time.deltaTime);

            global_timer += Time.deltaTime * 5;
            timer += Time.deltaTime;
            if (timer >= 0.1f)
            {
                curr_color++;
                timer = 0;
            }
            rb.velocity *= 0.75f;
            yield return null;
        }

        // Instancia las partículas de muerte
        Instantiate(particle_explosion, transform.position, particle_explosion.transform.rotation);
        // Genera un sonido de explosión
        SoundManager.instance.InstantiateSound(blood_explosion, transform.position, 0.2f);

        StampOnGround();

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (transform.parent != null)
            Destroy(transform.parent.gameObject);

        if (Rounds.instance != null)
        {
            Rounds.instance.enemies.Remove(gameObject);
            Rounds.instance.enemies_follow.Remove(enemyFollow);
        }
    }

    private IEnumerator ActivateKinematic()
    {
        yield return new WaitForSeconds(1.5f);
        enemyFollow.audioSource.clip = clip_growl;
        enemyFollow.audioSource.loop = true;
        enemyFollow.audioSource.Play();
        rb.isKinematic = true;
        rb.freezeRotation = false;
        rb.useGravity = true;
    }
}