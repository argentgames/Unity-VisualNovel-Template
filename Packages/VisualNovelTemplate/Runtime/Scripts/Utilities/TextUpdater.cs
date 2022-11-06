using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
namespace com.argentgames.visualnoveltemplate
{
    [RequireComponent(typeof(TMP_Text))]
    public class TextUpdater : MonoBehaviour
    {
        [SerializeField]
        TMP_Text text;
        void Awake()
        {
            if (text == null)
            {
                text = this.GetComponentInChildren<TMP_Text>();
            }
            
        }
        public void UpdateText(string newText)
        {
            text.text = newText;
        }
    }
}
