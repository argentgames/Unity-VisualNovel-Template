using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
public class Shot : SerializedScriptableObject
{

    public string bgName;
    public AssetReference bgPrefab;
    public Vector3 position;
    public Vector3 rotation;
    public float size;

    public void PlayAnimation(string animation = "")
    {

    }

}