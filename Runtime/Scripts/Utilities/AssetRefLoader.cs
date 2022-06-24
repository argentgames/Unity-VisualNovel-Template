using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using System;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;

public class AssetRefLoader : MonoBehaviour
{
    public static AssetRefLoader Instance { get; set; }
    [SerializeField]
    private Dictionary<AsyncOperationHandle, GameObject> assetMap = new Dictionary<AsyncOperationHandle, GameObject>();
    [SerializeField]
    private Dictionary<GameObject, AsyncOperationHandle> assetMapReversed = new Dictionary<GameObject, AsyncOperationHandle>();
    [SerializeField]
    private List<AssetReference> assetsToPreload = new List<AssetReference>();
    [SerializeField]
    private List<AssetReference> charSpritesToPreloadInGameScene = new List<AssetReference>();
    public static bool IsDoneLoadingCharacters = false;
    async UniTaskVoid Awake()
    {
        Instance = this;
        // #if !UNITY_EDITOR && !PLATFORM_ANDROID
        assetMap.Clear();
        await Resources.UnloadUnusedAssets();
        foreach (var asset in assetsToPreload)
        {
            await LoadAsset(asset, this.transform);
        }
        // var CameraShots = Resources.LoadAll<Shot>("camera shots");
        // for (int i = 0; i < CameraShots.Length; i++)
        // {
        //     var shot = CameraShots[i];
        //     LoadAsset(shot.bgPrefab,this.transform);
        // }
        // #endif

    }
    public async UniTask LoadCharacters()
    {
#if !UNITY_EDITOR && !PLATFORM_ANDROID
        var go = GameObject.Find("CharacterWrapper");
        Transform transform = null;
        if (go  != null)
        {
            transform = go.transform;
        }
        else
        {
            transform = this.transform;
        }
        foreach (var asset in charSpritesToPreloadInGameScene)
        {
            LoadAsset(asset,transform);
            
        }
        IsDoneLoadingCharacters = true;
#endif
    }

    public async UniTask<GameObject> LoadAsset(AssetReference asset, Transform transform)
    {
        // if (assetMap.ContainsKey(asset) && assetMap[asset] != null)
        // {
        //     Debug.Log("returning a reuesed go");
        //     var go = assetMap[asset];
        //     go.transform.SetParent(transform);
        //     return go;
        // }
        // else
        // {
        //     Debug.Log("creating a new go");
        GameObject handler;
        GameObject instantiatedGO = null;
        if (asset.OperationHandle.IsValid())
        {
            await UniTask.WaitWhile(() => asset.OperationHandle.Status == AsyncOperationStatus.None);
            switch (asset.OperationHandle.Status)
            {
                case AsyncOperationStatus.Succeeded:
                    Debug.Log("status is SUCCEEDEDDDDDD ALL WE GOTTA DO IS REUSE A CACHED HANDLER");
                    handler = assetMap[asset.OperationHandle];
                    instantiatedGO = GameObject.Instantiate(handler, transform);
                    instantiatedGO.SetActive(false);
                    break;
                case AsyncOperationStatus.Failed:
                    Debug.Log("sttaus is FAILED idk why but this snt safe");
                    Addressables.Release(asset.OperationHandle);
                    assetMap.Remove(asset.OperationHandle);

                    // handler = await asset.LoadAssetAsync<GameObject>();
                    handler = (GameObject)asset.OperationHandle.Result;
                    assetMap[asset.OperationHandle] = handler;

                    instantiatedGO = GameObject.Instantiate(handler, transform);
                    // assetMapReversed[instantiatedGO] = asset.OperationHandle;
                    instantiatedGO.SetActive(false);
                    break;
                case AsyncOperationStatus.None:
                    Debug.Log("statys is NONE, probably never seen this asset before");
                    // handler = await asset.LoadAssetAsync<GameObject>();
                    handler = (GameObject)asset.OperationHandle.Result;
                    assetMap[asset.OperationHandle] = handler;

                    instantiatedGO = GameObject.Instantiate(handler, transform);
                    assetMapReversed[instantiatedGO] = asset.OperationHandle;
                    instantiatedGO.SetActive(false);
                    break;
                default:
                    Debug.Log("no idea");
                    break;
            }
        }
        else
        {
            Debug.Log("statys INVALID, probably never seen this asset before");
            // handler = await asset.LoadAssetAsync<GameObject>();
            handler = (GameObject)asset.OperationHandle.Result;
            assetMap[asset.OperationHandle] = handler;

            instantiatedGO = GameObject.Instantiate(handler, transform);
            assetMapReversed[instantiatedGO] = asset.OperationHandle;
            instantiatedGO.SetActive(false);
        }






        return instantiatedGO;
        // }

    }
    [Button]
    public void ReleaseAsset(GameObject go)
    {
        Debug.Log("Releasing go: " + go.name);

        if (assetMapReversed.ContainsKey(go))
        {
            if (assetMapReversed[go].IsValid())
            {
                Debug.Log("releasing asset");

                Addressables.Release(assetMapReversed[go]);
                assetMapReversed.Remove(go);
                Addressables.ReleaseInstance(go);
                try
                {
                    Destroy(go);
                }
                catch
                {
                    Debug.Log("addressables release instance already desttroyed go");
                }
            }
            else
            {
                Debug.Log("not releasing but yes removing from assetmap");
                assetMapReversed.Remove(go);
                Destroy(go);
            }
        }


        else
        {
            Debug.Log("only destroying go");
            Destroy(go);
        }

    }

    void OnApplicationQuit()
    {
        Resources.UnloadUnusedAssets();
    }

}
