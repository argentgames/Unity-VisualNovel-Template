using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace com.argentgames.visualnoveltemplate
{
    public class MaterialPropertyBlockUtilities : MonoBehaviour
    {
        SpriteRenderer spriteRenderer;
        MaterialPropertyBlock propBlock;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            propBlock = new MaterialPropertyBlock();
        }

        public Texture GetTexture(string propName)
        {
            spriteRenderer.GetPropertyBlock(propBlock);
            return propBlock.GetTexture(propName);
        }

        public void UpdateProperties(Tuple<string, float> prop, int materialIndex = 0)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
            if (propBlock == null)
            {
                propBlock = new MaterialPropertyBlock();
            }
            spriteRenderer.GetPropertyBlock(propBlock);

            Debug.LogFormat("setting property {0} to val {1}", prop.Item1, prop.Item2);
            propBlock.SetFloat(prop.Item1, prop.Item2);

            spriteRenderer.SetPropertyBlock(propBlock);
        }

        public void UpdateProperties(Tuple<string, Texture2D> prop, int materialIndex = 0)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
            if (propBlock == null)
            {
                propBlock = new MaterialPropertyBlock();
            }
            spriteRenderer.GetPropertyBlock(propBlock);

            Debug.LogFormat("setting property {0} to val {1}", prop.Item1, prop.Item2);
            propBlock.SetTexture(prop.Item1, prop.Item2);

            spriteRenderer.SetPropertyBlock(propBlock);

            spriteRenderer.GetPropertyBlock(propBlock);
            Debug.LogFormat(
                "after setting property for go {1} with parent {2}, we have current val {0}",
                propBlock.GetTexture(prop.Item1).name,
                this.gameObject.name,
                this.gameObject.transform.parent.name
            );
        }

        public void UpdateProperties(Tuple<string, Color> prop, int materialIndex = 0)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
            if (propBlock == null)
            {
                propBlock = new MaterialPropertyBlock();
            }
            spriteRenderer.GetPropertyBlock(propBlock);

            Debug.LogFormat("setting property {0} to val {1}", prop.Item1, prop.Item2);
            propBlock.SetColor(prop.Item1, prop.Item2);

            spriteRenderer.SetPropertyBlock(propBlock);
        }

        public void UpdateProperties(List<Tuple<string, float>> props, int materialIndex = 0)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
            if (propBlock == null)
            {
                propBlock = new MaterialPropertyBlock();
            }
            spriteRenderer.GetPropertyBlock(propBlock);
            foreach (var prop in props)
            {
                Debug.LogFormat("setting property {0} to val {1}", prop.Item1, prop.Item2);
                propBlock.SetFloat(prop.Item1, prop.Item2);
            }

            spriteRenderer.SetPropertyBlock(propBlock);
        }

        public void UpdateProperties(List<Tuple<string, Texture2D>> props, int materialIndex = 0)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
            if (propBlock == null)
            {
                propBlock = new MaterialPropertyBlock();
            }
            spriteRenderer.GetPropertyBlock(propBlock);
            foreach (var prop in props)
            {
                Debug.LogFormat("setting property {0} to val {1}", prop.Item1, prop.Item2);
                propBlock.SetTexture(prop.Item1, prop.Item2);
            }

            spriteRenderer.SetPropertyBlock(propBlock);
        }

        public void UpdateProperties(List<Tuple<string, Color>> props, int materialIndex = 0)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
            if (propBlock == null)
            {
                propBlock = new MaterialPropertyBlock();
            }
            spriteRenderer.GetPropertyBlock(propBlock);
            foreach (var prop in props)
            {
                Debug.LogFormat("setting property {0} to val {1}", prop.Item1, prop.Item2);
                propBlock.SetColor(prop.Item1, prop.Item2);
            }

            spriteRenderer.SetPropertyBlock(propBlock);
        }
    }
}
