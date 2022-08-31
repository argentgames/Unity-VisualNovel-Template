using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace com.argentgames.visualnoveltemplate
{
    /// <summary>
    /// Often we have a set of toggles that we want to affect some other set of gameobjects.
    /// </summary>
    public class ToggleGroupExtension : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("If left empty, automatically populates with all immediate children.")]
        HashSet<GameObject> toggleGroupObjects = new HashSet<GameObject>();
        
        void Awake()
        {
            if (toggleGroupObjects.Count == 0)
            {
                for (int idx=0; idx < gameObject.transform.childCount; idx++)
                {
                    toggleGroupObjects.Add(gameObject.transform.GetChild(idx).gameObject);
                }
            }
        }
        public void UpdateToggleObjectsList()
        {
for (int idx=0; idx < gameObject.transform.childCount; idx++)
                {
                    toggleGroupObjects.Add(gameObject.transform.GetChild(idx).gameObject);
                }
        }
        public void EnableGameObject(GameObject gameObject)
        {
            foreach (var go in toggleGroupObjects)
            {
                if (go != gameObject)
                {
                    go.SetActive(false);
                }
            }
            gameObject.SetActive(true);
        }
        public void DisableAllGameObjects()
        {
            foreach (var go in toggleGroupObjects)
            {
                go.SetActive(false);
            }
        }
    }
}
