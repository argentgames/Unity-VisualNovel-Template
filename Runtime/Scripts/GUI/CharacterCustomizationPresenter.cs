using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using UnityEngine.EventSystems;
public class CharacterCustomizationPresenter : MonoBehaviour
{
    [SerializeField]
    TMP_InputField nameInput;
    [SerializeField]
    Button finalize;
    // Start is called before the first frame update
    PlayerControls _playerControls;
    void Awake()
    {

        // TECHDEBT manually reset mc displayname to default
        ((MC_NPC_SO) GameManager.Instance.NamedCharacterDatabase[NPC_NAME.MC]).DisplayName = "Mika";
        SetRXSubscriptions();
        _playerControls = new PlayerControls();
    }
    private void OnDisable()
    {
        _playerControls.Disable();
    }
    void OnEnable()
    {
        _playerControls.Enable();
    }

    void Start()
    {

         _playerControls.UI.Submit.performed += ctx =>
       {
           EventSystem.current.SetSelectedGameObject(null);
       };
    }

    private void SetRXSubscriptions()
    {
        MC_NPC_SO mc = (MC_NPC_SO) GameManager.Instance.NamedCharacterDatabase[NPC_NAME.MC];
        nameInput.onValueChanged.AsObservable().Subscribe(val =>
        {
            mc.DisplayName = val;
            
        }).AddTo(this);

        finalize.OnClickAsObservable().Subscribe(val => 
        {
            if (mc.DisplayName == "")
            {
                mc.DisplayName = "Mika";
            }
            // TECHDEBT: change scene level name and the transition type/duration probably
            AudioManager.Instance.StopMusic(3f);
            SceneTransitionManager.Instance.LoadScene("Ingame",2f,1f,doStopSound: false);
        }).AddTo(this);

    }
}
