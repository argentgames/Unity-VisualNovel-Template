using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Threading;
namespace com.argentgames.visualnoveltemplate
{
    public class Utilities : MonoBehaviour
    {
        public static  GameObject GetRootParent(GameObject go)
        {
            if (go.transform.parent == null)
            {
                return go;
            }
            List<string> hierarchyNames = new List<string>();
            GameObject parent = go.transform.parent.gameObject;
            while (parent.transform.parent != null)
            {
                parent = parent.transform.parent.gameObject;
                hierarchyNames.Add(parent.name);
            }
            hierarchyNames.Reverse();
            string s="";
            for (int i = 0; i < hierarchyNames.Count; i++)
            {

                s += new string('-',(i+1)*2)  + hierarchyNames[i]+"\n";
            }
            // Debug.Log(s);
            return parent;
        }
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

            /// <summary>
    /// Cast a ray to test if Input.mousePosition is over any UI object in EventSystem.current. This is a replacement
    /// for IsPointerOverGameObject() which does not work on Android in 4.6.0f3
    /// </summary>
    public static bool IsPointerOverUIObject(float mousePosX, float mousePosY)
    {

        // Referencing this code for GraphicRaycaster https://gist.github.com/stramit/ead7ca1f432f3c0f181f
        // the ray cast appears to require only eventData.position.
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(mousePosX, mousePosY);

        // Debug.LogFormat("testing raycast ui at position {0}",MousePosition.Instance.Position);

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        List<GameObject> blockingResults = new List<GameObject>();
        foreach (var res in results)
        {
            var go = res.gameObject;
            var inputBlocker = go.GetComponentInParent<InputBlocker>();
            GraphicRaycaster graphicsRaycaster;
            if (inputBlocker != null)
            {
                graphicsRaycaster = inputBlocker.GetComponent<GraphicRaycaster>();
            }
            else
            {
                continue;
            }
            if (graphicsRaycaster == null)
            {
                Debug.LogFormat("couldnt find graphicraycaster in parent for object {0}", go.name);
                graphicsRaycaster = go.GetComponentInChildren<GraphicRaycaster>();
            }

            // graphicsRaycaster = go.GetComponentInChildren<GraphicRaycaster>();
            // Debug.LogFormat("does go {0} have a graphicsraycaster?: {1}", go.name, graphicsRaycaster);
            if (graphicsRaycaster != null)
            {
                Debug.LogFormat("graphics raycaster has blockingObjects: {0}", graphicsRaycaster.blockingObjects);
                if (graphicsRaycaster.blockingObjects != GraphicRaycaster.BlockingObjects.None)
                {
                    Debug.LogFormat("graphics raycaster blocking objects is {0}", graphicsRaycaster.blockingObjects);
                    blockingResults.Add(go);
                }
            }

            // Debug.Log("number of blocking raycast objects: " + blockingResults.Count.ToString());
        }
        return blockingResults.Count > 0;
    }
    }
}