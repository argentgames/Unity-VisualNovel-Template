using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Sirenix.OdinInspector;
using UniRx;
using Cysharp.Threading.Tasks;

namespace com.argentgames.visualnoveltemplate
{


public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField]
    AudioBank_SO audioBank;
    List<Music_SO> music = new List<Music_SO>();
    List<SFX_SO> sfx = new List<SFX_SO>();

    List<Ambient_SO> ambients = new List<Ambient_SO>();

    Dictionary<string, Music_SO> musicLookup = new Dictionary<string, Music_SO>();
    Dictionary<string, SFX_SO> sfxLookup = new Dictionary<string, SFX_SO>();
    Dictionary<string, Ambient_SO> ambientLookup = new Dictionary<string, Ambient_SO>();

    private EventInstance musicInstance;
    private EventInstance ambientInstance1, ambientInstance2, ambientInstance3;
    private EventInstance sfxInstance;
    public string currentMusic = "", currentAmbient1 = "", currentAmbient2 = "", currentAmbient3 = "";

    FMOD.Studio.Bus MusicBus;
    FMOD.Studio.Bus SFXBus;
    FMOD.Studio.Bus MasterBus;

    private async UniTaskVoid Awake()
    {
        Instance = this;


        audioBank.PopulateMaps();
        sfxLookup = audioBank.sfxMap;
        ambientLookup = audioBank.ambientMap;
        musicLookup = audioBank.musicMap;

        MusicBus = FMODUnity.RuntimeManager.GetBus("bus:/Master/Music");
        SFXBus = FMODUnity.RuntimeManager.GetBus("bus:/Master/Sound");
        MasterBus = FMODUnity.RuntimeManager.GetBus("bus:/Master");

        await UniTask.WaitUntil(() => GameManager.Instance != null);
        SetRXSubscriptions();

    }

    public void GetFMODBuses()
    {
        MusicBus = FMODUnity.RuntimeManager.GetBus("bus:/Master/Music");
        SFXBus = FMODUnity.RuntimeManager.GetBus("bus:/Master/Sound");
        MasterBus = FMODUnity.RuntimeManager.GetBus("bus:/Master");
    }


    private void SetRXSubscriptions()
    {
        GameManager.Instance.Settings.MusicVolume.Subscribe(val =>
        {
            MusicBus.setVolume(val);
        }).AddTo(this);
        GameManager.Instance.Settings.SFXVolume.Subscribe(val =>
        {
            SFXBus.setVolume(val);
        }).AddTo(this);

    }

    [Button]
    public void PlayMusic(string name, float fadein = 0f)
    {
        var obj = GetEventPath(SoundType.Music, name);
        var eventPath = obj.Event;
        // need fadein ?
        if (currentMusic != "" && IsPlaying(musicInstance))
        {
            StopMusic(0);
        }
        musicInstance = RuntimeManager.CreateInstance(eventPath);
        StartCoroutine(FadeVolume(0, 1f, fadein, musicInstance));
        musicInstance.start();
        currentMusic = name;
    }
    [Button]
    public void StopMusic(float fadeout = 0f)
    {
        Debug.Log("now stopping music");
        if (fadeout == 0)
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            musicInstance.release();
        }
        else
        {
            // TODO: ?? how to add in fadeout timing
            StartCoroutine(FadeVolume(1f, 0, fadeout, musicInstance));
            //musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
        // TODO: is this async? do i need a unitask??

        currentMusic = "";

    }

    bool IsPlaying(FMOD.Studio.EventInstance instance)
    {
        Debug.Log("trying to play new track while old one still playing, force stop");
        FMOD.Studio.PLAYBACK_STATE state;
        instance.getPlaybackState(out state);
        return state != FMOD.Studio.PLAYBACK_STATE.STOPPED;
    }
    [Button]
    public void PlayAmbient(string name, int channel = 0, float fadein = 0f)
    {
        var obj = GetEventPath(SoundType.Ambient, name);
        var eventPath = obj.Event;
        switch (channel)
        {
            case 1:
                if (currentAmbient1 != "" && IsPlaying(ambientInstance1))
                {
                    StopAmbient(1, 0);
                }
                // ambient = EventReference.Find(eventPath);
                ambientInstance1 = RuntimeManager.CreateInstance(eventPath);
                StartCoroutine(FadeVolume(0, 1f, fadein, ambientInstance1));
                ambientInstance1.start();
                currentAmbient1 = name;
                break;
            case 2:
                if (currentAmbient2 != "" && IsPlaying(ambientInstance2))
                {
                    StopAmbient(2, 0);
                }
                ambientInstance2 = RuntimeManager.CreateInstance(eventPath);
                StartCoroutine(FadeVolume(0, 1f, fadein, ambientInstance2));
                ambientInstance2.start();
                currentAmbient2 = name;
                break;
            case 3:
                if (currentAmbient3 != "" && IsPlaying(ambientInstance3))
                {
                    StopAmbient(3, 0);
                }
                ambientInstance3 = RuntimeManager.CreateInstance(eventPath);
                StartCoroutine(FadeVolume(0, 1f, fadein, ambientInstance3));
                ambientInstance3.start();
                currentAmbient3 = name;
                break;
            default:
                if (currentAmbient1 != "" && IsPlaying(ambientInstance1))
                {
                    StopAmbient(0, 0);
                }
                ambientInstance1 = RuntimeManager.CreateInstance(eventPath);
                StartCoroutine(FadeVolume(0, 1f, fadein, ambientInstance1));
                ambientInstance1.start();
                currentAmbient1 = name;
                break;

        }
    }
    [Button]
    public void StopAmbient(float fadeout = 0f, int channel = 0)
    {
        EventInstance ambientInstance = ambientInstance1;
        switch (channel)
        {
            case 1:
                ambientInstance = ambientInstance1;
                currentAmbient1 = "";
                break;
            case 2:
                ambientInstance = ambientInstance2;
                currentAmbient2 = "";
                break;
            case 3:
                ambientInstance = ambientInstance3;
                currentAmbient3 = "";
                break;
            default:
                ambientInstance = ambientInstance1;
                currentAmbient1 = "";
                break;
        }
        if (fadeout == 0)
        {
            ambientInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            ambientInstance.release();
        }
        else
        {

            StartCoroutine(FadeVolume(1f, 0, fadeout, ambientInstance));
        }
    }
    public void StopAllAmbient(float fadeout=0f)
    {
        Debug.Log("now stopping all a mbient");
        currentAmbient1 = "";
        currentAmbient2 = "";
        currentAmbient3 = "";
        StopAmbient(fadeout,1);
        StopAmbient(fadeout,2);
        StopAmbient(fadeout,3);
    }
    [Button]
    public void PlaySFX(string name)
    {
        var obj = GetEventPath(SoundType.SFX, name);
        var eventPath = obj.Event;
        RuntimeManager.PlayOneShot(eventPath);
    }
    public Sound_SO GetEventPath(SoundType sound, string name)
    {
        switch (sound)
        {
            case SoundType.Music:
                return musicLookup[name];
            case SoundType.SFX:
                return sfxLookup[name];
            case SoundType.Ambient:
                return ambientLookup[name];
        }
        return null;
    }
    public void PlayUIEvent(UIEventType eventType)
    {
        // TODO: dont forget about specific inventory ui events toooooo
        switch (eventType)
        {
            case UIEventType.Hover:
                PlaySFX("uiHoverEvent");
                break;
            case UIEventType.Use:
                PlaySFX("uiUseEvent");
                break;
            case UIEventType.Drag:
                PlaySFX("uiDragEvent");
                break;
            case UIEventType.Pause:
                PlaySFX("uiPauseEvent");
                break;
            case UIEventType.UnPause:
                PlaySFX("uiUnpauseEvent");
                break;
            case UIEventType.Drop:
                PlaySFX("uiDropEvent");
                break;
            case UIEventType.Close:
                PlaySFX("uiCloseEvent");
                break;
        }
    }

    private IEnumerator FadeVolume(float start, float end, float dur, EventInstance type)
    {
        float time = 0;
        float value;
        float test;
        while (time <= dur)
        {
            time = time + Time.deltaTime;
            value = Mathf.Lerp(start, end, time / dur);
            // Debug.Log("SOUND VALUE = " + value);
            type.setParameterByName("Fade", value);
            type.getParameterByName("Fade", out test);
            // Debug.Log("PARAMETER VOLUME = " + test);
            if (time >= (dur - 0.1f) && end == 0)
            {
                type.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                type.release();
                // Debug.Log("STOP AND RELEASE MUSIC");
            }

            yield return null;

        }

    }

}


}