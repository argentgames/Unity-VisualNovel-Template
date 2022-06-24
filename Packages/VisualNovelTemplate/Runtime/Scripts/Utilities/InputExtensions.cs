using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
public class InputExtensions 
{
    public static void DeselectClickedButton(GameObject button)
    {
        if (EventSystem.current.currentSelectedGameObject == button)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
