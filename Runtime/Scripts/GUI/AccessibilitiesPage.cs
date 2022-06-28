using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using TMPro;

namespace com.argentgames.visualnoveltemplate
{
public class AccessibilitiesPage : MonoBehaviour
{
    [SerializeField]
    Toggle largeFont, mediumFont, smallFont, openDSFontToggle, screenshake, texttospeech;

    // Start is called before the first frame update
    void Awake()
    {
        if (GameManager.Instance.Settings.enableScreenShake)
        {screenshake.SetIsOnWithoutNotify(true);}
        else
        {screenshake.SetIsOnWithoutNotify(false);}

        if (GameManager.Instance.Settings.useOpenDSFont.Value)
        {openDSFontToggle.SetIsOnWithoutNotify(true);}
        else
        {openDSFontToggle.SetIsOnWithoutNotify(false);}

        if (GameManager.Instance.Settings.fontSize.Value == 1)
        {
            smallFont.SetIsOnWithoutNotify(true);
            mediumFont.SetIsOnWithoutNotify(false);
            largeFont.SetIsOnWithoutNotify(false);
        }
        else if (GameManager.Instance.Settings.fontSize.Value == 2)
        {
            smallFont.SetIsOnWithoutNotify(false);
            mediumFont.SetIsOnWithoutNotify(true);
            largeFont.SetIsOnWithoutNotify(false);
        }
        else
        {
            smallFont.SetIsOnWithoutNotify(false);
            mediumFont.SetIsOnWithoutNotify(false);
            largeFont.SetIsOnWithoutNotify(true);
        }

        SetRXSubscriptions();
    }

    void SetRXSubscriptions()
    {
        // TODO: tts and fonts not supported for now
        screenshake.onValueChanged.AsObservable().Subscribe(val =>
        {

            GameManager.Instance.Settings.enableScreenShake = val;

        }).AddTo(this);
        openDSFontToggle.onValueChanged.AsObservable().Subscribe(val =>
        {

            GameManager.Instance.Settings.useOpenDSFont.Value = val;

        }).AddTo(this);
        smallFont.onValueChanged.AsObservable().Subscribe(val =>
        {
            if (smallFont.isOn)
            {
            GameManager.Instance.Settings.fontSize.Value = 1;
            }

        }).AddTo(this);
        mediumFont.onValueChanged.AsObservable().Subscribe(val =>
        {
            if (mediumFont.isOn)
            {
            GameManager.Instance.Settings.fontSize.Value = 2;
            }

        }).AddTo(this);
        largeFont.onValueChanged.AsObservable().Subscribe(val =>
        {
            if (largeFont.isOn)
            {
            GameManager.Instance.Settings.fontSize.Value = 3;
            }

        }).AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {

    }
}

}