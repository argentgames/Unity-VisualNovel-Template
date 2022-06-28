using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public abstract class PortraitPresenter : MonoBehaviour
{
    [SerializeField] 
    [SceneObjectsOnly]
    [PropertyTooltip("The parent container that holds the portraits. Used to show/hide the entire portrait object.")]
    GameObject portraitHolder;

    /// <summary>
    /// Show the portrait character.
    /// </summary>
    /// <param name="npcName"></param>
    public abstract void ShowChar(string npcName);
    /// <summary>
    /// Hide the current visible portrait character but not necessarily any background.
    /// </summary>
    public virtual void HideChar(){}

    /// <summary>
    /// Hide the entire portrait object, usually consisting of both the background and the character.
    /// </summary>
    public virtual void HidePortrait()
    {
        portraitHolder.SetActive(false);
    }
    /// <summary>
    /// Show the entire portrait object. Usually you want to call ShowChar before ShowPortrait so that
    /// the player doesn't see an empty background before a character pops in, assuming there is a background.
    /// </summary>
    public virtual void ShowPortrait()
    {
        if (!portraitHolder.activeSelf)
        {
            portraitHolder.SetActive(true);
        }

    }
    
    public virtual bool IsShowingPortrait { get { return portraitHolder.activeSelf; } }
}
