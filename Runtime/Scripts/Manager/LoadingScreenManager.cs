using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Show a loading screen. Most common usage is loading in a Unity Scene,
/// but maybe you want to show a loading screen within a Unity Scene because
/// someone just purchased something?
/// </summary>
namespace com.argentgames.visualnoveltemplate
{
    public class LoadingScreenManager : MonoBehaviour
    {
        public static LoadingScreenManager Instance {get ; set ;}

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
