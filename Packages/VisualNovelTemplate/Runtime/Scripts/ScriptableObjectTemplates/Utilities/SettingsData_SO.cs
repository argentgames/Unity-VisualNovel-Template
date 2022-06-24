using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UniRx;
using TMPro;

public class SettingsData_SO : SerializedScriptableObject
{
    public FloatReactiveProperty MusicVolume = new FloatReactiveProperty(.5f);
    public FloatReactiveProperty SFXVolume = new FloatReactiveProperty(.5f);
    public string Resolution = "1920x1080";
    public FloatReactiveProperty TextSpeed = new FloatReactiveProperty(100f);
    [InfoBox("Fraction of minimum delay between lines")]
    public FloatReactiveProperty AutoSpeed = new FloatReactiveProperty(.5f); 
    public bool skipAllText = false;
    public bool enableScreenShake = true;
    public BoolReactiveProperty useOpenDSFont = new BoolReactiveProperty(false);
    public IntReactiveProperty fontSize = new IntReactiveProperty(1);
    public List<string> SeenText = new List<string>();
    public List<string> chosenChoices = new List<string>();
    // TECHDEBT: saving cg unlocked as plaintext cuz lazy to make yet another save obj <_<
    public bool pcgUnlocked = false, vcgUnlocked = false, lcgUnlocked = false;
    public Dictionary<string, bool> cgDict = new Dictionary<string,bool>{
        {"pcgUnlocked", false},
        {"vcgUnlocked", false},
        {"lcgUnlocked", false}

    };
    public bool watchedOP = false, watchedCredits = false, gdprConsent = false;
    


    [Button]
    public void ResetDefaults()
    {
        MusicVolume.Value = .5f;
        SFXVolume.Value = .5f;
        TextSpeed.Value = 250;
        AutoSpeed.Value = .5f;
        useOpenDSFont.Value = false;
        fontSize.Value = 1;
        SeenText.Clear();
        chosenChoices.Clear();
        skipAllText = false;
        enableScreenShake = true;
        pcgUnlocked = false;
        vcgUnlocked = false;
        lcgUnlocked = false;
        
        cgDict["pcgUnlocked"] = pcgUnlocked;
        cgDict["vcgUnlocked"] = vcgUnlocked;
        cgDict["lcgUnlocked"] = lcgUnlocked;
        watchedOP = false;
        watchedCredits = false;
        gdprConsent = false;
    }
    public SettingsSaveData Save()
    {
        Debug.Log("the text speed we're saving is: " + TextSpeed.Value.ToString());
        return new SettingsSaveData(MusicVolume.Value, SFXVolume.Value, TextSpeed.Value, AutoSpeed.Value,
        SeenText, chosenChoices, skipAllText, enableScreenShake, useOpenDSFont.Value, fontSize.Value,
        pcgUnlocked, vcgUnlocked, lcgUnlocked, watchedOP,watchedCredits,gdprConsent);
    }
    public void Load(SettingsSaveData data)
    {
        Debug.Log("the text speed we're loading is: " + data.textSpeed.ToString());
        MusicVolume.Value = data.musicVolume;
        SFXVolume.Value = data.sfxVolume;
        TextSpeed.Value = data.textSpeed;
        AutoSpeed.Value = data.autoSpeed;
        useOpenDSFont.Value = data.useOpenDSFont;
        fontSize.Value = data.fontSize;
        SeenText = data.seenText;
        chosenChoices = data.chosenChoices;
        skipAllText = data.skipAllText;
        enableScreenShake = data.enableScreenShake;
        pcgUnlocked = data.pcgUnlocked;
        vcgUnlocked = data.vcgUnlocked;
        lcgUnlocked = data.lcgUnlocked;

        cgDict["pcgUnlocked"] = pcgUnlocked;
        cgDict["vcgUnlocked"] = vcgUnlocked;
        cgDict["lcgUnlocked"] = lcgUnlocked;
        watchedOP = data.watchedOP;
        watchedCredits = data.watchedCredits;
        gdprConsent = data.gdprConsent;

    }

}

public class SettingsSaveData
{
    public float musicVolume, sfxVolume, textSpeed, autoSpeed;
    public int fontSize;
    public List<string> seenText;
    public List<string> chosenChoices;
    public bool skipAllText, enableScreenShake, useOpenDSFont, pcgUnlocked, vcgUnlocked, lcgUnlocked;

    public bool watchedOP,watchedCredits, gdprConsent;
    public SettingsSaveData(float music, float sfx, float text, float auto,
     List<string> seenText, List<string> chosenChoices, bool skipAllText,
    bool enableScreenShake, bool useOpenDSFont, int fontSize, bool pcgUnlocked,
    bool vcgUnlocked, bool lcgUnlocked, bool watchedOP, bool watchedCredits, bool gdprConsent)
    {
        this.musicVolume = music;
        this.sfxVolume = sfx;
        this.textSpeed = text;
        this.autoSpeed = auto;
        this.seenText = seenText;
        this.chosenChoices = chosenChoices;
        this.skipAllText = skipAllText;
        this.enableScreenShake = enableScreenShake;
        this.useOpenDSFont = useOpenDSFont;
        this.fontSize = fontSize;
        this.pcgUnlocked = pcgUnlocked;
        this.vcgUnlocked = vcgUnlocked;
        this.lcgUnlocked = lcgUnlocked;
        this.watchedOP = watchedOP;
        this.watchedCredits = watchedCredits;
        this.gdprConsent = gdprConsent;
    }
}