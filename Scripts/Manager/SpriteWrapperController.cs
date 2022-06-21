using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class SpriteWrapperController : SerializedMonoBehaviour
{
    [SerializeField]
    SpriteNPC_SO npc;

    public string currentExpression
    {
        get
        {
                var s = "";
                foreach (var sr in transform.GetComponentsInChildren<SpriteRenderer>())
                {
                    s += " " + sr.sprite.name;
                }
                return s;
            
        }
    }

    void Awake()
    {
        npc.SetInitialExpression(this.gameObject,"");
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // public void Save()
    // {
    //     var s = new SpriteSaveData();
    //     s.expressionImageName = currentExpression;
    //     s.position = transform.position;
    //     SaveLoadManager.Instance.currentSave.spriteSaveDatas.Add(npc.internalName,s);
    // }
    public SpriteSaveData Save()
    {
        var s = new SpriteSaveData();
        s.expressionImageName = npc.CurrentExpression;

        // this is dumb but hardcoding 
        // TECHDEBT:
        if (DialogueSystem.Instance != null)
        {
            if ((bool)DialogueSystem.Instance.Story.variablesState["sidepanel"])
            {
                s.position = this.transform.parent.parent.position;
            }
            else
            {
                s.position = transform.position;
            }
        }
        else
        {
            s.position = transform.position;
        }
        
        Debug.Log("expression: " + npc.CurrentExpression);
        return s;
    }
    public void Load()
    {

    }
}
