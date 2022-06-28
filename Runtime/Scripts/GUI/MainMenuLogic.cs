using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using UniRx;
using UnityEngine.SceneManagement;
// using UnityEngine.UI.Extensions;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace com.argentgames.visualnoveltemplate
{


public class MainMenuLogic : MonoBehaviour
{

    public ExtrasPresenter extrasPresenter;
    void Awake()
    {
        
        var canvasGroup = GetComponentInChildren<CanvasGroup>();
        // canvasGroup.alpha = 0;
        gameObject.SetActive(true);
        AudioManager.Instance.PlayMusic("mm",.5f);
        // DOTween.To(() => canvasGroup.alpha, 
        // x => canvasGroup.alpha = x, 1f, 1.5f).From(0f); 
    }
    
    public void StartNewGame()
    {
        // TECHDEBT
        // if someone finished the game and in the same session they start a new game,
        // we dont want the ingamebrain to try to load up old save
        SaveLoadManager.Instance.currentSave = null;
        SceneTransitionManager.Instance.LoadScene("CharacterCustomization",2.5f,1.5f,doStopSound: false);

    }
    public async UniTaskVoid LoadGame()
    {
        // await MenuManager.Instance.OpenMainMenuSaveLoad();
    }
    public void QuitGame()
    {
        // this should probably be in a static utils function
        // and have some other calls that clean up garbage
        // and dispose of everything properly to prevent memory
        // leaks
        Application.Quit();
    }
    public async UniTask ShowSettings(SettingsPage page)
    {
        Debug.Log("open da settings?");
        await MenuManager.Instance.OpenPage(page,SettingsType.MAINMENU);
        Debug.Log("open settings page");
        
        
    }
    public void ShowExtras(ExtrasPage page)
    {
        extrasPresenter.OpenPage(page);
        extrasPresenter.SetNavHeader(page);
    }
    
}


}