using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class Screenshot : MonoBehaviour
{
    public int superSize = 1;
    [Button]
    public void TakeScreenshot(string filePath)
    {
        ScreenCapture.CaptureScreenshot(Application.persistentDataPath + "/" + filePath + ".PNG", superSize);
        Debug.Log("Saved screenshot to: " + Application.persistentDataPath + "/" + filePath);
    }   
}
