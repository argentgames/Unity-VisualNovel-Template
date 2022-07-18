using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
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

    // TODO: have custom editor so we can remove odin inspector dependency
    // [CustomEditor(typeof(NamedGameObject))]
    // public class NamedGameObjectEditor : Editor
    // {
    //     SerializedProperty namedGameObject;
    //     void OnEnable()
    //     {
    //         namedGameObject = serializedObject.FindProperty("namedGameObject");
    //     }
    //     public override void OnInspectorGUI()
    //     {
    //         serializedObject.Update();
    //         EditorGUILayout.PropertyField(namedGameObject);
    //         serializedObject.ApplyModifiedProperties();
    //     }
    // }
}
