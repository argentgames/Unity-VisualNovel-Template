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
                UpdateToggleObjectsList();
            }
        }

        public void UpdateToggleObjectsList()
        {
            toggleGroupObjects.Clear();
            for (int idx = 0; idx < gameObject.transform.childCount; idx++)
            {
                toggleGroupObjects.Add(gameObject.transform.GetChild(idx).gameObject);
            }
        }

        public void EnableGameObject(GameObject gameObject)
        {
            List<GameObject> nullGos = new List<GameObject>();
            foreach (var go in toggleGroupObjects)
            {
                if (go == null)
                {
                    nullGos.Add(go);
                    continue;
                }
                if (go != gameObject)
                {
                    // Debug.LogFormat("disabling go {0}", go.name);
                    go.SetActive(false);
                }
            }
            gameObject.SetActive(true);

            for (int _i = 0; _i < nullGos.Count; _i++)
            {
                RemoveToggleGameObject(nullGos[_i]);
            }
        }

        public void DisableAllGameObjects()
        {
            foreach (var go in toggleGroupObjects)
            {
                go.SetActive(false);
            }
        }

        public void RemoveToggleGameObject(GameObject go)
        {
            toggleGroupObjects.Remove(go);
            UpdateToggleObjectsList();
        }
    }
}
