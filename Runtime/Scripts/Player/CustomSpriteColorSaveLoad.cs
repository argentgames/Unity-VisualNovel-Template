using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;
using AnimeTask;
using ElRaccoone.Tweens;
using UnityEngine.Events;
using System.Threading.Tasks;
using System.Threading;
namespace com.argentgames.visualnoveltemplate
{
    public abstract class CustomSpriteColorSaveLoad : MonoBehaviour
    {
        [SerializeField]
        public string inkVariable;
        public string InkVariable => inkVariable;
        public SpriteRenderer image;
        public MaterialPropertyBlock _propBlock,_propBlockDst;
        [SerializeField]
        public List<SpriteRenderer> dependentShadowLayers = new List<SpriteRenderer>();
        [SerializeField]
        public Texture multiplyBaseHack;
        public virtual void SetCustomColorizationPropertyFlags()
        {
            _propBlock = new MaterialPropertyBlock();
            _propBlockDst = new MaterialPropertyBlock();
            Debug.LogFormat("settting customc colorization prop flags for {0}",this.gameObject.name);
            // set doColorization to true
            image.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat("_DoColorization", 1);
            image.SetPropertyBlock(_propBlock);
            if (multiplyBaseHack != null)
            {
                foreach (var sr in dependentShadowLayers)
                {
                    sr.GetPropertyBlock(_propBlock);
                _propBlock.SetTexture("_SceneColorHack",multiplyBaseHack);
                sr.SetPropertyBlock(_propBlock);
                                Debug.LogFormat("set scene color hack multiply for {0}",sr.gameObject.name);

                }
                image.GetPropertyBlock(_propBlock);
                _propBlock.SetTexture("_SceneColorHack",multiplyBaseHack);
                image.SetPropertyBlock(_propBlock);
                                Debug.LogFormat("set scene color hack multiply for {0}",image.gameObject.name);

            }
        } 
    }
}