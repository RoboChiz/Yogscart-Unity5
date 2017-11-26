using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

public class EffectsManager : MonoBehaviour
{
    private const int saveVersion = 0;
    //0 - Created Effects

    public PostProcessingProfile defaultStack;

    private const float checkAgain = 0.1f;
    private float currentTime;

    private bool reapply = false;
    public void ToggleReapply() { reapply = true; }

    //Settings
    private bool useChromaticAberration = true;
    public bool GetUseChromaticAberration() { return useChromaticAberration; }

    private bool useAmbientOcculusion = true;
    public bool GetUseAmbientOcculusion() { return useAmbientOcculusion; }

    public enum AntiAliasing { Off, FXAALow, FXAAMedium, FXAAHigh, TAA, Max}
    private AntiAliasing useAntiAliasing = AntiAliasing.FXAAMedium;
    public AntiAliasing GetAntiAliasing() { return useAntiAliasing; }

    private bool useBloom = true;
    public bool GetBloom() { return useBloom; }

    void Start()
    {
        LoadLastProfile();
        ApplyLoad();
    }

	// Update is called once per frame
	void Update ()
    {
        //Stagger Updates
        currentTime += Time.deltaTime;
        if(currentTime >= checkAgain)
        {
            currentTime = 0f;

            foreach(Camera camera in FindObjectsOfType<Camera>())
            {
                if (camera.GetComponent<NoEffects>() == null)
                {
                    PostProcessingBehaviour postProcessBehaviour = camera.GetComponent<PostProcessingBehaviour>();
                    if (postProcessBehaviour == null)
                    {
                        //Give the Camera a copy of the default effects
                        postProcessBehaviour = camera.gameObject.AddComponent<PostProcessingBehaviour>();
                        postProcessBehaviour.profile = ScriptableObject.CreateInstance<PostProcessingProfile>();

                        //Update Local Behaviour
                        ApplyLoad(postProcessBehaviour.profile);
                    }
                    else if (reapply)
                    {
                        ApplyLoad(postProcessBehaviour.profile);
                    }
                }
            }

            if(reapply)
            {
                reapply = false;
            }

        }

    }

    public void SaveProfile()
    {
        string saveString = "";

        saveString += saveVersion.ToString() + ";";
        saveString += useChromaticAberration + ";";
        saveString += useAmbientOcculusion + ";";
        saveString += ((int)useAntiAliasing).ToString() + ";";
        saveString += useBloom + ";";

        PlayerPrefs.SetString("YogscartPostEffectsOptions", saveString);
    }

    public void LoadLastProfile()
    {
        try
        {
            string[] options = PlayerPrefs.GetString("YogscartPostEffectsOptions","").Split(';');

            int version = int.Parse(options[0]);

            if (version >= saveVersion)
            {
                useChromaticAberration = bool.Parse(options[1]);
                useAmbientOcculusion = bool.Parse(options[2]);

                int aaVal = int.Parse(options[3]);
                if (aaVal < 0 || aaVal >= (int)AntiAliasing.Max)
                {
                    throw new System.Exception("Impossible AA");
                }
                else
                {
                    useAntiAliasing = (AntiAliasing)aaVal;
                }

                useBloom = bool.Parse(options[4]);
            }
        }
        catch
        {
            //Something broke, Reset save
            ResetEverything();
            SaveProfile();
        } 
    }

    private void ResetEverything()
    {
        useChromaticAberration = true;
        useAmbientOcculusion = true;
        useAntiAliasing = AntiAliasing.FXAAMedium;
    }

    public void UpdateValues(bool _chromaticAberration, bool _ambientOcculusion, AntiAliasing _antiAliasing, bool _bloom)
    {
        useChromaticAberration = _chromaticAberration;
        useAmbientOcculusion = _ambientOcculusion;
        useAntiAliasing = _antiAliasing;
        useBloom = _bloom;

        SaveProfile();
        ToggleReapply();
    }

    private void ApplyLoad() { ApplyLoad(defaultStack); }
    private void ApplyLoad(PostProcessingProfile profile)
    {
        //ChromaticAberration
        profile.chromaticAberration.enabled = useChromaticAberration;
        if (useChromaticAberration)
        {
            ChromaticAberrationModel.Settings settings = profile.chromaticAberration.settings;
            settings.intensity = defaultStack.chromaticAberration.settings.intensity;
            settings.spectralTexture = defaultStack.chromaticAberration.settings.spectralTexture;
            profile.chromaticAberration.settings = settings;
        }

        //Ambient Occulusion
        profile.ambientOcclusion.enabled = useAmbientOcculusion;

        //Antialiasing
        if(useAntiAliasing > AntiAliasing.Off)
        { 
            profile.antialiasing.enabled = true;

            AntialiasingModel.Settings aaSetings = profile.antialiasing.settings;        
            switch(useAntiAliasing)
            {
                case AntiAliasing.FXAALow:
                    aaSetings.method = AntialiasingModel.Method.Fxaa;
                    aaSetings.fxaaSettings.preset = AntialiasingModel.FxaaPreset.Performance;
                    break;
                case AntiAliasing.FXAAMedium:
                    aaSetings.method = AntialiasingModel.Method.Fxaa;
                    aaSetings.fxaaSettings.preset = AntialiasingModel.FxaaPreset.Default;
                    break;
                case AntiAliasing.FXAAHigh:
                    aaSetings.method = AntialiasingModel.Method.Fxaa;
                    aaSetings.fxaaSettings.preset = AntialiasingModel.FxaaPreset.ExtremeQuality;
                    break;
                case AntiAliasing.TAA:
                    aaSetings.method = AntialiasingModel.Method.Taa;
                    break;
            }

            profile.antialiasing.settings = aaSetings;
        }

        profile.bloom.enabled = useBloom;
        if (useBloom)
        {
            BloomModel.Settings settings = profile.bloom.settings;

            settings.bloom.intensity = defaultStack.bloom.settings.bloom.intensity;
            settings.bloom.antiFlicker = defaultStack.bloom.settings.bloom.antiFlicker;
            settings.bloom.radius = defaultStack.bloom.settings.bloom.radius;
            settings.bloom.softKnee = defaultStack.bloom.settings.bloom.softKnee;
            settings.bloom.threshold = defaultStack.bloom.settings.bloom.threshold;
            settings.lensDirt.intensity = defaultStack.bloom.settings.lensDirt.intensity;
            settings.lensDirt.texture = defaultStack.bloom.settings.lensDirt.texture;

            profile.bloom.settings = settings;
        }
    }

    
}
