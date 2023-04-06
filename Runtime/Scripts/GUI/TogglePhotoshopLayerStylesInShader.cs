using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace com.argentgames.visualnoveltemplate
{
    public class TogglePhotoshopLayerStylesInShader : MonoBehaviour
    {
        [SerializeField]
        bool multiply = false;
        
SpriteRenderer image;
        private MaterialPropertyBlock _propBlock;
        
        void Awake()
        {
            image = GetComponent<SpriteRenderer>();
            _propBlock = new MaterialPropertyBlock();
            SetMultiply(multiply);
        }
        [Sirenix.OdinInspector.Button]
        public void SetMultiply(bool val)
        {
            multiply = val;

             if (multiply)
            {
                if (_propBlock == null)
                {
                    _propBlock = new MaterialPropertyBlock();
                    Debug.LogWarningFormat("have to make new propblock for toggle photoshop layer style {0}",gameObject.name);
                }
                if (image == null)
                {
                    image = GetComponent<SpriteRenderer>();
                    Debug.LogWarningFormat("have to get image sr in toggle photoshop layer for obj {0}",gameObject.name);
                }
                image.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat("_NeedsMultiply",1);
            image.SetPropertyBlock(_propBlock);
            }
        }
    }
}
