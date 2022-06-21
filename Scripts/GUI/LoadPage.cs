using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
public class LoadPage : MonoBehaviour
{
    [SerializeField]
    GameObject saveSlotPrefab, saveSlotContentHolder, loadSavePanel;
    [SerializeField]
    int numSaveSlots = 9;
    IDisposable disposable;
    // Start is called before the first frame update
    void Start()
    {
        loadSavePanel.SetActive(false);
        loadSavePanel.GetComponent<ConfirmPanel>().no.OnClickAsObservable().Subscribe(val =>
        {
            disposable.Dispose();
            loadSavePanel.SetActive(false);
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

    void CreateSaveSlot(int i)
    {
        var go = Instantiate(saveSlotPrefab, saveSlotContentHolder.transform);
        var saveSlot = go.GetComponent<SaveSlot>();
        // slot text > screenshot > date time
        string filePath = SaveLoadManager.Instance.saveFileNamePrefix + i.ToString() + SaveLoadManager.Instance.extension;

        // if a save file already exists, when we click it we want to ask if they want to overwrite
        if (SaveLoadManager.Instance.saveFiles.ContainsKey(filePath))
        {
            var save = SaveLoadManager.Instance.saveFiles[filePath];
            //Read
            // byte[] bytes = save.screenshot;
            // //Convert image to texture
            // Texture2D loadTexture = new Texture2D(2, 2);
            // loadTexture.LoadImage(bytes);
            saveSlot.emptyText.text = "";

            saveSlot.screenshot.sprite = Sprite.Create(save.screenshot,
            new Rect(0, 0, save.screenshot.width, save.screenshot.height), Vector2.zero);

            saveSlot.date.text = save.dateTime;

            go.GetComponent<Button>().OnClickAsObservable()
            .Subscribe(_ =>
            {
                RunLoadSlot(filePath);

            }
            )
            .AddTo(this);
        }
    }

    void CreateSaveSlots()
    {
        // put auto save slot at top
        var go = Instantiate(saveSlotPrefab, saveSlotContentHolder.transform);
        var saveSlot = go.GetComponent<SaveSlot>();
                string filePath = SaveLoadManager.Instance.autoSaveNamePrefix + SaveLoadManager.Instance.extension;
if (SaveLoadManager.Instance.saveFiles.ContainsKey(filePath))
        {
            var save = SaveLoadManager.Instance.saveFiles[filePath];
            //Read
            // byte[] bytes = save.screenshot;
            // //Convert image to texture
            // Texture2D loadTexture = new Texture2D(2, 2);
            // loadTexture.LoadImage(bytes);
            saveSlot.emptyText.text = "";

            saveSlot.screenshot.sprite = Sprite.Create(save.screenshot,
            new Rect(0, 0, save.screenshot.width, save.screenshot.height), Vector2.zero);

            saveSlot.date.text = save.dateTime;

            go.GetComponent<Button>().OnClickAsObservable()
            .Subscribe(_ =>
            {
                RunLoadSlot(filePath);

            }
            )
            .AddTo(this);
        }
        else
        {
            saveSlot.emptyText.text = "No autosave yet";
            saveSlot.date.text = "";
        }





        for (int i = 0; i < numSaveSlots; i++)
        {
            CreateSaveSlot(i);
        }
    }


    async UniTaskVoid RunLoadSlot(string filePath)
    {
        if (!(SceneManager.GetActiveScene().name == "MainMenu"))
        {
            loadSavePanel.SetActive(true);
            disposable = loadSavePanel.GetComponent<ConfirmPanel>().yes.OnClickAsObservable()
            .Subscribe(_ =>
                {
                    ActuallyRunLoadSlot(filePath).Forget();


                }).AddTo(this);
        }
        else
        {
            ActuallyRunLoadSlot(filePath).Forget();

        }

    }

    async UniTaskVoid ActuallyRunLoadSlot(string filePath)
    {
        AudioManager.Instance.StopMusic(1);
        AudioManager.Instance.StopAllAmbient(1);
        await SceneTransitionManager.Instance.FadeToBlack(2f);
        SaveLoadManager.Instance.LoadGame(filePath);
    }
}
