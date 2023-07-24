using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.IO;
using Cysharp.Threading.Tasks;
using System.Threading;
using UniRx;
using Sirenix.Serialization;
using UnityEngine.SceneManagement;

namespace com.argentgames.visualnoveltemplate
{
    public class SaveData
    {
        public string inkData;
        [NonSerialized]
        public Texture2D screenshot;
        public string dateTime;
        public Dictionary<string, SpriteSaveData> spriteSaveDatas = new Dictionary<string, SpriteSaveData>();
        public string currentMusic = "";
        public List<Tuple<string, int>> currentAmbients = new List<Tuple<string, int>>();
        [NonSerialized]
        public Dialogue? currentDialogue = null;
        /// <summary>
        /// If we save with a choice displaying, we might also be displaying some accompanying text
        /// so we should load that up too.
        /// </summary>
        public DialogueSaveData currentDialogueSaveData;
        public string currentShot = "black";
        [OdinSerialize]

        public Vector3 currentBGCameraPosition;
        public Vector3 currentBGCameraRotation;
        public float currentBGSize;
        public List<DialogueHistoryLine> dialogueHistory = new List<DialogueHistoryLine>();
        public bool isTinted = false;
        public List<string> currentVisibleDialogueUIs = new List<string>();
        public string currentDialogueWindowMode = "";
        public SaveData(string saveData, Texture2D texture, string dateTime)
        {
            this.inkData = saveData;
            //Convert to png
            this.screenshot = texture;
            this.dateTime = dateTime;
        }
        public SaveData()
        {
            currentDialogue = null;
        }
        public void Save(string filePath,DataFormat format)
        {
            var dialogue = (Dialogue)DialogueSystemManager.Instance.CurrentProcessedDialogue;
            currentDialogueSaveData = new DialogueSaveData(dialogue.expression,
            dialogue.speaker,
            dialogue.text,
            dialogue.npc.internalName,
            dialogue.duration);

            currentVisibleDialogueUIs.Clear();
            foreach (var win in DialogueSystemManager.Instance.VisibleUIWindows)
            {
                currentVisibleDialogueUIs.Add(win);
            }

            byte[] bytes = SerializationUtility.SerializeValue(this,format);
            File.WriteAllBytes(filePath, bytes);

            var ss = this.screenshot.EncodeToPNG();
            File.WriteAllBytes(filePath + ".PNG", ss);

            Debug.Log("done saving to: " + filePath);
        }
        public void Load(string filePath,DataFormat format)
        {
            byte[] bytes = File.ReadAllBytes(filePath);

            var save = SerializationUtility.DeserializeValue<SaveData>(bytes, format);

            var npcName = save.currentDialogueSaveData.npcName == null ||
            save.currentDialogueSaveData.npcName == "" ? "narrator" : save.currentDialogueSaveData.npcName;
            this.currentDialogue = new Dialogue(save.currentDialogueSaveData.expression,
            save.currentDialogueSaveData.speaker,
            save.currentDialogueSaveData.text,
            GameManager.Instance.GetNPC(npcName),
            save.currentDialogueSaveData.duration
            );

            this.inkData = save.inkData;
            this.dateTime = save.dateTime;
            this.spriteSaveDatas = save.spriteSaveDatas;
            this.currentMusic = save.currentMusic;
            this.currentAmbients = save.currentAmbients;
            this.currentDialogueSaveData = save.currentDialogueSaveData;
            this.currentShot = save.currentShot;
            this.currentBGCameraPosition = save.currentBGCameraPosition;
            this.currentBGCameraRotation = save.currentBGCameraRotation;
            this.currentBGSize = save.currentBGSize;
            this.dialogueHistory = save.dialogueHistory;
            this.isTinted = save.isTinted;
            this.currentDialogueWindowMode = save.currentDialogueWindowMode;
            if (save.currentVisibleDialogueUIs != null)
            {
                this.currentVisibleDialogueUIs = save.currentVisibleDialogueUIs;
            }
            else
            {
                this.currentVisibleDialogueUIs = new List<string>();
            }
            

            var s = "";
            foreach (var win in currentVisibleDialogueUIs)
            {
                s += " " + win + " ";
            }
            Debug.Log(s);
            

            var ss = File.ReadAllBytes(filePath + ".PNG");
            Texture2D loadTexture = new Texture2D(2, 2);
            loadTexture.LoadImage(ss);
            this.screenshot = loadTexture;
            // return save;
        }

    }
}
