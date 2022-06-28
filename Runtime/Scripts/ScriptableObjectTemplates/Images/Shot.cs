using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace com.argentgames.visualnoveltemplate
{
    /// <summary>
    /// Cinematic background with default location/rotation. 
    /// Used in inkscript to show a new background.
    /// </summary>
    public class Shot : SerializedScriptableObject
    {

        public string bgName;
        public AssetReference bgPrefab;
        public Vector3 position;
        public Vector3 rotation;
        public float size;

        // TODO: maybe we want a shot that has a custom animation, but we want
        // to call the custom animatino at a specified time in the script, rather
        // than just having it play immediately?
        public void PlayAnimation(string animation = "")
        {

        }

    }
}