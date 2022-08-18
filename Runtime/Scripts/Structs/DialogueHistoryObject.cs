using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.argentgames.visualnoveltemplate;
using TMPro;

namespace com.argentgames.visualnoveltemplate
{
    public class DialogueHistoryObject : MonoBehaviour
    {
        [SerializeField]
        TMP_Text dialogueText, speakerName;
        public void SetData(DialogueHistoryLine historyObject) 
        {
            dialogueText.text = historyObject.line;
            speakerName.text = historyObject.speaker;
            
            // optional icon?
        }

    }
}
