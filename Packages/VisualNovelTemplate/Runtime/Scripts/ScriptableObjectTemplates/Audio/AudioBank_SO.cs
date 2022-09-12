using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using FMODUnity;
using System;
/// <summary>
/// The AudioBank contains all the sounds used in the game. Populate the Data sounds, and maps will automatically be created.
/// Use the internalName to access the sound in game.
/// You can also optionally manually create scriptable objects and place them in the Resources folder.
/// If the bank finds any Sound_SO files in Resources, it will add them to the maps.
/// BE CAREFUL OF MIXING THE METHODS FOR POPULATING THE MAPS. You don't want to be confused about where map 
/// files are coming from...
/// </summary>
namespace com.argentgames.visualnoveltemplate
{
    [CreateAssetMenu(fileName = "Audio Bank", menuName = "Argent Games/Visual Novel Template/ScriptableObjects/Audio Bank")]
    public class AudioBank_SO : SerializedScriptableObject
    {
        [InfoBox("These maps are how we refer to music files in game. The maps are automatically populated OnEnable via all files listed in the Data section.")]
        [BoxGroup("Maps")]
        [ShowInInspector, ReadOnly]
        readonly Dictionary<string, Music_SO> musicMap = new Dictionary<string, Music_SO>();
        [BoxGroup("Maps")]
        [ShowInInspector, ReadOnly]
        readonly Dictionary<string, Ambient_SO> ambientMap = new Dictionary<string, Ambient_SO>();
        [BoxGroup("Maps")]
        [ShowInInspector, ReadOnly]
        readonly Dictionary<string, SFX_SO> sfxMap = new Dictionary<string, SFX_SO>();
        [BoxGroup("Maps")]
        [ShowInInspector, ReadOnly]
        readonly Dictionary<string, SFX_SO> uiSfxMap = new Dictionary<string, SFX_SO>();

        [PropertySpace(SpaceBefore = 15)]
        [SerializeField]
        [BoxGroup("Data")]
        [InfoBox("Ambient sounds. These loop. Optionally specify a default channel to play the sound.")]
        [ListDrawerSettings(NumberOfItemsPerPage = 5)]
        private List<AudioData> ambientData = new List<AudioData>();
        [SerializeField]
        [BoxGroup("Data")]
        [InfoBox("One-shot sounds. These do not loop.")]
        [ListDrawerSettings(NumberOfItemsPerPage = 5)]
        private List<AudioData> sfxData = new List<AudioData>();
        [SerializeField]
        [BoxGroup("Data")]
        [InfoBox("Music sounds. These loop.")]
        [ListDrawerSettings(NumberOfItemsPerPage = 5)]
        private List<AudioData> musicData = new List<AudioData>();
        [SerializeField]
        [BoxGroup("Data")]
        [InfoBox("UI SFX sounds. These don't loop.")]
        [ListDrawerSettings(NumberOfItemsPerPage = 5)]
        private List<AudioData> uiSfxData = new List<AudioData>();

        [SerializeField]
        string ambientPath = "Sound/Ambient", musicPath = "Sound/Music", sfxPath = "Sound/SFX", uiSfxPath = "Sound/SFX/UI";
        void OnEnable()
        {
            PopulateMaps();
        }
        public void PrintData()
        {
            Debug.LogFormat("num sfxData: {0}",sfxData.Count);
            foreach (var ad in sfxData)
            {
                Debug.LogFormat("{0}",ad.internalName);
            }
        }

        // [Button("Populate maps")]
        /// <summary>
        /// Clear all the maps and populate with data from the Data lists and the Resources folder.
        /// </summary>
        public async void PopulateMaps()
        {
            Debug.Log("populating audio maps now");
            musicMap.Clear();
            ambientMap.Clear();
            sfxMap.Clear();
            uiSfxMap.Clear();

            for (int i = 0; i < ambientData.Count; i++)
            {
                var f = ambientData[i];
                var s = ScriptableObject.CreateInstance<Ambient_SO>();
                s.InternalName = f.internalName;
                s.Event = f.eventReference;
                s.ClosedCaption = f.closedCaption;
                ambientMap.Add(s.InternalName, s);
            }
            for (int i = 0; i < sfxData.Count; i++)
            {
                var f = sfxData[i];
                var s = ScriptableObject.CreateInstance<SFX_SO>();
                s.InternalName = f.internalName;
                s.Event = f.eventReference;
                s.ClosedCaption = f.closedCaption;
                sfxMap.Add(s.InternalName, s);
            }
            for (int i = 0; i < uiSfxData.Count; i++)
            {
                var f = uiSfxData[i];
                var s = ScriptableObject.CreateInstance<SFX_SO>();
                s.InternalName = f.internalName;
                s.Event = f.eventReference;
                s.ClosedCaption = f.closedCaption;
                uiSfxMap.Add(s.InternalName, s);
            }
            for (int i = 0; i < musicData.Count; i++)
            {
                var f = musicData[i];
                var s = ScriptableObject.CreateInstance<Music_SO>();
                s.InternalName = f.internalName;
                s.Event = f.eventReference;
                s.ClosedCaption = f.closedCaption;
                musicMap.Add(s.InternalName, s);
            }

            // try to find any Resources/_SO files and automatically populate from there.
            var sounds = Resources.LoadAll<Sound_SO>(ambientPath);
            Debug.LogFormat("number of ambient sounds found: {0}",sounds.Length);
            for (int i = 0; i < sounds.Length; i++)
            {
                var sound = sounds[i];
                 if (sound is Ambient_SO)
                {
                    ambientMap[sound.InternalName] = (Ambient_SO)sound;
                }
            }
            sounds = Resources.LoadAll<Sound_SO>(musicPath);
            Debug.LogFormat("number of music sounds found: {0}",sounds.Length);
            for (int i = 0; i < sounds.Length; i++)
            {
                var sound = sounds[i];
                 if (sound is Music_SO)
                {
                    musicMap[sound.InternalName] = (Music_SO)sound;
                }
            }
            sounds = Resources.LoadAll<Sound_SO>(sfxPath);
            Debug.LogFormat("number of sfx sounds found: {0}",sounds.Length);
            for (int i = 0; i < sounds.Length; i++)
            {
                var sound = sounds[i];
                 if (sound is SFX_SO)
                {
                    sfxMap[sound.InternalName] = (SFX_SO)sound;
                }
            }
            sounds = Resources.LoadAll<Sound_SO>(uiSfxPath);
            Debug.LogFormat("number of ui sfx sounds found: {0}",sounds.Length);
            for (int i = 0; i < sounds.Length; i++)
            {
                var sound = sounds[i];
                 if (sound is SFX_SO)
                {
                    uiSfxMap[sound.InternalName] = (SFX_SO)sound;
                }
            }



        }

        public Music_SO GetMusic(string trackName)
        {
            try
            {
                return musicMap[trackName];
            }
            catch
            {
                Debug.LogErrorFormat("Could not find music [{0}]",trackName);
                return null;
            }
        }
        public SFX_SO GetSfx(string trackName)
        {
            try
            {
                return sfxMap[trackName];
            }
            catch
            {
                Debug.LogErrorFormat("Could not find sfx [{0}]",trackName);
                return null;
            }
        }
        public Ambient_SO GetAmbient(string trackName)
        {
            try
            {
                return ambientMap[trackName];
            }
            catch
            {
                Debug.LogErrorFormat("Could not find ambient [{0}]",trackName);
                return null;
            }
        }
    }

}