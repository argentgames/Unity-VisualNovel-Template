using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using UniRx;
using UnityEngine.SceneManagement;
// using UnityEngine.UI.Extensions;
using Cysharp.Threading.Tasks;


namespace com.argentgames.visualnoveltemplate
{


    public class MainMenuLogic : MonoBehaviour
    {

        public string newGameLoadScene = "CharacterCustomization";
        public float fadeToBlackDuration = 2.5f;
        public float fadeOutOfBlackDuration = 1.5f;
        public bool stopSoundOnLoad = true;

        void Awake()
        {
            gameObject.SetActive(true);
            AudioManager.Instance.PlayMusic("mm", .5f);
        }

        public void StartNewGame()
        {
            // TECHDEBT
            // if someone finished the game and in the same session they start a new game,
            // we dont want the ingamebrain to try to load up old save
            SaveLoadManager.Instance.currentSave = null;
            SceneTransitionManager.Instance.LoadScene(newGameLoadScene, fadeToBlackDuration, fadeOutOfBlackDuration, stopSoundOnLoad);

        }
        public void QuitGame()
        {
            // this should probably be in a static utils function
            // and have some other calls that clean up garbage
            // and dispose of everything properly to prevent memory
            // leaks
            Application.Quit();
        }

        public async UniTask OpenMenu(string menu="")
        {
            await MenuManager.Instance.OpenPage("mainMenuSettings",menu);
        }

    }


}