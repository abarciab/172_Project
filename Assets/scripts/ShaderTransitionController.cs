using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

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

        public void Display(float progress)
        {
            processingVolume.weight = progress;
        }
    }

    [SerializeField, Range(0, 1)] float progress;
    [SerializeField, Range(0, 1)] float progressLerpTarget = 0;
    public bool next, prev;
    [SerializeField] float transitionSmoothness = 0.025f;

    [SerializeField] public Light actualSun;

    [SerializeField] List<ShaderSettings> shaders = new List<ShaderSettings>();

    [SerializeField] int shader1, shader2;
    bool transitioning;
    Vector3 transitionTarget;
    float maxDist;

    [SerializeField] List<Fact> ShaderFacts = new List<Fact>();

    private void Start()
    {
        if (Application.isPlaying) {
            progress = 0;
            progressLerpTarget = 0;
        }
    }

    public void LoadShaders()
    {
        if (!Application.isPlaying) return;

        var fman = FactManager.i;

        if (fman.IsPresent(ShaderFacts[3])) SwitchToShader(4);
        else if (fman.IsPresent(ShaderFacts[2])) SwitchToShader(3);
        else if (fman.IsPresent(ShaderFacts[1])) SwitchToShader(2);
        else SwitchToShader(1);
    }

    public void EndTransition(int shaderID)
    {
        if (shaderID != shader2) return;

        progressLerpTarget = 1;
        transitioning = false;
    }

    public void StartTransition(float _maxDist, Vector3 _transitionTarget, int stage)
    {
        shader1 = stage;
        shader2 = stage + 1;
        progress = 0;
        progressLerpTarget = 0;

        maxDist = _maxDist;
        transitionTarget = _transitionTarget;
        transitioning = true;
    }

    void SwitchToShader(int num)
    {
        num -= 1;
        shader1 = num;
        shader2 = num + 1;
        progress = 0;
        progressLerpTarget = 0;

        if (num == 3) {
            shader1 = 2;
            shader2 = 3;
            progress = 1;
            progressLerpTarget = 1;
        }
    }

    private void Update()
    {
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
            if (i != shader1 && i != shader2) shaders[i].Display(0);
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
        A.Display(1 - progress);
        B.Display(progress);
        RenderSettings.fogColor = Color.Lerp(A.fogColor, B.fogColor, progress);
        actualSun.transform.rotation = Quaternion.Lerp(A.sun.transform.rotation, B.sun.transform.rotation, progress);

        //RenderSettings.skybox = progress < 0.5 ? A.skyboxMat : B.skyboxMat;
        LerpSkyBox(A, B, progress);

        LerpSunLight(A.sun, B.sun, progress);
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