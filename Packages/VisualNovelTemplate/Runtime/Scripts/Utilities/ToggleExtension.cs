using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Sirenix.OdinInspector;
using TMPro;
[RequireComponent(typeof(Toggle))]
public class ToggleExtension : MonoBehaviour
{
    [ShowIf("doSpriteSwapWhenSelected")]
    [SerializeField]
    Image toggleTargetGraphic;
    [ShowIf("doSpriteSwapWhenSelected")]
    [SerializeField]
    Sprite defaultSprite, selectedSprite;
    [SerializeField]
    bool doSpriteSwapWhenSelected = false, doTextSwapWhenSelected = false, doTextColorChangeWhenSelected = false, doImageColorChangeWhenSelected = false;
    [ShowIf("doTextSwapWhenSelected")]
    [SerializeField]
    string defaultText = "OFF", selectedText = "ON";
    [ShowIf("doTextColorChangeWhenSelected")]
    [SerializeField]
    Color defaultColor = Color.white, selectedColor = Color.yellow;
    [ShowIf("doImageColorChangeWhenSelected")]
    [SerializeField]
    Color graphicDefaultColor = Color.white, graphicSelectedColor = Color.blue;
    [SerializeField]
    TMP_Text textField;
    Toggle toggle;

    // Start is called before the first frame update
    void Start()
    {
        toggle = GetComponent<Toggle>();
        SetRXSubscriptions();
        if (doSpriteSwapWhenSelected)
        {
            SpriteSwapOnSelect(toggle.isOn);
        }
        if (doTextSwapWhenSelected)
        {
            TextSwapOnSelect(toggle.isOn);
        }
        if (doTextColorChangeWhenSelected)
        {
            TextColorSwapOnSelect(toggle.isOn);
        }
        if (doImageColorChangeWhenSelected)
        {
            ImageColorSwapOnSelect(toggle.isOn);
        }

    }

    void SetRXSubscriptions()
    {
        toggle.onValueChanged.AsObservable().Subscribe(val =>
        {
            if (doSpriteSwapWhenSelected)
            {
                SpriteSwapOnSelect(val);
            }

            if (doTextSwapWhenSelected)
            {
                TextSwapOnSelect(val);
            }
            if (doTextColorChangeWhenSelected)
            {
                TextColorSwapOnSelect(val);
            }
            if (doImageColorChangeWhenSelected)
            {
                ImageColorSwapOnSelect(val);
            }
            InputExtensions.DeselectClickedButton(toggle.gameObject);
        }).AddTo(this);


    }

    public void ImageColorSwapOnSelect(bool val)
    {
        if (val)
        {
            toggle.image.color = graphicSelectedColor;
        }
        else
        {
            toggle.image.color = graphicDefaultColor;
        }
    }
    public void SpriteSwapOnSelect(bool val)
    {
        if (val)
        {
            toggleTargetGraphic.sprite = selectedSprite;
        }
        else
        {
            toggleTargetGraphic.sprite = defaultSprite;
        }
    }
    public void TextSwapOnSelect(bool val)
    {
        if (val)
        {
            textField.text = selectedText;
        }
        else
        {
            textField.text = defaultText;
        }
    }
    public void TextColorSwapOnSelect(bool val)
    {
        if (val)
        {
            textField.color = selectedColor;
        }
        else
        {
            textField.color = defaultColor;
        }
    }
}
