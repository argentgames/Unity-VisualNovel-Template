using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using Sirenix.Serialization;
using Cysharp.Threading.Tasks;
public class MC_NPC_SO : NPC_SO
{
    public Pronouns pronouns = Pronouns.They;

    void OnEnable()
    {
        npcName = NPC_NAME.MC;
    }

}
