using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace com.argentgames.visualnoveltemplate
{


public class ChoiceButton : MonoBehaviour
{
    [SerializeField]
    Sprite hasPreviouslySelectedChoiceStyle, hasPreviouslySelectedChoiceHoverStyle, regularChoiceStyle, regularHoverStyle;
    [SerializeField]
    TMP_Text choiceText;
    public string pathString = "";
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetChoiceStyle()
    {
        if (DialogueSystemManager.Instance.PreviouslySelectedChoice(choiceText.text,pathString))
        {
            // TODO: set the styles hasPrevoiuslySelectedChoice styles
        }
        else
        {
            // TODO: do nothing, or set the regular choice style
        }
    }
}
}