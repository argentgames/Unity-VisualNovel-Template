using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
namespace com.argentgames.visualnoveltemplate
{
    [RequireComponent(typeof(TMP_InputField))]
    public class InputFieldExtension : MonoBehaviour
    {
        [SerializeField]
        string defaultPlaceholder = "";
        [SerializeField]
        bool resetPlaceholderOnDeselect = true;
        [SerializeField]
        bool hidePlaceholderOnSelect = true;

        TMP_Text placeholder, inputText;

        TMP_InputField inputField;

        void Awake()
        {
            inputField = GetComponentInChildren<TMP_InputField>();

            placeholder = transform.Find("Text Area/Placeholder").GetComponentInChildren<TMP_Text>();
            inputText = transform.Find("Text Area/Text").GetComponentInChildren<TMP_Text>();

            inputField.onSelect.AddListener(value =>
            {
                if (hidePlaceholderOnSelect)
                {
                    placeholder.gameObject.SetActive(false);
                }
            });
            inputField.onDeselect.AddListener(value =>
            {
                if (resetPlaceholderOnDeselect)
                {
                    placeholder.gameObject.SetActive(true);
                    placeholder.text = defaultPlaceholder;
                }
            });
        }

    }
}
