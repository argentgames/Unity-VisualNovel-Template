using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class CharacterSpriteController : MonoBehaviour
{
    public SpriteRenderer body;
    public SpriteRenderer head;
    public SpriteRenderer arm;
    public SpriteRenderer brow;
    public SpriteRenderer eyes;
    public SpriteRenderer mouth;
    public GameObject accessories;

    public NPC_SO npc;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateSprite(){
        // eyes = npc.paperdoll.Eyes;
    }
}
