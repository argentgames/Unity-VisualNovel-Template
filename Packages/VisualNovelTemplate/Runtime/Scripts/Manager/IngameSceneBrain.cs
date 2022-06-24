using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using Ink.Runtime;
using System.Threading;
public class IngameSceneBrain : MonoBehaviour
{
    // Start is called before the first frame update
    public DialogueUIManager dialogueUIManager;
    Story story;

    public bool IsDoneSettingScene = false;

    async UniTaskVoid Start()
    {
        // AssetRefLoader.Instance.LoadCharacters();
        // dialogueUIManager.ingameHUDPresenter.ToggleMenuOpen(false);
        dialogueUIManager.HideUI();
        var customInkFunctions = GameObject.FindObjectOfType<CustomInkFunctions>();
        await UniTask.WaitUntil(() => customInkFunctions.registeredFunctions);
        DialogueSystem.Instance.Story.ResetState();
        story = DialogueSystem.Instance.Story;
        // await UniTask.Delay(TimeSpan.FromSeconds(1));

        if (SaveLoadManager.Instance.currentSave != null)
        {
            Debug.Log("setting up save data like images...");
            var currentSave = SaveLoadManager.Instance.currentSave;
            DialogueSystem.Instance.Story.state.LoadJson(SaveLoadManager.Instance.currentSave.inkData);
            DialogueSystem.Instance.DialogueHistory = SaveLoadManager.Instance.currentSave.dialogueHistory;
            ImageManager.Instance.SetTint(currentSave.isTinted);
            List<UniTask> tasks = new List<UniTask>();
            tasks.Add(ImageManager.Instance.ShowBG(currentSave.currentShot,duration:0));
            ImageManager.Instance.MoveCam("position", currentSave.currentBGCameraPosition, 0);
            ImageManager.Instance.MoveCam("rotation", currentSave.currentBGCameraRotation, 0);
            ImageManager.Instance.PlayBGTween();
             

            // do we need to show any characters?
            if (currentSave.spriteSaveDatas.Count > 0)
            {
                Debug.LogFormat("Number of saved sprites to respawn: {0}", currentSave.spriteSaveDatas.Count);
                ImageManager.Instance.ClearCharactersOnScreen();
                await UniTask.Yield();
                
                foreach (var kvPair in currentSave.spriteSaveDatas)
                {
                     tasks.Add(ImageManager.Instance.SpawnCharFromSave(kvPair.Key, kvPair.Value));
                }

                
            }
            // do we need to play any music?
            if (currentSave.currentMusic != "" && currentSave.currentMusic != null)
            {
                AudioManager.Instance.PlayMusic(currentSave.currentMusic);
            }
            if (currentSave.currentAmbient1 != "" && currentSave.currentAmbient1 != null)
            {
                AudioManager.Instance.PlayAmbient(currentSave.currentAmbient1, 0, .3f);
            }
            if (currentSave.currentAmbient2 != "" && currentSave.currentAmbient2 != null)
            {
                AudioManager.Instance.PlayAmbient(currentSave.currentAmbient2, 1, .3f);
            }
            if (currentSave.currentAmbient3 != "" && currentSave.currentAmbient3 != null)
            {
                AudioManager.Instance.PlayAmbient(currentSave.currentAmbient3, 2, .3f);
            }

            MC_NPC_SO mc = (MC_NPC_SO)GameManager.Instance.NamedCharacterDatabase[NPC_NAME.MC];
            mc.DisplayName =  (string)DialogueSystem.Instance.Story.variablesState["mc_name"];
            await UniTask.WhenAll(tasks);
            ImageManager.Instance.SetAllCharactersOnScreenActive();
        }

        else
        {

            MC_NPC_SO mc = (MC_NPC_SO)GameManager.Instance.NamedCharacterDatabase[NPC_NAME.MC];
            if (mc.DisplayName != "")
            {
                 DialogueSystem.Instance.Story.variablesState["mc_name"] = mc.DisplayName;
            }
           

        }

        
        IsDoneSettingScene = true;
    }



}
