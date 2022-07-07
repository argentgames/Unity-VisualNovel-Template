using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.argentgames.visualnoveltemplate
{
    public class ExampleMainMenuSettingsPageController : MonoBehaviour
    {

        void OnDisable()
        {
            SaveLoadManager.Instance.SaveSettings();
        }
        void OnDestroy()
        {
            SaveLoadManager.Instance.SaveSettings();
        }
    }
}
