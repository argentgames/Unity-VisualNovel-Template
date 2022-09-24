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
            if (multiply)
            {
                image.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat("_NeedsMultiply",1);
            image.SetPropertyBlock(_propBlock);
            }
        }
        public void SetMultiply(bool val)
        {
            multiply = val;
        }
    }
}
