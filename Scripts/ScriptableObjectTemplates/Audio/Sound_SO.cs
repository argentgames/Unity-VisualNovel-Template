using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using FMODUnity;
public abstract class Sound_SO : SerializedScriptableObject
{
    public abstract string InternalName {
        get;set;
    }
    public abstract bool IsLoop {
        get;
    }
    public abstract EventReference Event { get;set; }

}
