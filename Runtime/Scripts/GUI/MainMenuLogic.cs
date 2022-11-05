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
        public float delayBeforeFadeOut = 1f;
        public bool stopSoundOnLoad = true;
        [SerializeField]
        [Tooltip("What audio do we play when loading into the main menu scene?")]
        string mainMenuAudio = "MainMenu";
        [SerializeField]
        float mainMenuAudioFadeInDuration = .5f;
        [SerializeField]
        string mainMenuSettingsName = "mmSettings";

        void Awake()
        {
            gameObject.SetActive(true);
            if (mainMenuAudio != "")
            {
                AudioManager.Instance.PlayMusic(mainMenuAudio, mainMenuAudioFadeInDuration);
            }
            
        }

        public void StartNewGame()
        {
            // TECHDEBT
            // if someone finished the game and in the same session they start a new game,
            // we dont want the ingamebrain to try to load up old save
            SaveLoadManager.Instance.currentSave = new SaveData();
            SceneTransitionManager.Instance.LoadScene(newGameLoadScene, fadeToBlackDuration, fadeOutOfBlackDuration, doStopSound:stopSoundOnLoad, delayBeforeFadeIn: delayBeforeFadeOut);
            DialogueSystemManager.Instance.RestartGame();

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
            await MenuManager.Instance.OpenPage(mainMenuSettingsName,menu);
        }

    }


}