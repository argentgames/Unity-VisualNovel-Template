using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace com.argentgames.visualnoveltemplate
{
    public class Utilities : MonoBehaviour
    {
        public static void DestroyAllChildGameObjects(GameObject parent)
        {
            // for (int i=parent.transform.childCount; i > 0; i--)
            // {
            //     Destroy(parent.transform.GetChild(i).gameObject);
            // }

            for (int i=0; i < parent.transform.childCount; i++)
            {
                Destroy(parent.transform.GetChild(i).gameObject);
            }
        }
    }
}