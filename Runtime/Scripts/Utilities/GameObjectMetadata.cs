using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.argentgames.visualnoveltemplate;

namespace com.argentgames.visualnoveltemplate
{
    /// <summary>
    /// Add extra metadata to gameobjects so we don't have to use the name of the GO.
    /// </summary>
    public class GameObjectMetadata : MonoBehaviour
    {
        [SerializeField]
        private string internalName;
        public string InternalName
        {
            get
            {
                if (internalName != null)
                {
                    return internalName;
                }
                else
                {
                    return "";
                }
            }
        }
        [SerializeField]
        private string displayName = "";
        public string DisplayName { get { return displayName; }}


        void Awake()
        {
            if (internalName == null || internalName == "")
            {
                internalName = gameObject.name;
            }
        }

        [SerializeField]
        private string nameReferenceToOtherGameObject = "";
        public string NameReferenceToOtherGameObject { get { return nameReferenceToOtherGameObject; }}

        [SerializeField]
        private GameObject referenceToOtherGameObject;
        public GameObject ReferenceToOtherGameObject { get { return referenceToOtherGameObject; }}
    }
}
