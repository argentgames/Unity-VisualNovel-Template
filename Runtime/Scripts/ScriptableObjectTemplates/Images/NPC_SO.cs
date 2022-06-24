using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using Sirenix.Serialization;
using Cysharp.Threading.Tasks;
using DG.Tweening;
public abstract class NPC_SO : SerializedScriptableObject
{
    [SerializeField]
    public string internalName;
    public string DisplayName;
    public Color NameColor = new Color(255,255,255,255);
    public Color TextColor = new Color(255,255,255,255);
    public NPC_NAME npcName;
}

public enum Pronouns
{
    He,
    She,
    They
}