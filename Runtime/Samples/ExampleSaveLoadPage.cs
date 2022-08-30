using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.argentgames.visualnoveltemplate;

using System;
using UnityEngine.UI;
using TMPro;
using UniRx;
using System.IO;
using Cysharp.Threading.Tasks;
namespace com.argentgames.visualnoveltemplate
{
    public class ExampleSaveLoadPage : MonoBehaviour
    {
        [SerializeField]
        GameObject saveSlotPrefab, saveSlotContentHolder, overwriteSavePanel;
        [SerializeField]
        int numSaveSlots = 9;
        int currentSelectedSaveSlot = 0;
        CompositeDisposable disposables = new CompositeDisposable();
        
        // Start is called before the first frame update
        void Start()
        {

            overwriteSavePanel.SetActive(false);

            overwriteSavePanel.GetComponent<ConfirmPanel>().no.OnClickAsObservable().Subscribe(val =>
            {
                disposables.Clear();
                overwriteSavePanel.SetActive(false);
            });
        }

        // Update is called once per frame
            void OnEnable()
            {
            for (int i = 0; i < saveSlotContentHolder.transform.childCount; i++)
            {
                Destroy(saveSlotContentHolder.transform.GetChild(i).gameObject);
            }
            // SaveLoadManager.Instance.LoadSaveFiles();
            CreateSaveSlots();
        }
        void CreateSaveSlots()
        {
            // put auto save slot at the top
            // autosave doesn't count towards the defined numSaveSlots.
            var go = Instantiate(saveSlotPrefab, saveSlotContentHolder.transform);
            var saveSlot = go.GetComponent<SaveLoadSlot>();
            go.GetComponent<Button>().interactable = false;
            string filePath = SaveLoadManager.Instance.autoSaveNamePrefix + SaveLoadManager.Instance.extension;

            // not allowed to save over autosave
            if (SaveLoadManager.Instance.saveFiles.ContainsKey(filePath))
            {
                var save = SaveLoadManager.Instance.saveFiles[filePath];
                saveSlot.emptyText.text = "";

                saveSlot.screenshot.sprite = Sprite.Create(save.screenshot,
                new Rect(0, 0, save.screenshot.width, save.screenshot.height), Vector2.zero);

                saveSlot.date.text = save.dateTime;
            }
            else
            {
                saveSlot.emptyText.text = "Autosave";
                saveSlot.date.text = "";
            }


            for (int i = 0; i < numSaveSlots; i++)
            {
                CreateSaveSlot(i);
            }
        }
        void CreateSaveSlot(int i)
        {
            var go = Instantiate(saveSlotPrefab, saveSlotContentHolder.transform);
            var saveSlot = go.GetComponent<SaveLoadSlot>();
            // slot text > screenshot > date time
            string filePath = SaveLoadManager.Instance.saveFileNamePrefix + i.ToString() + SaveLoadManager.Instance.extension;
            Debug.Log(filePath);
            var slotIndex = i;
            Debug.LogFormat("saveloadmanager contains save file {0}: {1}", filePath, SaveLoadManager.Instance.saveFiles.ContainsKey(filePath));
            // if a save file already exists, when we click it we want to ask if they want to overwrite
            if (SaveLoadManager.Instance.saveFiles.ContainsKey(filePath))
            {
                var save = SaveLoadManager.Instance.saveFiles[filePath];
                //Read
                // byte[] bytes = save.screenshot;
                //Convert image to texture
                // Texture2D loadTexture = new Texture2D(2, 2);
                // loadTexture.LoadImage(bytes);

                saveSlot.emptyText.text = "";

                saveSlot.screenshot.sprite = Sprite.Create(save.screenshot,
                new Rect(0, 0, save.screenshot.width, save.screenshot.height), Vector2.zero);

                saveSlot.date.text = save.dateTime;

                go.GetComponent<Button>().OnClickAsObservable()
                .Subscribe(_ =>
                {
                    overwriteSavePanel.SetActive(true);
                // TECHDEBT: stupid hard code get child
                overwriteSavePanel.GetComponent<ConfirmPanel>().yes.OnClickAsObservable()
            .Subscribe(_ =>
            {
                        currentSelectedSaveSlot = slotIndex;
                        saveSlot.RunSave();
                        overwriteSavePanel.SetActive(false);

                    }).AddTo(disposables);

                }
                )
                .AddTo(this);
            }
            else
            {
                saveSlot.emptyText.text = "Empty Slot";
                saveSlot.date.text = "";
                go.GetComponent<Button>().OnClickAsObservable()
                .Subscribe(_ =>
                {
                    currentSelectedSaveSlot = slotIndex;
                    saveSlot.RunSave();
                    overwriteSavePanel.SetActive(false);
                }).AddTo(this);
            }
        }
    }
}
