using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    private bool healed = false;
    public bool can_heal = false;
    private float heal_timer = 0;

    [Header("References")]
    public LayerMask enemyMask;
    public UnityEvent ability;
    public Image cooldown_image;
    public Image returning_effect_image;
    public GameObject water_explosion_particle;
    public ParticleSystem explosion_particle;
    public ParticleSystem huge_water_explosion_particle;
    public GameObject ob_AfterImage;

    private Vector3 afterImage_target;

    [Header("Sonidos")]
    public AudioClip return_clip;
    public AudioClip explosion_clip, funny_explosion_clip, tictac_clip, nautilus_explosion;
    public AudioClip cooldown_ready_clip, cooldown_not_ready_clip;
    public List<AudioClip> clips_electric_move;
    public AudioClip electric_spark;

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


    private bool isFadingOut = false, isFadingIn = false;

    private bool afterImage_attack = false;

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
            // La posición de la cámara se pasa al punto donde volverás en el tiempo para dar una vista aérea
            Camera.main.transform.parent = null;
            Camera.main.transform.localPosition = new Vector3(past_positions[0].x, past_positions[0].y + 4, past_positions[0].z);

            /// Calcula la distancia hasta el último punto en un rango de 1 a 0
            cooldown_image.fillAmount = 1 - (1 - (Vector3.Distance(transform.position, past_positions[0]) / Vector3.Distance(past_positions[0], past_positions[past_positions.Count - 1])));
            /// Cambia la transparencia del efecto de volver en el tiempo
            returning_effect_image.color = new Color(returning_effect_image.color.r, returning_effect_image.color.g, returning_effect_image.color.b,  1 - cooldown_image.fillAmount * 1.15f);


            if (Vector3.Distance(transform.position, past_positions[current_point]) > 1f)
            {
                Vector3 dir = (past_positions[current_point] - transform.position).normalized;
                transform.Translate(dir * (return_speed * 0.75f + Vector3.Distance(transform.position, past_positions[current_point])) * Time.deltaTime, Space.World); /// Mueve al jugador en la dirección a su anterior posición

                transform.rotation = Quaternion.Lerp(transform.rotation, q_rotations[current_point], Time.deltaTime * return_speed);

                Camera.main.transform.LookAt(transform);
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

                    SoundManager.instance.PlaySound(null);

                    Camera.main.transform.position = transform.position;
                    Camera.main.transform.SetParent(transform);

                    cooldown_image.fillAmount = 0;

                    #region DamageToEnemy
                    /*
                    if (SoundManager.instance.funnySounds) /// Sonidos de explosión
                    {
                        SoundManager.instance.InstantiateSound(funny_explosion_clip, transform.position);
                    }
                    else
                    {
                        SoundManager.instance.InstantiateSound(explosion_clip, transform.position);
                    }
                    GameManager.gm.ShakeController(1, 0.25f, 1);
                    DamageToEnemies(transform.position, damage, explosion_range, Vector3.zero);
                    */
                    #endregion

                    playerLife.Invulnerable(); /// Hace que el jugador sea invulnerable cuando acaba de llegar
                    PlayerMovement.instance.canMove = true;
                    GetComponent<Rigidbody>().isKinematic = false;
                    GetComponent<Collider>().isTrigger = false;

                    /// Instancia las partículas de explosión
                    ParticleSystem.ShapeModule shape = explosion_particle.shape;
                    shape.radius = explosion_range;
                    Instantiate(explosion_particle, transform.position, explosion_particle.transform.rotation);

                    StartCoroutine(GameManager.gm.HideImage(2, returning_effect_image, null, false));

                    returning = false;
                    cooldown = true;
                    timer = 0;
                }
            }
        }
        else /// Resto de comprobaciones en el update
        {
            if (cooldown) // Cuenta atrás del cooldown
            {
                if (cooldown_timer == 0) ClearReturnLists(); /// Vacía la lista de posiciones
                cooldown_timer += Time.deltaTime;
                cooldown_image.fillAmount += Time.deltaTime / cooldown_time; /// Suma la cantidad de fill a la imágen del cooldown
                cooldown_image.color = Color.Lerp(cooldown_image.color, color_on_cooldown, Time.deltaTime * 1.75f); /// Cambia el color de la imágen al de cooldown

                // El cooldown ha terminado
                if (cooldown_timer > cooldown_time)
                {
                    // Muestra un círculo en el icono de cooldown para mostrar que ya está el cooldown
                    GameManager.gm.SpawnUICircle(cooldown_image.rectTransform, 2, Color.gray, true, 5);

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
                if (!afterImage_attack)
                {
                    ob_AfterImage.transform.position = Vector3.Lerp(ob_AfterImage.transform.position, past_positions[0], Time.deltaTime * 1.25f);
                    ob_AfterImage.transform.rotation = Quaternion.Lerp(ob_AfterImage.transform.rotation, q_rotations[0], Time.deltaTime);

                    // Si está muy cerca del jugador se desvanecerá
                    if (Vector3.Distance(ob_AfterImage.transform.position, transform.position) < 1)
                    {
                        if (!isFadingOut)
                            StartCoroutine(FadeOutHologramObject(ob_AfterImage));
                    }
                    else
                    {
                        if (!isFadingIn)
                            StartCoroutine(FadeInHologramObject(ob_AfterImage));
                    }
                }
            }
        }

        if (healed)
        {
            heal_timer += Time.deltaTime;
            if (heal_timer > 0.1f)
            {
                heal_timer = 0;
                healed = false;
            }
        }
    }

    private IEnumerator FadeOutHologramObject(GameObject ob)
    {
        isFadingOut = true; /// Controla si se está ejecutando la ecorutina

        MeshRenderer mesh_r = ob.GetComponent<MeshRenderer>();

        MeshRenderer[] childs_mesh = new MeshRenderer[ob.transform.childCount];
        for (int i = 0; i < ob.transform.childCount; i++)
        {
            if (ob.transform.GetChild(i).TryGetComponent<MeshRenderer>(out MeshRenderer mr))
            {
                childs_mesh[i] = mr;
            }
        }

        float alpha = mesh_r.materials[0].GetFloat("_Alpha");
        while (alpha > 0)
        {
            alpha -= Time.deltaTime * 0.5f;
            mesh_r.materials[0].SetFloat("_Alpha", alpha);
            for (int i = 0; i < childs_mesh.Length; i++)
            {
                childs_mesh[i].materials[0].SetFloat("_Alpha", alpha);
            }
            yield return null;
        }

        isFadingOut = false;
    }
    private IEnumerator FadeInHologramObject(GameObject ob)
    {
        isFadingIn = true;

        MeshRenderer mesh_r = ob.GetComponent<MeshRenderer>();

        MeshRenderer[] childs_mesh = new MeshRenderer[ob.transform.childCount];
        for (int i = 0; i < ob.transform.childCount; i++)
        {
            childs_mesh[i] = ob.transform.GetChild(i).GetComponent<MeshRenderer>();
        }

        float alpha = mesh_r.materials[0].GetFloat("_Alpha");
        while (alpha < 1)
        {
            alpha += Time.deltaTime * 0.5f;
            mesh_r.materials[0].SetFloat("_Alpha", alpha);
            for (int i = 0; i < childs_mesh.Length; i++)
            {
                childs_mesh[i].materials[0].SetFloat("_Alpha", alpha);
            }
            yield return null;
        }

        isFadingIn = false;
    }

    public void DamageToEnemies(Vector3 origin, int damage_amount, float range, Vector3 dir = new Vector3())
    {
        Collider[] colls = Physics.OverlapSphere(origin, range, enemyMask);
        if (colls.Length > 0)
        {
            float level = AbilitiesSystem.instance.abilities_log[(int)AbilitiesSystem.Abilities.BLOODTHIRSTY].ability_level;
            foreach (Collider coll in colls)
            {
                Vector3 d = (coll.transform.position - transform.position).normalized;
                dir += d;
                coll.GetComponent<EnemyFollow>().AddForceToEnemy(dir * (damage_amount * 1.25f * level));
                coll.GetComponent<EnemyLife>().Damage((int)(damage_amount * level));

                // Cura del jugador
                if (can_heal && !healed)
                {
                    playerLife.Damage((int)(-damage * 0.15f * level));
                    healed = true;
                }
            }
        }
    }

    public void ReturnToLastPosition(InputAction.CallbackContext con)
    {
        if (con.performed && !AbilitiesSystem.instance.gambling_open) // Se ha pulsado espacio y no está abierto el panel de gambling
        {
            if (!cooldown && !returning)
            {
                returning = true;
                GetComponent<Rigidbody>().isKinematic = true; /// Deshabilita el rigidbody y las colisiones para evitar problemas al volver
                GetComponent<Collider>().isTrigger = true;
                PlayerMovement.instance.canMove = false; /// Evita que el jugador pueda moverse mientras vuelve
                current_point = past_positions.Count - 1;

                StartCoroutine(HideObjectsOnView());

                SoundManager.instance.PlaySound(return_clip);

                if (ability != null) /// Ejecuta la habilidad del player
                {
                    ability.Invoke();
                }

                StartCoroutine(AfterImageAttack());
            }
            else /// El jugador ha terminado la habilidad antes
            {
                past_positions[0] = transform.position;
                current_point = 0;
            }
            if (cooldown)
            {
                SoundManager.instance.PlaySound(cooldown_not_ready_clip);
                Debug.Log(cooldown_image.transform.parent.name);

                GameManager.gm.ShakeUIElement(cooldown_image.transform.parent.GetComponent<RectTransform>(), 0.25f, 100);
            }
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


    private IEnumerator AfterImageAttack()
    {
        afterImage_attack = true;

        List<Vector3> positions = new List<Vector3>(past_positions);
        List<Quaternion> rotations = new List<Quaternion>(q_rotations);

        int curr_pos = positions.Count - 1;

        // Cuánta más distancia recorra la imagen más daño hará
        float scaled_damage = (Vector3.Distance(positions[curr_pos], positions[0]) * 0.1f) * (damage * 0.2f);

        ob_AfterImage.transform.position = transform.position;
        ob_AfterImage.transform.rotation = transform.rotation;
        ob_AfterImage.SetActive(true);

        // Espera a que el jugador haya terminado de volver
        while (returning)
            yield return null;

        // Hace un sonido de movimiento eléctrico
        SoundManager.instance.InstantiateSound(clips_electric_move[Random.Range(0, clips_electric_move.Count)], ob_AfterImage.transform.position, 0.25f);


        List<string> names_hit = new List<string>();

        /// Hace vibrar el mando
        GameManager.gm.ShakeController(10, 0, 0.006f);

        while (curr_pos >= -1)
        {
            if (curr_pos >= 0)
            {
                // Movimiento del objeto
                ob_AfterImage.transform.position = Vector3.Lerp(ob_AfterImage.transform.position, positions[curr_pos], Time.deltaTime * (return_speed * 2));
                ob_AfterImage.transform.rotation = Quaternion.Lerp(ob_AfterImage.transform.rotation, rotations[curr_pos], Time.deltaTime * return_speed);


                // Si toca al jugador acaba la habilidad
                if (Vector3.Distance(ob_AfterImage.transform.position, transform.position) <= 1f)
                    curr_pos = -1;
                else if (Vector3.Distance(ob_AfterImage.transform.position, positions[curr_pos]) <= 0.15f)
                {
                    curr_pos--;

                    yield return new WaitForSeconds(0.1f);

                    // Hace un sonido de movimiento eléctrico
                    SoundManager.instance.InstantiateSound(clips_electric_move[Random.Range(0, clips_electric_move.Count)], ob_AfterImage.transform.position, 0.25f);
                }
            }
            else
            {
                ob_AfterImage.transform.position = Vector3.Lerp(ob_AfterImage.transform.position, transform.position, Time.deltaTime * (return_speed * 2));
                if (Vector3.Distance(ob_AfterImage.transform.position, transform.position) <= 1f)
                    break;
            }

            // Comprobación de choque
            Collider[] colls = Physics.OverlapSphere(ob_AfterImage.transform.position, 2, enemyMask);
            if (colls.Length > 0)
            {
                for (int i = 0; i < colls.Length; i++)
                {
                    try
                    {
                        if (colls[i] != null && names_hit.Contains(colls[i].transform.parent.name)) continue;

                        // Envia el enemigo a volar y después le aplica el daño
                        Vector3 dir = (colls[i].transform.position - ob_AfterImage.transform.position).normalized;
                        colls[i].GetComponent<EnemyFollow>().AddForceToEnemy(dir * scaled_damage * 5);
                        colls[i].GetComponent<EnemyLife>().Damage((int)scaled_damage);

                        // hace un sonido de choque eléctrico
                        SoundManager.instance.InstantiateSound(electric_spark, colls[i].transform.position, 0.75f);

                        GameManager.gm.SpawnShpereRadius(colls[i].transform.position, 3, Color.green, true, 200);

                        names_hit.Add(colls[i].transform.parent.name);
                    }
                    catch
                    {
                        Debug.Log("Enemy was destroyed and ReturnScript 435 is trying to access it");
                    }
                }
            }

            yield return null;
        }

        GameManager.gm.ShakeController(0, 0, 0);

        //ob_AfterImage.SetActive(false);

        afterImage_attack = false;
    }

    private IEnumerator HideObjectsOnView()
    {
        List<MeshRenderer> meshes_to_hide = new List<MeshRenderer>();
        List<float> alpha = new List<float>();

        while (returning)
        {
            Vector3 dir = (transform.position - Camera.main.transform.position).normalized;

            if (Physics.Raycast(Camera.main.transform.position, dir, out RaycastHit hit, Vector3.Distance(Camera.main.transform.position, transform.position)))
            {
                MeshRenderer renderer = hit.transform.GetComponent<MeshRenderer>();
                if (renderer != null && !meshes_to_hide.Contains(renderer) && hit.transform.name != "Player")
                {
                    meshes_to_hide.Add(renderer);
                    alpha.Add(1);

                    // Pone el material como transparente
                    foreach (Material mat in renderer.materials)
                    {
                        mat.SetFloat("_Mode", 3);
                        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        mat.SetInt("_ZWrite", 0);
                        mat.DisableKeyword("_ALPHATEST_ON");
                        mat.EnableKeyword("_ALPHABLEND_ON");
                        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        mat.renderQueue = 3000;
                    }
                }

                for (int i = 0; i < meshes_to_hide.Count; i++)
                {
                    if (alpha[i] > 0)
                    {
                        foreach (Material mat in meshes_to_hide[i].materials)
                        {
                            if (!mat.shader.name.ToLower().Contains("standard")) continue;
                            Color col = mat.color;
                            alpha[i] -= Time.deltaTime;
                            mat.color = new Color(col.r, col.g, col.b, alpha[i]);
                        }
                    }
                }
            }

            yield return null;
        }



        for (int i = 0; i < meshes_to_hide.Count; i++)
        {
            foreach (Material mat in meshes_to_hide[i].materials)
            {
                while (alpha[i] < 1)
                {
                    Color col = mat.color;
                    alpha[i] += Time.deltaTime; // Gradually increase transparency
                    alpha[i] = Mathf.Clamp01(alpha[i]); // Prevent alpha from exceeding 1
                    mat.color = new Color(col.r, col.g, col.b, alpha[i]); // Restore transparency
                    yield return null;
                }
                    

                // Restore material to Opaque mode
                mat.SetFloat("_Mode", 0); // Opaque
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                mat.SetInt("_ZWrite", 1);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = -1; // Default render queue for opaque objects
            }
        }
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


        // Cambia el radio de las partículas por el radio de la explosión
        float range = 3 + (damage * 0.2f);
        if (water_explosion_particle.transform.childCount > 0)
        {
            ParticleSystem ground_explosion = explosionParticle.transform.GetChild(0).GetComponent<ParticleSystem>();
            ParticleSystem.ShapeModule shape_module = ground_explosion.shape;
            shape_module.radius = range;
        }

        List<GameObject> pre_explosion_particles = new List<GameObject>();
        for (int i = 0; i < positions.Count; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(positions[i], Vector3.down, out hit))
            {
                positions[i] = hit.point;
            }
            pre_explosion_particles.Add(Instantiate(water_explosion_particle, positions[i], water_explosion_particle.transform.rotation));
        }

        while (explosion_num >= 0)
        {
            explosion_timer += Time.deltaTime;
            if (explosion_timer > 0.75f)
            {
                float level = AbilitiesSystem.instance.abilities[(int)AbilitiesSystem.Abilities.EXPLODE_PATH].ability_level;
                /// Instancia las partículas
                Destroy(pre_explosion_particles[explosion_num]);

                GameManager.gm.SpawnShpereRadius(positions[explosion_num], range * 2 * level, Color.cyan, true, 50);

                /// Aplica el daño a los enemigos
                DamageToEnemies(
                    positions[explosion_num], 
                    (int)(damage * 0.25f * level), 
                    range, 
                    Vector3.up * (2 + (damage * 0.1f))
                );

                

                Destroy(Instantiate(huge_water_explosion_particle, positions[explosion_num], huge_water_explosion_particle.transform.rotation), 2);

                float dist = Vector3.Distance(positions[explosion_num], transform.position);
                if (dist < 15) /// Si el jugador está cerca hará vibrár el mando
                {   
                    if (nautilus_explosion != null) /// Sonido de explosión
                        SoundManager.instance.InstantiateSound(nautilus_explosion, positions[explosion_num]);
                    
                    /// Vibración del mando
                    GameManager.gm.ShakeController(0.005f + (1 - (dist / 10)), (0.01f + (1 - (dist / 15))), 0);
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
        Gizmos.color = Color.white;
        for (int i = 0; i < past_positions.Count; i++)
        {
            Gizmos.DrawWireSphere(past_positions[i], 1);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosion_range);
    }
}