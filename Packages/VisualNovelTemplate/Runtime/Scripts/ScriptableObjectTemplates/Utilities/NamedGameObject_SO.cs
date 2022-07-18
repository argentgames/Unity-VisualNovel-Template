using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
/// <summary>
/// Data container where you can refer to a gameobject or assetreference prefab by name, since Unity doesn't have
/// nice Dictionary serialization.
/// </summary>
namespace com.argentgames.visualnoveltemplate
{
    [CreateAssetMenu(fileName = "Named GO", menuName = "Argent Games/Visual Novel Template/ScriptableObjects/Named Game Object")]
    public class NamedGameObject_SO : ScriptableObject
    {
        public AssetReference assetReference;
        public GameObject prefab;
        public string internalName = "";
    }
}
