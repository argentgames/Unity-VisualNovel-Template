using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using Sirenix.Serialization;
using Cysharp.Threading.Tasks;
public class GenericNPC_SO : NPC_SO
{
    void OnEnable()
    {
        npcName = NPC_NAME.OTHER;
    }


}
