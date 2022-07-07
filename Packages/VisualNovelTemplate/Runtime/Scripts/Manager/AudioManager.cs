using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Sirenix.OdinInspector;
using UniRx;
using Cysharp.Threading.Tasks;
using System;

/// <summary>
/// Ingame audio management. Access your typical sound control methods such as Play/Stop music/ambient/sfx.
/// We have Music, SFX, and (almost) unlimited Ambient audio tracks. The volume of these tracks are controlled
/// by their respective buses, + a master bus. 
/// Sometimes we can't fit an Ambient volume control in our UI (or it seems too unintuitive), so we will expose
/// only Master, Music, and SFX volume controls.
/// </summary>
namespace com.argentgames.visualnoveltemplate
{
    public struct FMODEventInstanceData
    {
        public SoundType soundType;
        private EventInstance? eventInstance;
        public EventInstance EventInstance { get { return (EventInstance)eventInstance; } }
        public string soundName;
        // QUESTION @Dovah: is it possible to have individual eventInstance volumes, separate from the Bus?
        public float trackVolume;

        public FMODEventInstanceData(SoundType soundType, EventInstance? eventInstance,
        string soundName, bool isplaying, float trackvolume)
        {
            this.soundType = soundType;
            this.eventInstance = eventInstance;
            this.soundName = soundName;
            this.trackVolume = trackvolume;
        }

        public bool IsPlaying
        {
            get
            {
                FMOD.Studio.PLAYBACK_STATE state;
                EventInstance.getPlaybackState(out state);
                return state != FMOD.Studio.PLAYBACK_STATE.STOPPED;
            }
        }
    }

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        [SerializeField]
        AudioBank_SO audioBank;
        [SerializeField]
        [InfoBox("How many ambient tracks do we want to be able to play simultaneously? 3 is a pretty safe number, increase at your own risk of confusion!")]
        int numAmbientTracks = 3;

        private Dictionary<string, FMODEventInstanceData> eventInstanceMap = new Dictionary<string, FMODEventInstanceData>();

        private FMODEventInstanceData fmodEvent;
        private EventInstance musicInstance, ambientInstance, sfxInstance;

        FMOD.Studio.Bus MusicBus;
        FMOD.Studio.Bus SFXBus;
        FMOD.Studio.Bus MasterBus;
        // TODO: add ambient bus

        private async UniTaskVoid Awake()
        {
            Instance = this;

            // TECHDEBT: We really need to understand how/when OnEnable is run so that we aren't 
            // constantly calling this.
            // populate our audio bank on awake just in case it doesn't populate correctly.
            audioBank.PopulateMaps();

            GetFMODBuses();


            // create all our eventInstanceMap so we can have unlimited ambient tracks :>
            eventInstanceMap["music"] = new FMODEventInstanceData(SoundType.Music, null, "", false, 1.0f);
            eventInstanceMap["sfx"] = new FMODEventInstanceData(SoundType.SFX, null, "", false, 1.0f);
            for (int i = 0; i < numAmbientTracks; i++)
            {
                eventInstanceMap["ambient" + i.ToString()] = new FMODEventInstanceData(SoundType.Ambient, null, "", false, 1.0f);
            }

            await UniTask.WaitUntil(() => GameManager.Instance != null);

            SetRXSubscriptions();

        }

        /// <summary>
        /// TODO: We might want to return an object that contains soundName, eventInstanceVolume, and other information.
        /// </summary>
        /// <returns></returns>
        public string GetCurrentPlayingMusic()
        {
            if (eventInstanceMap["music"].IsPlaying)
            {
                return eventInstanceMap["music"].soundName;
            }
            else
            {
                return "";
            }
        }
        public List<Tuple<string,int>> GetCurrentPlayingAmbients()
        {
            List<Tuple<string,int>> currAmbs = new List<Tuple<string, int>>();
            for (int i=0; i < numAmbientTracks; i++)
            {
                var instance = eventInstanceMap["ambient"+i.ToString()];
                if (instance.IsPlaying)
                {
                    currAmbs.Add(new Tuple<string, int>(instance.soundName,i));
                }
            }
            return currAmbs;
        }

        public void GetFMODBuses()
        {
            // TODO: add ambient bus
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
            GameManager.Instance.Settings.MasterVolume.Subscribe(val =>
            {
                MasterBus.setVolume(val);
            }).AddTo(this);
            // TODO: uncomment after adding ambient bus
            // GameManager.Instance.Settings.AmbientVolume.Subscribe(val =>
            // {
            //     AmbientBus.setVolume(val);
            // }).AddTo(this);

        }

        [Button]
        public void PlayMusic(string name, float fadein = 0f)
        {
            fmodEvent = eventInstanceMap["music"];
            var obj = audioBank.GetMusic(name);
            var eventPath = obj.Event;

            // if music is currently playing, we need to stop it before making a new 
            // music instance, otherwise we will lose the reference to the musicInstance
            // and it will overlap with new music.
            musicInstance = fmodEvent.EventInstance;
            if (fmodEvent.IsPlaying)
            {
                StopMusic(0);
            }
            musicInstance = RuntimeManager.CreateInstance(eventPath);
            StartCoroutine(FadeVolume(0, 1f, fadein, musicInstance));
            musicInstance.start();
        }
        [Button]
        public void StopMusic(float fadeout = 0f)
        {
            // Debug.Log("now stopping music");
            fmodEvent = eventInstanceMap["music"];
            musicInstance = fmodEvent.EventInstance;
            if (fadeout == 0)
            {
                musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                musicInstance.release();
            }
            else
            {
                StartCoroutine(FadeVolume(1f, 0, fadeout, musicInstance));
                //musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }

        }

        [Button]
        public void PlayAmbient(string name, int channel = 0, float fadein = 0f, float fadeout = 0f)
        {
            fmodEvent = eventInstanceMap["ambient" + channel.ToString()];
            ambientInstance = fmodEvent.EventInstance;
            var obj = audioBank.GetAmbient(name);
            var eventPath = obj.Event;

            if (fmodEvent.IsPlaying)
            {
                StopAmbient(fadeout, channel);
            }
            ambientInstance = RuntimeManager.CreateInstance(eventPath);
            StartCoroutine(FadeVolume(0, 1f, fadein, ambientInstance));
            ambientInstance.start();
        }
        [Button]
        public void StopAmbient(float fadeout = 0f, int channel = 0)
        {
            fmodEvent = eventInstanceMap["ambient" + channel.ToString()];
            ambientInstance = fmodEvent.EventInstance;
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
        public void StopAllAmbient(float fadeout = 0f)
        {
            for (int i = 0; i < numAmbientTracks; i++)
            {
                StopAmbient(fadeout, i);
            }
        }
        [Button]
        public void PlaySFX(string name)
        {
            var obj = audioBank.GetSfx(name);
            var eventPath = obj.Event;
            RuntimeManager.PlayOneShot(eventPath);
        }

        public void PlayUIEvent(UIEventType eventType)
        {
            // TODO: dont forget about specific inventory ui events too
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