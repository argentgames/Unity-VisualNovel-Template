using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.argentgames.visualnoveltemplate
{
    public class Preload : MonoBehaviour
    {
        public string sceneToLoad = "MainMenu";
        // Start is called before the first frame update
        void Start()
        {
            SceneTransitionManager.Instance.LoadScene(sceneToLoad,UnityEngine.SceneManagement.LoadSceneMode.Single);
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
