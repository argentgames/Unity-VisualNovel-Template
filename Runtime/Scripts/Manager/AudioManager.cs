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
        public EventInstance EventInstance
        {
            get { return (EventInstance)eventInstance; }
            set { eventInstance = value; }
        }
        public string soundName;

        // QUESTION @Dovah: is it possible to have individual eventInstance volumes, separate from the Bus?
        public float trackVolume;

        public FMODEventInstanceData(
            SoundType soundType,
            EventInstance? eventInstance,
            string soundName,
            bool isplaying,
            float trackvolume
        )
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
        public AudioBank_SO AudioBank => audioBank;

        [SerializeField]
        [InfoBox(
            "How many ambient tracks do we want to be able to play simultaneously? 3 is a pretty safe number, increase at your own risk of confusion!"
        )]
        int numAmbientTracks = 3;

        private Dictionary<string, FMODEventInstanceData> eventInstanceMap =
            new Dictionary<string, FMODEventInstanceData>();

        private FMODEventInstanceData fmodEvent;
        private EventInstance musicInstance,
            ambientInstance,
            sfxInstance;

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

            eventInstanceMap = new Dictionary<string, FMODEventInstanceData>();

            // create all our eventInstanceMap so we can have unlimited ambient tracks :>
            eventInstanceMap["music"] = new FMODEventInstanceData(
                SoundType.Music,
                new EventInstance(),
                "",
                false,
                1.0f
            );
            eventInstanceMap["sfx"] = new FMODEventInstanceData(
                SoundType.SFX,
                new EventInstance(),
                "",
                false,
                1.0f
            );
            Debug.Log(eventInstanceMap["music"].EventInstance);
            for (int i = 0; i < numAmbientTracks; i++)
            {
                eventInstanceMap["ambient" + i.ToString()] = new FMODEventInstanceData(
                    SoundType.Ambient,
                    new EventInstance(),
                    "",
                    false,
                    1.0f
                );
            }

            await UniTask.WaitUntil(() => GameManager.Instance != null);

            SetRXSubscriptions();
        }

        /// <summary>
        /// TODO: We might want to return an object that contains soundName, eventInstanceVolume, and other information.
        /// </summary>
        /// <returns></returns>
        [Button]
        public string GetCurrentPlayingMusic()
        {
            if (eventInstanceMap["music"].IsPlaying)
            {
                return eventInstanceMap["music"].soundName;
            }
            else
            {
                Debug.Log("music not currently playing!");
                return "";
            }
        }

        public List<Tuple<string, int>> GetCurrentPlayingAmbients()
        {
            List<Tuple<string, int>> currAmbs = new List<Tuple<string, int>>();
            for (int i = 0; i < numAmbientTracks; i++)
            {
                var instance = eventInstanceMap["ambient" + i.ToString()];
                if (instance.IsPlaying)
                {
                    currAmbs.Add(new Tuple<string, int>(instance.soundName, i));
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
            GameManager.Instance.Settings.MusicVolume
                .Subscribe(val =>
                {
                    MusicBus.setVolume(val);
                })
                .AddTo(this);
            GameManager.Instance.Settings.SFXVolume
                .Subscribe(val =>
                {
                    SFXBus.setVolume(val);
                })
                .AddTo(this);
            GameManager.Instance.Settings.MasterVolume
                .Subscribe(val =>
                {
                    MasterBus.setVolume(val);
                })
                .AddTo(this);
            // TODO: uncomment after adding ambient bus
            // GameManager.Instance.Settings.AmbientVolume.Subscribe(val =>
            // {
            //     AmbientBus.setVolume(val);
            // }).AddTo(this);
        }

        [Button]
        public void PlayMusic(string _name, float fadein = 0f, bool showClosedCaption = false)
        {
            try
            {
                fmodEvent = eventInstanceMap["music"];
                var obj = audioBank.GetMusic(_name);
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
                fmodEvent.EventInstance = musicInstance;
                fmodEvent.soundName = _name;
                eventInstanceMap["music"] = fmodEvent;
                if (showClosedCaption)
                {
                    if (GameManager.Instance.Settings.enableClosedCaptions.Value)
                    {
                        if (obj.ClosedCaption != "")
                        {
                            NotificationManager.Instance.ShowTextNotification(obj.ClosedCaption);
                        }
                    }
                }
            }
            catch
            {
                Debug.LogErrorFormat("Unable to play music: [{0}]", _name);
            }
        }

        [Button]
        public void StopMusic(float fadeout = 0f)
        {
            try
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
            catch (Exception e)
            {
                Debug.LogWarningFormat("unable to stop music with error {0}", e);
            }
        }

        [Button]
        public void PlayAmbient(
            string _name,
            int channel = 0,
            float fadein = 0f,
            float fadeout = 0f,
            bool showClosedCaption = false
        )
        {
            try
            {
                fmodEvent = eventInstanceMap["ambient" + channel.ToString()];
                ambientInstance = fmodEvent.EventInstance;
                var obj = audioBank.GetAmbient(_name);
                var eventPath = obj.Event;

                if (fmodEvent.IsPlaying)
                {
                    StopAmbient(fadeout, channel);
                }
                ambientInstance = RuntimeManager.CreateInstance(eventPath);
                StartCoroutine(FadeVolume(0, 1f, fadein, ambientInstance));
                ambientInstance.start();

                fmodEvent.EventInstance = ambientInstance;
                fmodEvent.soundName = _name;
                eventInstanceMap["ambient" + channel.ToString()] = fmodEvent;

                if (showClosedCaption)
                {
                    if (GameManager.Instance.Settings.enableClosedCaptions.Value)
                    {
                        if (obj.ClosedCaption != "")
                        {
                            NotificationManager.Instance.ShowTextNotification(obj.ClosedCaption);
                        }
                    }
                }
            }
            catch
            {
                Debug.LogErrorFormat("Unable to play Ambient: [{0}]", _name);
            }
        }

        [Button]
        public void StopAmbient(float fadeout = 0f, int channel = 0)
        {
            try
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
            catch (Exception e)
            {
                Debug.LogWarningFormat(
                    "unable to stop ambient on channel {0} with exception {1}",
                    channel,
                    e
                );
            }
        }

        public void StopAllAmbient(float fadeout = 0f)
        {
            var currentPlayingAmbients = GetCurrentPlayingAmbients();
            for (int i = 0; i < currentPlayingAmbients.Count; i++)
            {
                StopAmbient(fadeout, currentPlayingAmbients[i].Item2);
            }
        }

        [Button]
        public void PlaySFX(string _name, bool showClosedCaption = false)
        {
            try
            {
                var obj = audioBank.GetSfx(_name);
                var eventPath = obj.Event;
                RuntimeManager.PlayOneShot(eventPath);

                if (showClosedCaption)
                {
                    if (GameManager.Instance.Settings.enableClosedCaptions.Value)
                    {
                        if (obj.ClosedCaption != "")
                        {
                            NotificationManager.Instance.ShowTextNotification(obj.ClosedCaption);
                        }
                    }
                }
            }
            catch
            {
                Debug.LogErrorFormat("Unable to play sfx: [{0}]", _name);
            }
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
