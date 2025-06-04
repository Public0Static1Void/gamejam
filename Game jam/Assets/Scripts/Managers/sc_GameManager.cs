using System.Collections;
using System.Collections.Generic;
using System.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.HighDefinition.Attributes;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

[RequireComponent(typeof(SaveManager))]
public class GameManager : MonoBehaviour
{
    public static GameManager gm { get; private set; }



    private float alpha = 0;
    private bool show_announce = false;
    private float txt_show_speed;
    private bool pause = false;

    public TMP_Text message_text;

    public GameObject pause_menu;

    private CursorLockMode previous_lockmode;

    public enum TextPositions { CENTER, CENTER_LOWER, LAST_NO_USE }

    [Header("References")]
    public Transform main_canvas;
    public List<TMP_Text> screen_texts;

    private PlayerLife playerLife;

    public UnityEngine.UI.Button btn_resume, btn_continue;

    public GameObject stats_resume_holder;

    public TMP_Text txt_enemies_killed;
    public TMP_Text txt_damage_done;
    public TMP_Text txt_damage_recieved;
    public TMP_Text txt_damage_healed;
    public TMP_Text txt_total_score_points;
    public TMP_Text txt_converted_score_points;

    public UnityEngine.UI.Slider fps_slider;

    public GameObject ob_shpere;

    public Sprite cirle_sprite;

    public Texture2D plane, stamp;
    public GameObject plane_ob;

    public Material m_Stamp;

    [Header("Pause menu")]
    public Animator anim_pause;
    public UnityEngine.UI.Image im_hp;
    public UnityEngine.UI.Image im_xp;
    public UnityEngine.UI.Image im_stamina;
    public TMP_Text txt_hp_text;
    public TMP_Text txt_xp_text;
    public TMP_Text txt_stamina_text;
    public TMP_Text txt_damage_text;
    public TMP_Text txt_speed_text;

    public Transform pause_abilities_holder;

    [Header("Stats")]
    public int enemies_killed = 0;
    public int damage_done = 0;
    public int damage_recieved = 0;
    public int damage_healed = 0;

    public float stamp_size;

    public SaveManager saveManager;

    public PlayerInput playerInput;

    public string controller_name;

    void Awake()
    {
        if (gm == null)
            gm = this;
        else
            Destroy(this.gameObject);

        Application.targetFrameRate = 60;
        ShakeController(0, 0, 0);

        saveManager = GetComponent<SaveManager>();
    }

    private void Start()
    {
        if (anim_pause != null)
            anim_pause.updateMode = AnimatorUpdateMode.UnscaledTime;
        if (PlayerMovement.instance != null)
            playerLife = PlayerMovement.instance.gameObject.GetComponent<PlayerLife>();
    }

    public void ChangeControllerScheme(string scheme)
    {
        playerInput.SwitchCurrentControlScheme(scheme);
    }
    public void ChangeCurrentInputMap(string map)
    {
        playerInput.SwitchCurrentActionMap(map);
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

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            return;
            if (Physics.Raycast(PlayerMovement.instance.transform.position, Camera.main.transform.forward, out RaycastHit hit))
            {
                plane_ob = hit.transform.gameObject;
                Debug.Log($"Name: {hit.transform.name}");
            }

            stamp = ConvertToEditable(stamp);

            MeshCollider meshCollider = hit.collider as MeshCollider;
            Mesh mesh = meshCollider.sharedMesh;
            Material mat = plane_ob.GetComponent<MeshRenderer>().material;

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

            Transform t = plane_ob.transform;
            Vector3 p0 = t.TransformPoint(vertices[i0]);
            Vector3 p1 = t.TransformPoint(vertices[i1]);
            Vector3 p2 = t.TransformPoint(vertices[i2]);

            Vector2 uv0 = uvs[i0];
            Vector2 uv1 = uvs[i1];
            Vector2 uv2 = uvs[i2];

            float worldEdge = (Vector3.Distance(p0, p1) + Vector3.Distance(p1, p2) + Vector3.Distance(p2, p0)) / 3f;
            float uvEdge = (Vector2.Distance(uv0, uv1) + Vector2.Distance(uv1, uv2) + Vector2.Distance(uv2, uv0)) / 3f;

            float uvPerWorldUnit = uvEdge / worldEdge;

            float worldStampSize = 2.5f;
            float uvSize = worldStampSize * uvPerWorldUnit;

            int stampPixelSize = Mathf.RoundToInt(mainTex.width * uvSize);


            StampTexture(mainTex, stamp, hit.textureCoord, stampPixelSize / (int)stamp_size, stampPixelSize);


            //PauseGame();
        }

        // Fade del texto
        if (show_announce)
        {
            alpha += Time.deltaTime * txt_show_speed;
            if (alpha < 1)
            {
                Color col = message_text.color;
                message_text.color = new Color(col.r, col.g, col.b, alpha);
            }
            else if (alpha / 3 > 2)
            {
                show_announce = false;
            }
        }
        else if (alpha > 0)
        {
            alpha -= Time.deltaTime * txt_show_speed;
            Color col = message_text.color;
            message_text.color = new Color(col.r, col.g, col.b, alpha);
        }
    }


    public void SpawnUICircle(RectTransform rectTransform, float radius, Color color, bool grow, float grow_speed = 1)
    {
        if (grow)
        {
            StartCoroutine(SpawnUICircleRoutine(rectTransform, radius, color, grow_speed));
        }
    }
    private IEnumerator SpawnUICircleRoutine(RectTransform rt, float radius, Color color, float grow_speed)
    {
        RectTransform rectTransform = new GameObject("Circle").AddComponent<RectTransform>();

        Transform new_parent = rt.parent == null ? main_canvas.transform : rt.parent.transform;
        rectTransform.gameObject.transform.SetParent(new_parent, false);

        // Pone la posici�n como el recttransform pasado
        rectTransform.anchorMin = rt.anchorMin;
        rectTransform.anchorMax = rt.anchorMax;
        rectTransform.pivot = rt.pivot;

        rectTransform.anchoredPosition = rt.anchoredPosition;
        rectTransform.sizeDelta = rt.sizeDelta;


        UnityEngine.UI.Image im = rectTransform.gameObject.AddComponent<UnityEngine.UI.Image>();
        im.sprite = cirle_sprite;
        im.color = color;

        rectTransform.localScale = Vector3.zero;

        Color col = im.color;
        float alpha = 1;

        float current_radius = 0;
        while (current_radius <= radius || alpha >= 0)
        {
            if (current_radius <= radius)
            {
                current_radius += Time.deltaTime * grow_speed;
            }
            else
            {
                current_radius += Time.deltaTime * grow_speed * 0.5f;
            }
            rectTransform.localScale = Vector2.one * current_radius;


            if (alpha >= 0)
            {
                alpha -= Time.deltaTime * grow_speed * 0.25f;
                im.color = new Color(col.r, col.g, col.b, alpha);
            }

            yield return null;
        }

        Destroy(rectTransform.gameObject);
    }

    /// <summary>
    /// Instancia una esfera con el rango y el color pasados, si grow se marca como true crecer� de 0 hasta radius
    /// </summary>
    public void SpawnShpereRadius(Vector3 position, float radius, Color col, bool grow, float grow_speed = 50, Material material = null)
    {
        if (grow)
        {
            StartCoroutine(GrowSphere(position, radius, col, grow_speed, material));
        }
        else
        {
            GameObject ob = Instantiate(ob_shpere, position, Quaternion.identity);
            Renderer renderer = ob.GetComponent<Renderer>();
            renderer.material.SetColor("_BaseColor", col);
            ob.transform.localScale *= radius;

            StartCoroutine(HideShaderObject(renderer));
        }
    }
    private IEnumerator GrowSphere(Vector3 position, float radius, Color col, float grow_speed, Material material)
    {
        // Instancia el objetp
        GameObject ob = Instantiate(ob_shpere, position, Quaternion.identity);
        // Cambia el color del shader
        Renderer renderer = ob.GetComponent<Renderer>();
        if (material != null)
        {
            renderer.material = material;
        }
        renderer.material.SetColor("_BaseColor", col);
        // Cambia su tama�o a 0
        ob.transform.localScale = Vector3.zero;

        // Esconde el objeto
        StartCoroutine(HideShaderObject(renderer));

        // Hace crecer el objeto hasta el radio
        float curr_radius = 0;
        while (curr_radius < radius)
        {
            curr_radius += Time.deltaTime * grow_speed;
            if (ob != null)
                ob.transform.localScale = Vector3.one * curr_radius;

            yield return null;
        }

    }

    private IEnumerator HideShaderObject(Renderer renderer)
    {
        float alpha = renderer.material.GetFloat("_Alpha");

        while (alpha > 0)
        {
            alpha -= Time.deltaTime;
            renderer.material.SetFloat("_Alpha", alpha);

            yield return null;
        }

        renderer.enabled = false;

        yield return new WaitForSeconds(0.5f);

        Destroy(renderer.gameObject);
    }

    public void ShakeUIElement(RectTransform rectTransform, float duration, float force)
    {
        StartCoroutine(ShakeUIElementRoutine(rectTransform, duration, force));
    }
    private IEnumerator ShakeUIElementRoutine(RectTransform rectTransform, float duration, float force)
    {
        Vector3 original_position = rectTransform.anchoredPosition;
        float timer = 0;
        while (timer < duration)
        {
            //if (PlayerMovement.instance == null && !PlayerMovement.instance.canMove) break;

            timer += Time.deltaTime;

            float x = Random.Range(-1, 1) * force;
            float y = Random.Range(-1, 1) * force;

            rectTransform.anchoredPosition = Vector3.Lerp(original_position, original_position + new Vector3(x, y, 0), Time.deltaTime);

            yield return null;
        }

        rectTransform.anchoredPosition = original_position;
    }

    public void ChangeImageSize(UnityEngine.UI.Image image, Vector3 new_size, float speed)
    {
        StartCoroutine(ChangeImageSizeRoutine(image, new_size, speed));
    }
    private IEnumerator ChangeImageSizeRoutine(UnityEngine.UI.Image image, Vector3 new_size, float speed)
    {
        while (Vector3.Distance(image.rectTransform.sizeDelta, new_size) > 0.1f)
        {
            image.rectTransform.sizeDelta = Vector3.Lerp(image.rectTransform.sizeDelta, new_size, speed * Time.deltaTime);
            yield return null;
        }
    }

    public string GetCurrentControllerName()
    {
        string name = "Keyboard";
        if (playerInput != null && playerInput.currentControlScheme == "Gamepad")
        {
            if (Gamepad.current != null)
            {
                name = Gamepad.current.displayName;
            }
        }

        return name;
    }
    public bool IsPlayerOnKeyboard()
    {
        if (GetCurrentControllerName() == "Keyboard")
            return true;
        return false;
    }

    public void ShakeController(float time, float low_frequency, float high_frequency)
    {
        // Solo vibrar� el mando cuando se est� usando
        if (GetCurrentControllerName() == "Keyboard") return;

        StartCoroutine(ControllerShake(time, low_frequency, high_frequency));
    }
    private IEnumerator ControllerShake(float time, float low_frequency, float high_frequency)
    {
        Gamepad pad = Gamepad.current;
        if (pad != null)
        {
            pad.SetMotorSpeeds(low_frequency, high_frequency);
            yield return new WaitForSeconds(time);
            pad.SetMotorSpeeds(0, 0);
        }
    }

    public void StampTexture(Texture2D main_texture, Texture2D stamp_texture, Vector2 uv, int width, int height, float angleDegrees = 0f)
    {
        int xStart = Mathf.RoundToInt(uv.x * main_texture.width) - width / 2;
        int yStart = Mathf.RoundToInt(uv.y * main_texture.height) - height / 2;

        float angleRad = angleDegrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);

        float stampHalfWidth = width / 2f;
        float stampHalfHeight = height / 2f;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // Coordinates relative to stamp center
                float u = i - stampHalfWidth;
                float v = j - stampHalfHeight;

                // Apply rotation
                float ru = u * cos - v * sin;
                float rv = u * sin + v * cos;

                // Map rotated coordinates to stamp texture
                int stampX = Mathf.RoundToInt((ru + stampHalfWidth) / width * stamp_texture.width);
                int stampY = Mathf.RoundToInt((rv + stampHalfHeight) / height * stamp_texture.height);

                int px = xStart + i;
                int py = yStart + j;

                // Skip if outside main texture bounds
                if (px < 0 || px >= main_texture.width || py < 0 || py >= main_texture.height)
                    continue;

                // Skip if outside rotated stamp bounds
                if (stampX < 0 || stampX >= stamp_texture.width || stampY < 0 || stampY >= stamp_texture.height)
                    continue;

                Color stampColor = stamp_texture.GetPixel(stampX, stampY);
                Color baseColor = main_texture.GetPixel(px, py);
                Color blended = Color.Lerp(baseColor, stampColor, stampColor.a);
                main_texture.SetPixel(px, py, blended);
            }
        }

        main_texture.Apply();
    }
    
    
    // Pausa -------------------------------------------------------------------------------------------------------------
    public void ResumeGame()
    {
        PlayerMovement.instance.GetComponent<PlayerInput>().SwitchCurrentActionMap("Player");

        Time.timeScale = previous_lockmode == CursorLockMode.None ? 0 : 1;

        UnityEngine.Cursor.lockState = previous_lockmode;
        UnityEngine.Cursor.visible = previous_lockmode == CursorLockMode.None ? true : false;

        if (anim_pause != null)
            anim_pause.Play("anim_close_pause");

        pause = false;
        StopCoroutine("InterpolateHighPass");
        SoundManager.instance.SetHighPassEffect(10);

    }
    public void PauseGame()
    {
        LoadPauseInfo();
        previous_lockmode = UnityEngine.Cursor.lockState;

        btn_resume.Select();

        pause_menu.SetActive(true);
        if (anim_pause != null)
            anim_pause.Play("anim_open_pause");

        ShakeController(0, 0, 0);
        Time.timeScale = 0;

        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        PlayerMovement.instance.GetComponent<PlayerInput>().SwitchCurrentActionMap("UI");
        StartCoroutine(InterpolateHighPass(1000));
    }

    private void LoadPauseInfo()
    {
        if (im_hp == null) return;

        im_hp.fillAmount = 1 - (1 - ((float)playerLife.hp / (float)playerLife.max_hp));
        im_xp.fillAmount = AbilitiesSystem.instance.current_xp / AbilitiesSystem.instance.max_xp;
        im_stamina.fillAmount = PlayerMovement.instance.current_stamina / PlayerMovement.instance.max_stamina;

        txt_hp_text.text = $"{playerLife.hp.ToString("F2")}|{playerLife.max_hp}";
        txt_xp_text.text = $"{AbilitiesSystem.instance.current_xp.ToString("F2")}|{AbilitiesSystem.instance.max_xp}";
        txt_stamina_text.text = $"{PlayerMovement.instance.current_stamina.ToString("F1")}|{PlayerMovement.instance.max_stamina}";
        txt_damage_text.text = $"Damage\n\n  {ReturnScript.instance.damage}";
        txt_speed_text.text = $"Speed\n\n {PlayerMovement.instance.speed}";

        if (pause_abilities_holder.childCount < AbilitiesSystem.instance.abilities_equipped.Count)
        {
            List<Ability> abs = new List<Ability>(AbilitiesSystem.instance.abilities_equipped);

            for (int i = 0; i < abs.Count; i++)
            {
                if (i < pause_abilities_holder.childCount)
                {
                    pause_abilities_holder.GetChild(i).gameObject.GetComponent<UnityEngine.UI.Image>().sprite = abs[i].icon;
                }
                else
                {
                    GameObject ob = new GameObject(abs[i].name);
                    UnityEngine.UI.Image im = ob.AddComponent<UnityEngine.UI.Image>();
                    im.sprite = abs[i].icon;

                    ob.transform.SetParent(pause_abilities_holder);
                }
            }
        }
    }

    private IEnumerator InterpolateHighPass(float value)
    {
        float target_highpass_freq = value;
        float current_freq = 10;
        while (current_freq < target_highpass_freq)
        {
            if (!pause) /// Si el men� de pausa se cierra se pone la frecuencia a 10 y se sale del bucle
            {
                SoundManager.instance.SetHighPassEffect(10);
                break;
            }
            current_freq += Time.unscaledDeltaTime * 250;
            SoundManager.instance.SetHighPassEffect(current_freq);
            yield return null;
        }
    }

    public void SetFPS()
    {
        Application.targetFrameRate = (int)fps_slider.value;
    }

    public void ShowGameObject(GameObject ob)
    {
        ob.SetActive(true);
    }
    public void HideGameObject(GameObject ob)
    {
        ob.SetActive(false);
    }
    public void ShowOrHideGameobject(GameObject ob)
    {
        ob.SetActive(!ob.gameObject.activeSelf);
    }
    public void SelectUIButton(UnityEngine.UI.Button button)
    {
        button.Select();
    }

    public void EnableDisableButton(UnityEngine.UI.Button button)
    {
        button.enabled = !button.enabled;
    }

    public void InputPause(InputAction.CallbackContext con)
    {
        if (con.performed)
        {
            pause = !pause;
            if (pause)
                PauseGame();
            else
                ResumeGame();
        }
    }

    public void ShowText(string text, int show_speed = 3)
    {
        message_text.text = text;
        txt_show_speed = show_speed;
        show_announce = true;
    }
    /// <summary>
    /// Cambia la opacidad y el contenido del texto que le indiques, seg�n si le das una velocidad positiva o negativa
    /// </summary>
    public void ShowText(TextPositions text_position, string text, float showspeed)
    {
        StartCoroutine(ShowTextCoroutine(screen_texts[(int)text_position], text,  showspeed));
    }
    
    public void ShowText(string text)
    {
        text = IdiomManager.instance.GetKeyText(text);  
        StartCoroutine(ShowTextCoroutine(screen_texts[(int)TextPositions.CENTER_LOWER], text,  10));
    }

    public void ColorPulse(UnityEngine.UI.Image im, Color pulse_color, float pulse_speed)
    {
        StartCoroutine(ColorPulseRoutine(im, pulse_color, pulse_speed));
    }
    private IEnumerator ColorPulseRoutine(UnityEngine.UI.Image im, Color pulse_color, float pulse_speed)
    {
        Color col = im.color;

        float timer = 0;
        while (timer < 1)
        {
            im.color = Color.Lerp(im.color, pulse_color, Time.deltaTime * pulse_speed);
            timer += Time.deltaTime * pulse_speed;
            yield return null;
        }

        timer = 0;
        while (timer < 2)
        {
            im.color = Color.Lerp(im.color, col, Time.deltaTime * pulse_speed * 2);
            timer += Time.deltaTime * pulse_speed;
            yield return null;
        }
    }
    
    private IEnumerator ShowTextCoroutine(TMP_Text text_reference, string text, float show_speed)
    {
        text_reference.text = text;

        float alpha = 0;
        float timer = 0;
        Color col = text_reference.color;

        int multiplier = 1;
        if (show_speed < 0)
        {
            multiplier = -1;
            show_speed *= -1;
        }

        while (timer < 1)
        {
            timer += Time.deltaTime * show_speed;
            
            /// Cambia el color del texto
            alpha += (Time.deltaTime * show_speed) * multiplier;
            text_reference.color = new Color(col.r, col.g, col.b, alpha);

            yield return null;
        }
    }

    public void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public IEnumerator HideImage(float hide_speed, UnityEngine.UI.Image image_to_hide, TMP_Text text_to_hide = null, bool relocate = true)
    {
        Color col = image_to_hide.color;
        while (image_to_hide.color.a > 0)
        {
            image_to_hide.color = new Color(col.r, col.g, col.b, image_to_hide.color.a - Time.fixedDeltaTime * hide_speed);
            if (text_to_hide != null)
            {
                text_to_hide.color = new Color(text_to_hide.color.r, text_to_hide.color.g, text_to_hide.color.b, image_to_hide.color.a);
            }
            yield return null;
        }
        if (relocate)
        {
            image_to_hide.rectTransform.anchoredPosition = Vector2.zero;
            image_to_hide.rectTransform.position = Vector2.zero;
            if (text_to_hide != null)
            {
                text_to_hide.rectTransform.anchoredPosition = Vector2.zero;
                text_to_hide.rectTransform.position = Vector2.zero;
            }
        }
    }

    

    public void SaveGame()
    {
        // Estructura los datos para guardarlos
        PlayerData playerData = new PlayerData();

        playerData.stamina = PlayerMovement.instance.max_stamina;
        playerData.speed = PlayerMovement.instance.speed;
        playerData.damage = ReturnScript.instance.damage;
        playerData.explosion_range = ReturnScript.instance.damage;
        playerData.hp = ReturnScript.instance.GetComponent<PlayerLife>().max_hp;
        playerData.score += ScoreManager.instance.score;

        saveManager.SaveGame(playerData);
    }

    IEnumerator EndGameCoroutine()
    {
        yield return new WaitForSeconds(10);
        LoadScene("Menu");
    }


    public void EndGame()
    {
        // Mostrar UI con resumen de estad�sticas de la partida
        StartCoroutine(EndGameCoroutine());

        SelectUIButton(btn_continue);

        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        ShakeController(0, 0, 0);

        Time.timeScale = 0;

        txt_damage_done.text = "Damage dealt: " + damage_done.ToString();
        txt_damage_healed.text = "Damage healed: " + damage_healed.ToString();
        txt_damage_recieved.text = "Damage recieved: " + damage_recieved.ToString();

        txt_enemies_killed.text = "Enemies killed: " + enemies_killed.ToString();

        txt_total_score_points.text = "Score: " + ScoreManager.instance.score.ToString();
        txt_converted_score_points.text = "Skill points: " + (ScoreManager.instance.score / 100).ToString();

        stats_resume_holder.SetActive(true);

        ScoreManager.instance.score = ScoreManager.instance.score / 100;

        SaveGame();
    }

    #region UtilityFunctions
    public static int GetChildIndex(Transform child)
    {
        Transform parent = child.parent;
        if (parent == null) return -1;

        for (int i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i) == child)
            {
                return i;
            }
        }
        return -1;
    }

    public static void ChangeUIPosition(Vector2 position, RectTransform element1, RectTransform element2 = null)
    {
        element1.anchoredPosition = position;

        Vector2 anch_pos = element1.anchoredPosition;
        Vector2 pos = element1.position;
        if (element2 != null)
        {
            element2.anchoredPosition = anch_pos;
            element2.position = new Vector2(pos.x - element1.rect.width / 3, pos.y);
        }
    }

    // Change Ui size
    public void UIGrowButton(UnityEngine.UI.Image image)
    {
        StopCoroutine(UIChangeButtonSizeCoroutine(image, new Vector2(image.rectTransform.sizeDelta.x * 1.1f, image.rectTransform.sizeDelta.y * 1.1f)));
        StartCoroutine(UIChangeButtonSizeCoroutine(image, new Vector2(image.rectTransform.sizeDelta.x * 1.1f, image.rectTransform.sizeDelta.y * 1.1f)));
    }
    public void UIShrinkButton(UnityEngine.UI.Image image)
    {
        StopCoroutine(UIChangeButtonSizeCoroutine(image, new Vector2(image.rectTransform.sizeDelta.x * 0.9f, image.rectTransform.sizeDelta.y * 0.9f)));
        StartCoroutine(UIChangeButtonSizeCoroutine(image, new Vector2(image.rectTransform.sizeDelta.x * 0.9f, image.rectTransform.sizeDelta.y * 0.9f)));
    }
    private IEnumerator UIChangeButtonSizeCoroutine(UnityEngine.UI.Image image, Vector2 new_size)
    {
        while (Vector2.Distance(image.rectTransform.sizeDelta, new_size) > 0.15f)
        {
            image.rectTransform.sizeDelta = Vector2.Lerp(image.rectTransform.sizeDelta, new_size, Time.unscaledDeltaTime * 3);
            yield return null;
        }
    }

    public static float GetPercentage(float value, float min, float max)
    {
        return ((value - min) / max - min) * 100f;
    }

    public void LerpFloat(float value, float target_value, float speed)
    {
        StartCoroutine(LerpFloatCoroutine(value, target_value, speed));
    }
    private IEnumerator LerpFloatCoroutine(float value, float target_value, float speed)
    {
        while (value < target_value)
        {
            value += speed;
            yield return null;
        }
    }
    #endregion
}