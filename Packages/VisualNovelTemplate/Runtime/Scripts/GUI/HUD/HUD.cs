using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.argentgames.visualnoveltemplate
{
    public class HUD : MonoBehaviour
    {
        [SerializeField]
        GameObject pc, mobile;
        // [SerializeField]
        // ingamehud pc/mobile
        void Awake()
        {
            // TODO: Set the hud controller depending on what platform we're on
            #if UNITY_ANDROID || UNITY_IOS
            pc.SetActive(false);
            mobile.SetActive(true);
            #else
            pc.SetActive(true);
            mobile.SetActive(false);
            #endif
        }


    }
}
