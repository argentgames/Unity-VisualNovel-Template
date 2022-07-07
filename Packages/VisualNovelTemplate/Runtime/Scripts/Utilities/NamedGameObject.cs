using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.argentgames.visualnoveltemplate
{
    public class NamedGameObject : MonoBehaviour
    {
        public string internalName;

        void Awake()
        {
            if (internalName == null || internalName == "")
            {
                internalName = gameObject.name;
            }
        }
    }
}
