using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UniRx;
using TMPro;

namespace com.argentgames.visualnoveltemplate
{
    public abstract class Settings_SO : SerializedScriptableObject
    {
        public FloatReactiveProperty MasterVolume = new FloatReactiveProperty(.5f);
        public FloatReactiveProperty AmbientVolume = new FloatReactiveProperty(.5f);
        public FloatReactiveProperty MusicVolume = new FloatReactiveProperty(.5f);
        public FloatReactiveProperty SFXVolume = new FloatReactiveProperty(.5f);
        public FloatReactiveProperty TextSpeed = new FloatReactiveProperty(100f);
        [InfoBox("Fraction of minimum delay between lines")]
        public FloatReactiveProperty AutoSpeed = new FloatReactiveProperty(.5f);
        [InfoBox("When skipping through the game, do we skip unseen text?")]
        public bool skipAllText = false;
        public bool enableScreenShake = true;
        public BoolReactiveProperty useOpenDSFont = new BoolReactiveProperty(false);
        public IntReactiveProperty fontSize = new IntReactiveProperty(1);

        [Button]
        public virtual void ResetDefaults()
        {
            MasterVolume.Value = .5f;
            AmbientVolume.Value = .5f;
            MusicVolume.Value = .5f;
            SFXVolume.Value = .5f;
            TextSpeed.Value = 250;
            AutoSpeed.Value = .5f;
            useOpenDSFont.Value = false;
            fontSize.Value = 1;
            skipAllText = false;
            enableScreenShake = true;

        }
        public virtual SettingsSaveData Save()
        {
            Debug.Log("the text speed we're saving is: " + TextSpeed.Value.ToString());
            return new SettingsSaveData(MusicVolume.Value, SFXVolume.Value, TextSpeed.Value, AutoSpeed.Value,
            skipAllText, enableScreenShake, useOpenDSFont.Value, fontSize.Value);
        }
        public virtual void Load(SettingsSaveData data)
        {
            Debug.Log("the text speed we're loading is: " + data.textSpeed.ToString());
            Debug.Log("music volume we load is: " + data.musicVolume.ToString());
            MusicVolume.Value = data.musicVolume;
            SFXVolume.Value = data.sfxVolume;
            TextSpeed.Value = data.textSpeed;
            AutoSpeed.Value = data.autoSpeed;
            useOpenDSFont.Value = data.useOpenDSFont;
            fontSize.Value = data.fontSize;
            skipAllText = data.skipAllText;
            enableScreenShake = data.enableScreenShake;

        }

    }
}
