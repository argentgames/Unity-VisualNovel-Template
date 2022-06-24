using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ConfirmPanel : MonoBehaviour
{
    public TMP_Text heading;
    public string headingContent;
    public Button yes, no;
    // Start is called before the first frame update
    void Start()
    {
        heading.text = headingContent;
    }


    
}
