using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.argentgames.visualnoveltemplate
{
    [CreateAssetMenu(fileName = "NPC", menuName = "Argent Games/Visual Novel Template/ScriptableObjects/Dialogue Window")]
    public class DialogueWindowMode_SO : ScriptableObject
    {
        public GameObject prefab;
        public string windowModeName = "";
    }
}
