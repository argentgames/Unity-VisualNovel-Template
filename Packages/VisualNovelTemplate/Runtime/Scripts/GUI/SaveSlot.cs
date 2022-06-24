using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class SaveSlot : MonoBehaviour
{
    public TMP_Text emptyText, date;
    public Image screenshot;
    // Start is called before the first frame update
    void Awake()
    {
         emptyText.text = "Empty Slot";
        date.text = "";
    }
    void Start()
    {
        // emptyText.text = "Empty Slot";
        // date.text = "";
    }

 
}
