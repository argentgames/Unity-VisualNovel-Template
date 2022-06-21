using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CGGalleryCG : MonoBehaviour
{
    // techdebt: turn locked img into a global thing
    [SerializeField]
    Sprite cg, locked;
    [SerializeField]
    string unlockCGVariable;
    Image image;
    Button button;
    public Sprite CG { get { return cg;}}
    
    // Start is called before the first frame update
    void Awake()
    {
        image = GetComponent<Image>();
        button = GetComponent<Button>();
    }
    void OnEnable()
    {
        
        if (GameManager.Instance.Settings.cgDict[unlockCGVariable])
        {
            image.sprite = cg;
            button.interactable = true;
            Debug.Log("enabling cg");

        }
        else
        {
            //image = lockedImage;
            image.sprite = locked;
            button.interactable = false;
            Debug.Log("disabling cg");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
