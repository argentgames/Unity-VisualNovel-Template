using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class SettingsLogic : MonoBehaviour
{
    // Start is called before the first frame update
    SettingsPresenter settingsPresenter;
    async UniTaskVoid Awake()
    {
        // take screenshot in case we want to save file;
    }
    void Start()
    {
        settingsPresenter = GetComponent<SettingsPresenter>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
