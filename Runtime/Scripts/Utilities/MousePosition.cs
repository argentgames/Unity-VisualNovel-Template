using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace com.argentgames.visualnoveltemplate
{
    public class MousePosition: MonoBehaviour
    {
            public static MousePosition Instance;
    // Start is called before the first frame update
    PlayerControls _defaultControls;
    void Awake()
    {
        _defaultControls = new PlayerControls();

        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
        }
        Debug.Log("name of mouseposition gameobje: " + this.gameObject.name);
            Instance = this;
        
    }
    public Vector2 Position
    {
        get
        {
            if (Instance == null)
            {
                return new Vector2();
            }
            else
            {
                return Instance._defaultControls.UI.MousePosition.ReadValue<Vector2>();
            }
        }

    }

    private void OnEnable() 
    {
        _defaultControls.Enable();
    }
    private void OnDisable()
    {
        _defaultControls.Disable();
    }
    }


}
