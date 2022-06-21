using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class BuildVersionText : MonoBehaviour
{
    public TMP_Text tmpText;
    // Start is called before the first frame update
    void Start()
    {
        tmpText.text = "Build v" + Application.version + "";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
