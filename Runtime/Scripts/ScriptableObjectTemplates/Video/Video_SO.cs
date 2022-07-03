using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Video;
using Sirenix.OdinInspector;

/// <summary>
/// Video asset data. Contains data for the video player, e.g. the OP or ED.
/// When playing the video in game, we need to have a separate audio track because
/// of how encodings work.
/// NOT SUPPORTED: urls. Unity does support 
/// </summary>
namespace com.argentgames.visualnoveltemplate
{
    [CreateAssetMenu(fileName = "Video", menuName = "Argent Games/Visual Novel Template/ScriptableObjects/Video")]
    public class Video_SO : ScriptableObject
    {
        public string internalName = "";
        public VideoClip videoClip;
        [InfoBox("Any audio we want to play, synced alongside the video. The track is looked up in the Audio Bank, which lets you use the track outside of the video player.")]
        public string audioName;
        public bool isSkippableWithoutWatching = false;
        public bool HasWatchedOnce { get { return watchCount > 0; }}
        public int watchCount = 0;

    }

}