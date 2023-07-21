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
namespace com.argentgames.visualnoveltemplate
{
    public class NPCBank_SO : SerializedScriptableObject
    {
        public Dictionary<string, NPC_SO> allNPCDatabase = new Dictionary<string, NPC_SO>();
        [SerializeField]
        [PropertyTooltip("The Resources/<characterDir> locatino to automatically populate our NPCBank with.")]
        string characterDir = "Characters";
        private void OnEnable()
        {
            allNPCDatabase.Clear();

            var loadStuff = Resources.LoadAll(characterDir, typeof(NPC_SO));
            for (int i = 0; i < loadStuff.Length; i++)
            {
                var npc = (NPC_SO)loadStuff[i];
                if (!allNPCDatabase.ContainsValue(npc))
                {
                    allNPCDatabase[npc.internalName] = npc;
                }
                

            }

        }

        public NPC_SO GetNPC(string npcName)
        {
            if (!allNPCDatabase.ContainsKey(npcName))
            {
                Debug.LogErrorFormat("{0} not in npcDatabase!, abort", npcName);
                return null;
            }
            return allNPCDatabase[npcName];
        }

    }

}