using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Sirenix.OdinInspector;

public enum NPC_NAME
{
    HAKON,
    VENDEL,
    LEIF,
    PHYLLIS,
    MC,
    OTHER,
    DIGBY,
    MAJA,
    LYSANDER,
    HERMIA,
    GAGNEF
}
public class NPCBank_SO : SerializedScriptableObject
{
    public Dictionary<NPC_NAME, NPC_SO> namedNPCDatabase = new Dictionary<NPC_NAME, NPC_SO>();
    public Dictionary<string,NPC_SO> allNPCDatabase = new Dictionary<string, NPC_SO>();

    private void OnEnable()
    {
        allNPCDatabase.Clear();
        namedNPCDatabase.Clear();

        var loadStuff = Resources.LoadAll("Characters",typeof(NPC_SO));
        for (int i=0; i < loadStuff.Length; i++)
        {
            var npc = (NPC_SO)loadStuff[i];
            if (!allNPCDatabase.ContainsValue(npc))
            {
                allNPCDatabase[npc.internalName] = npc;
            }
            if (npc.npcName != NPC_NAME.OTHER)
            {
                namedNPCDatabase.Add(npc.npcName,npc);
            }
        }

    }

    public NPC_SO GetNPC(NPC_NAME npcName)
    {
        if (!namedNPCDatabase.ContainsKey(npcName))
        {
            Debug.LogErrorFormat("{0} not in npcDatabase!",npcName);
            return null;
        }
        return namedNPCDatabase[npcName];
    }
}
