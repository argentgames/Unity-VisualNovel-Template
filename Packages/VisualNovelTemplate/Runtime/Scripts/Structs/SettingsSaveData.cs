using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.argentgames.visualnoveltemplate
{
    public struct SettingsSaveData
{
    public float musicVolume, sfxVolume, textSpeed, autoSpeed;
    public int fontSize;
    public bool skipAllText, enableScreenShake, useOpenDSFont ;

    public SettingsSaveData(float music, float sfx, float text, float auto,
     bool skipAllText, bool enableScreenShake, bool useOpenDSFont, int fontSize)
    {
        this.musicVolume = music;
        this.sfxVolume = sfx;
        this.textSpeed = text;
        this.autoSpeed = auto;
        this.skipAllText = skipAllText;
        this.enableScreenShake = enableScreenShake;
        this.useOpenDSFont = useOpenDSFont;
        this.fontSize = fontSize;
    }
}
}
