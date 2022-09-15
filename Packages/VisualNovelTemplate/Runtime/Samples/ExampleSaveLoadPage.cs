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
        GameObject saveSlotPrefab, saveSlotContentHolder, autoSaveSlotPrefab;
        [SerializeField]
        int numSaveSlots = 9;
        [SerializeField]
        bool isSavePage, isLoadPage, isSaveLoadPage;

        // Start is called before the first frame update
        void Awake()
        {
            CreateSaveSlots();
        }
        void OnEnable()
        {
            if (isLoadPage)
            {
                // if we're a Load page, then we may need to refresh our data if player just made some saves and then switched over!
            Utilities.DestroyAllChildGameObjects(saveSlotContentHolder);
            CreateSaveSlots();
            }
            
        }

        void CreateSaveSlots()
        {
            Debug.Log("how many times is create save slots called");
            if (GameManager.Instance.DefaultConfig.autosave)
            {
                // create autosave slot at top

                // put auto save slot at the top
                // autosave doesn't count towards the defined numSaveSlots.
                // var go = Instantiate(saveSlotPrefab, saveSlotContentHolder.transform);
                // var saveSlot = go.GetComponent<SaveLoadSlot>();
                // go.GetComponent<Button>().interactable = false;
                // string filePath = SaveLoadManager.Instance.autoSaveNamePrefix + SaveLoadManager.Instance.extension;

                // // not allowed to save over autosave
                // if (SaveLoadManager.Instance.saveFiles.ContainsKey(filePath))
                // {
                //     var save = SaveLoadManager.Instance.saveFiles[filePath];
                //     saveSlot.emptyText.text = "";

                //     saveSlot.screenshot.sprite = Sprite.Create(save.screenshot,
                //     new Rect(0, 0, save.screenshot.width, save.screenshot.height), Vector2.zero);

                //     saveSlot.date.text = save.dateTime;
                // }
                // else
                // {
                //     saveSlot.emptyText.text = "Autosave";
                //     saveSlot.date.text = "";
                // }

            }


            for (int i = 0; i < numSaveSlots; i++)
            {
                CreateSaveSlot(i);
            }
        }
        async void CreateSaveSlot(int i)
        {
            var go = Instantiate(saveSlotPrefab, saveSlotContentHolder.transform);
            var saveSlot = go.GetComponent<SaveLoadSlot>();
             var btn = saveSlot.button;
            // slot text > screenshot > date time
            string filePath = SaveLoadManager.Instance.saveFileNamePrefix + i.ToString() + SaveLoadManager.Instance.extension;
            Debug.Log(filePath);
            Debug.LogFormat("saveloadmanager contains save file {0}: {1}", filePath, SaveLoadManager.Instance.SaveExists(i.ToString()));
            // if a save file already exists, when we click it we want to ask if they want to overwrite
            if (SaveLoadManager.Instance.SaveExists(i.ToString()))
            {
                var save = SaveLoadManager.Instance.GetSaveData(i.ToString());
                saveSlot.saveData = save;
                saveSlot.SetExistingLoadData();
                Debug.LogFormat("setting existing saveslot data: {0} {1}", saveSlot.date, saveSlot.saveIndex);


            }
            else
            {
                if (isLoadPage)
                {
                    btn.interactable = false;
                }
            }
            
            saveSlot.saveIndex = i;
            

           
            if (isSavePage)
            {
                btn.OnClickAsObservable().Subscribe(val =>
                {
                    saveSlot.RunSave();
                }).AddTo(this);
            }
            else if (isLoadPage)
            {
                btn.OnClickAsObservable().Subscribe(val =>
                                {
                                    saveSlot.RunLoad();
                                }).AddTo(this);
            }
            else
            {
                Debug.LogWarning("Generic Save/Load combo page not implemented!");
            }
        }
    }
}
