using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The dialogue window mode, such as ADV and NVL. We use this data to instantiate the window prefab (which needs to have a UI controller component) 
/// and set currentSaveData's currentDialogueWindowMode so that when we reload a save, we use the correct window mode.
/// </summary>
namespace com.argentgames.visualnoveltemplate
{
    [CreateAssetMenu(fileName = "NPC", menuName = "Argent Games/Visual Novel Template/ScriptableObjects/Dialogue Window")]
    public class DialogueWindowMode_SO : ScriptableObject
    {
        public GameObject prefab;
        public string windowModeName = "";
    }
}
