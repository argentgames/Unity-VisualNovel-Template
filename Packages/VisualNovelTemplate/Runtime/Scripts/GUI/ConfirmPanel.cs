using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ConfirmPanel : MonoBehaviour
{
    public TMP_Text heading;
    [SerializeField]
    [Tooltip("Text used to populate the heading field on Start")]
    string headingContent;
    public Button yes, no;
    // Start is called before the first frame update
    void Awake()
    {
        heading.text = headingContent;
    }

    public void SetHeader(string content)
    {
        heading.text = content;
    }

    
}
