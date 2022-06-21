using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
public class GenericTexts_SO : SerializedScriptableObject
{
    public Dictionary< GenericText, string> Texts = new Dictionary<GenericText, string>();
}

public enum GenericText {
    START_QUOTE_CHAR,
    END_QUOTE_CHAR
}