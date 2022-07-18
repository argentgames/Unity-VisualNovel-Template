using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "CG Bank", menuName = "Argent Games/Visual Novel Template/ScriptableObjects/CG Bank")]
public class CGBank_SO : SerializedScriptableObject
{
    [HorizontalGroup("CG Database"), LabelWidth(.3f)]
	[VerticalGroup("CG Database/Name ref")]
    [SerializeField]
    public List<string> cgNames = new List<string>();
    
    [HorizontalGroup("CG Database")]
	[VerticalGroup("CG Database/Asset ref")]
    [SerializeField]
    public List<AssetReference> cgAssets = new List<AssetReference>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
