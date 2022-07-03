using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using FMODUnity;
using System;

/// <summary>
/// The VideoBank contains all video files used in the game.
/// </summary>
namespace com.argentgames.visualnoveltemplate
{
    [CreateAssetMenu(fileName = "Video Bank", menuName = "Argent Games/Visual Novel Template/ScriptableObjects/Video Bank")]
    public class VideoBank_SO : SerializedScriptableObject
    {
        [InfoBox("These maps are how we refer to video files in game. The maps are automatically populated OnEnable via all files listed in the Data section.")]
        [BoxGroup("Maps")]
        [ShowInInspector, ReadOnly]
        readonly Dictionary<string, Video_SO> videoMap = new Dictionary<string, Video_SO>();

        [PropertySpace(SpaceBefore = 15)]
        [SerializeField]
        [BoxGroup("Data")]
        [InfoBox("Video data")]
        private List<Video_SO> videoData = new List<Video_SO>();

        void OnEnable()
        {
            PopulateMaps();
        }

        public void PopulateMaps()
        {
            videoMap.Clear();

            foreach (var video in videoData)
            {
                videoMap[video.internalName] = video;
            }

            // try to find any Resources/_SO files and automatically populate from there.
            var videos = Resources.LoadAll<Video_SO>(".");
            for (int i = 0; i < videos.Length; i++)
            {
                var video = videos[i];
                videoMap[video.internalName] = video;
            }



        }

        public Video_SO GetVideo(string videoName)
        {
            try
            {
                return videoMap[videoName];
            }
            catch
            {
                Debug.LogErrorFormat("Could not find video [{0}]",videoName);
            }
        }

    }

}