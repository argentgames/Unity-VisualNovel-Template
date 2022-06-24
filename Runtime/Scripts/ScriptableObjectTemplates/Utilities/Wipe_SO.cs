using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using DG.Tweening;
public class Wipe_SO : SerializedScriptableObject
{

    public string internalName;
    public Texture2D wipePrefab;
    public Ease ease = Ease.InCubic;

}