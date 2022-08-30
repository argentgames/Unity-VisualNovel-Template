using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using com.argentgames.visualnoveltemplate;

namespace com.argentgames.visualnoveltemplate
{
    /// <summary>
    /// History objects are shown in a scrollbox.
    /// </summary>
    public class ExampleHistoryPage : MonoBehaviour
    {
        [SerializeField]
        GameObject contentHolder;
        [SerializeField]
        GameObject historyPrefab;

        void Awake()
        {
            Utilities.DestroyAllChildGameObjects(contentHolder);
        }
        void OnEnable()
        {
            foreach (var historyObject in DialogueSystemManager.Instance.currentSessionDialogueHistory)
            {
                var go = Instantiate(historyPrefab,contentHolder.transform);
                var history = go.GetComponentInChildren<DialogueHistoryObject>();
                history.SetData(historyObject);
            }
        }
        void OnDisable()
        {
            Utilities.DestroyAllChildGameObjects(contentHolder);
        }

    }
}
