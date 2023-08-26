using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;
using UnityEngine.UI;

[ExecuteAlways]
public class ShaderTransitionController : MonoBehaviour {
    public static ShaderTransitionController i;
    void Awake() { i = this; }

    [System.Serializable]
    public class ShaderSettings 
    {
        public Volume processingVolume;
        public Material skyboxMat;
        public Color fogColor, skyBoxTint;
        [Range(0, 5)]public float skyBoxExposure;
        public Light sun;

        public void Display(float progress, bool paused)
        {
            if (paused) processingVolume.weight = 0;
            else processingVolume.weight = progress;
        }
    }

    [SerializeField, Range(0, 1)] float progress;
    [SerializeField, Range(0, 1)] float progressLerpTarget = 0;
    [SerializeField] bool _next, _prev; 
    bool next, prev;
    [SerializeField] float transitionSmoothness = 0.025f;

    [SerializeField] public Light actualSun;

    [SerializeField] List<ShaderSettings> shaders = new List<ShaderSettings>();

    [SerializeField] int shader1, shader2;
    bool transitioning;
    Vector3 transitionTarget;
    float maxDist;
    bool paused;

    [HideInInspector] public int time;

    [Header("final fight settings")]
    [SerializeField] float brighterExposure = -1.5f;
    [SerializeField] float darkerExposure = -7f;

    [Header("questText")]
    [SerializeField] Color morningColor;
    [SerializeField] Color dayColor, eveningColor, nightColor;
    [SerializeField] TextMeshProUGUI questText;
    [SerializeField] Image questUnderline;

    public void DarkenNight()
    {
        StartCoroutine(LerpExposure(darkerExposure));
    }

    IEnumerator LerpExposure(float targetExposure, float time = 1)
    {
        ColorAdjustments color;
        shaders[shader2].processingVolume.profile.TryGet(out color);

        float timePassed = 0;
        float originalExposure = color.postExposure.value;
        while (timePassed < time) {
            color.postExposure.value = Mathf.Lerp(originalExposure, targetExposure, timePassed / time);
            timePassed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    public void BrightenNight()
    {
        
        StartCoroutine(LerpExposure(brighterExposure));
    }

    public void PausePP()
    {
        paused = true;
    }

    public void ResumePP()
    {
        paused = false;
    }

    private void Start()
    {
        if (Application.isPlaying) {
            progress = 0;
            progressLerpTarget = 0;
        }
    }

    public void EndTransition(int shaderID)
    {
        if (shaderID != shader2) return;

        progressLerpTarget = 1;
        transitioning = false;
    }

    public void StartTransition(float _maxDist, Vector3 _transitionTarget, int stage)
    {
        float _progress = 0;
        if (shader1 == stage) _progress = progress;
        if (shader1 > stage) return;
        
        shader1 = stage;
        shader2 = stage + 1;
        progress = _progress;
        progressLerpTarget = _progress;

        maxDist = _maxDist;
        transitionTarget = _transitionTarget;
        transitioning = true;
    }

    public void SwitchToShader(int num)
    {
        time = num + 1;
        num -= 1;
        shader1 = num;
        shader2 = num + 1;
        progress = 0;
        progressLerpTarget = 0;

        if (num == 3) {
            time = 4;
            shader1 = 2;
            shader2 = 3;
            progress = 1;
            progressLerpTarget = 1;
        }
        transitioning = false;
    }

    private void Update()
    {
        if (!Application.isPlaying) {
            next = _next;
            prev = _prev;
            _next = _prev = false;
        }

        UpdateQuestTextColor();

        if (transitioning) {
            float dist = Vector3.Distance(Player.i.transform.position, transitionTarget);
            float newValue = 1 - Mathf.Clamp01(dist / maxDist);
            if (newValue > progressLerpTarget) progressLerpTarget = newValue;
            if (progress >= 0.99f) {
                progressLerpTarget = 1;
                transitioning = false;
            }
        }
        progress = Mathf.Lerp(progress, progressLerpTarget, transitionSmoothness);


        for (int i = 0; i < shaders.Count; i++) {
            if (i != shader1 && i != shader2) shaders[i].Display(0, paused);
        }

        if (next && shader2 < shaders.Count - 1) {
            next = false;
            shader1 += 1;
            shader2 += 1;
            progress = 0;
        }
        else next = false;
        if (prev && shader2 > 1) {
            prev = false;
            shader1 -= 1;
            shader2 -= 1;
            progress = 1;
        }
        else prev = false;

        if (shaders.Count < 2) return;

        var A = shaders[shader1];
        var B = shaders[shader2];
        A.Display(1 - progress, paused);
        B.Display(progress, paused);
        RenderSettings.fogColor = Color.Lerp(A.fogColor, B.fogColor, progress);
        actualSun.transform.rotation = Quaternion.Lerp(A.sun.transform.rotation, B.sun.transform.rotation, progress);

        LerpSkyBox(A, B, progress);
        LerpSunLight(A.sun, B.sun, progress);
    }

    void UpdateQuestTextColor()
    {
        if (!questText || !questUnderline) return;

        var color = morningColor;
        int current = progress > 0.5f ? shader2 : shader1;
        if (current == 1) color = dayColor;
        if (current == 2) color = eveningColor;
        if (current == 3) color = nightColor;

        questText.color = Color.Lerp(questText.color, color, 0.05f);
        questUnderline.color = Color.Lerp(questUnderline.color, color, 0.05f);
    }

    void LerpSkyBox(ShaderSettings A, ShaderSettings B, float progress)
    {
        RenderSettings.skybox.SetColor("_Tint", Color.Lerp(A.skyBoxTint, B.skyBoxTint, progress));
        RenderSettings.skybox.SetFloat("_Exposure", Mathf.Lerp(A.skyBoxExposure, B.skyBoxExposure, progress));
    }

    void LerpSunLight(Light A, Light B, float progress)
    {
        actualSun.intensity = Mathf.Lerp(A.intensity, B.intensity, progress);
        actualSun.color = Color.Lerp(A.color, B.color, progress);
        actualSun.colorTemperature = Mathf.Lerp(A.colorTemperature, B.colorTemperature, progress);
    }
}
