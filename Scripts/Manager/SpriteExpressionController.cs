// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using Cysharp.Threading.Tasks;
// using Sirenix.OdinInspector;
// public class SpriteExpressionController : MonoBehaviour
// {
//     GameObject NewTexWrapper, currExpWrapper;
//     [Range(0,1)]
//     public float dissolvAmount;
//     List<SpriteRenderer> newMats = new List<SpriteRenderer>();
//     List<SpriteRenderer> oldMats = new List<SpriteRenderer>();
//     [SerializeField]
//     RawImage test;
//     int width=0,height=0;
//     void Start()
//     {
//      NewTexWrapper = gameObject.transform.GetChild(0).gameObject;

//         foreach (var sr in NewTexWrapper.GetComponentsInChildren<SpriteRenderer>())
//         {
//             sr.material = Instantiate<Material>(sr.material);
//             sr.material.SetTexture("NewTex",sr.material.GetTexture("_MainTex"));
//             // newMats.Add(sr);
//             // if (sr.sprite.rect.width > width)
//             // {
//             //     width = (int)sr.sprite.rect.width;
//             // }
//             // if (sr.sprite.rect.height > height)
//             // {
//             //     height = (int)sr.sprite.rect.height;
//             // }
//             // sr.color = new Color(255,255,255,0);
//         }
//         // foreach (var sr in currExpWrapper.GetComponentsInChildren<SpriteRenderer>())
//         // {
//         //     oldMats.Add(sr);
//         //     // sr.color = new Color(255,255,255,255);
//         // }
//         // Texture2D newTex = new Texture2D(width,height,TextureFormat.ARGB32, false);
//         // int topLeftX,topLeftY;

//         // newMats[0].sprite.bounds.

//         // foreach (var sr in newMats)
//         // {
//         //     for (int x=0; x < sr.sprite.rect.width; x++)
//         //     {
//         //         for (int y=0; y < sr.sprite.rect.height;y++)
//         //         {
//         //             Debug.LogFormat("{0}x,{1}y",x,y);
//         //             newTex.SetPixel(x,y,sr.sprite.texture.GetPixel(x,y));
//         //         }
//         //     }
//         // }
//         // newTex.Apply();
//         // test.texture = newTex;

//         //              for(int x = 0; x < tex1.width; x++)
//         //      {
//         //            for(int y = 0; y < tex1.height; y++)
//         //            {
//         //                 mainTex.SetPixel(x,y,tex1.GetPixel(x,y));
//         //            }
//         //      }
//         //      for(int x = 0; x < tex2.width; x++)
//         //      {
//         //            for(int y = 0; y < tex2.height; y++)
//         //            {
//         //                // Add the height or width of the first image to either the x or y, in this case y because the texture will be on top.
//         //                 mainTex.SetPixel(x,y + tex1.height,tex1.GetPixel(x,y));
//         //            }
//         //      }
//         //      mainTex.Apply();

//     }



//     // Update is called once per frame
//     void Update()
//     {
//         // for (int i=0; i < newMats.Count; i++)
//         // {
//         //     newMats[i].material.SetFloat("_TransitionAmount",dissolvAmount);
//         // }
//         // for (int i=0; i < oldMats.Count; i++)
//         // {
//         //     oldMats[i].material.SetFloat("_TransitionAmount",1-dissolvAmount);
//         // }
        
//     }
// }
