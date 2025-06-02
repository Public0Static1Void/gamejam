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
            transform.forward,
            transform.right,
            -transform.right,
            -transform.forward,
            transform.up,
            -transform.up
        };
        GameObject target = null;
        RaycastHit hit = new RaycastHit();

        for (int i = 0; i < dirs.Length; i++)
        {
            if (Physics.Raycast(transform.position, dirs[i], out hit))
            {
                target = hit.transform.gameObject;
                Debug.Log($"Name: {hit.transform.name}");
            }
        }
        
        if (target == null) return;

        stamp = ConvertToEditable(stamp);
        Material mat = target.GetComponent<MeshRenderer>().material;

        if (mat == null) return;

        Vector2 tiling = mat.mainTextureScale;
        Vector2 offset = mat.mainTextureOffset;

        Texture2D mainTex = ConvertToEditable((Texture2D)mat.GetTexture("_DecalTexture"));
        mat.SetTexture("_DecalTexture", mainTex);

        Bounds bounds = target.GetComponent<MeshRenderer>().bounds;

        Vector3 localPlayerPos = hit.point;

        Vector2 uv_pos = new Vector2(
            1 - (localPlayerPos.x - bounds.min.x) / bounds.size.x * tiling.x + offset.x,
            1 - (localPlayerPos.z - bounds.min.z) / bounds.size.z * tiling.y + offset.y
        );

        uv_pos.x /= tiling.x;
        uv_pos.y /= tiling.y;

        uv_pos.x += offset.x;
        uv_pos.y += offset.y;

        //uv_pos = hit.textureCoord;
        Debug.Log($"UV Pos = {uv_pos}");

        StampTexture(mainTex, stamp, hit.textureCoord, 10);
    }
    public void StampTexture(Texture2D main_texture, Texture2D stamp_texture, Vector2 uv, int size)
    {
        int x = (int)(uv.x * main_texture.width) - size / 2;
        int y = (int)(uv.y * main_texture.height) - size / 2;

        Debug.Log("Size: " + size);
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                int stampX = (int)((float)i / size * stamp_texture.width);
                int stampY = (int)((float)j / size * stamp_texture.height);

                if (x + i < 0 || x + i >= main_texture.width || y + j < 0 || y + j >= main_texture.height)
                    continue;

                Color stampColor = stamp_texture.GetPixel(stampX, stampY);
                Color baseColor = main_texture.GetPixel(x + i, y + j);
                Color blended = Color.Lerp(baseColor, stampColor, stampColor.a); // Alpha blending
                main_texture.SetPixel(x + i, y + j, blended);
            }
        }

        main_texture.Apply();
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

            transform.Rotate(Random.Range(0, 50) * global_timer * Time.deltaTime, Random.Range(0, 50) * global_timer * Time.deltaTime, Random.Range(0, 50) * global_timer * Time.deltaTime);

            global_timer += Time.deltaTime * 5;
            timer += Time.deltaTime;
            if (timer >= 0.1f)
            {
                curr_color++;
                timer = 0;
            }
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
    }
}