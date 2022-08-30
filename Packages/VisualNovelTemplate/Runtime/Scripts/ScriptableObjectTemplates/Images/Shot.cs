using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace com.argentgames.visualnoveltemplate
{
    [CreateAssetMenu(fileName = "Shot", menuName = "Argent Games/Visual Novel Template/ScriptableObjects/Shot")]

    /// <summary>
    /// Cinematic background with default location/rotation. 
    /// Used in inkscript to show a new background.
    /// </summary>
    public class Shot : SerializedScriptableObject
    {

        public string bgName;
        public bool UseAddressables = false;
        [HideIf("@this.UseAddressables")]
        [DisableIf("@this.bgAssetReference.RuntimeKeyIsValid()")]
        public GameObject bgPrefab;
        [HideIf("@this.UseAddressables == false")]
        [DisableIf("@this.bgPrefab != null")]
        public AssetReference bgAssetReference;
        public Vector3 position;
        public Vector3 rotation;
        /// <summary>
        /// Orthographic camera size (zoom amount)
        /// </summary>
        public float size;
        public ImageLayer imageLayer = ImageLayer.CurrentBackground;

        // TODO: maybe we want a shot that has a custom animation, but we want
        // to call the custom animatino at a specified time in the script, rather
        // than just having it play immediately?
        public void PlayAnimation(string animation = "")
        {

        }

    }
}