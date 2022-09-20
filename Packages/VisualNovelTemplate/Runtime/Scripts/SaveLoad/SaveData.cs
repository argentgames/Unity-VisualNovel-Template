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
        /// <summary>
        /// If we save with a choice displaying, we might also be displaying some accompanying text
        /// so we should load that up too.
        /// </summary>
        public Dialogue currentDialogue;
        public string currentShot = "black";
        [OdinSerialize]

        public Vector3 currentBGCameraPosition;
        public Vector3 currentBGCameraRotation;
        public float currentBGSize;
        public List<DialogueHistoryLine> dialogueHistory = new List<DialogueHistoryLine>();
        public bool isTinted = false;
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

        }
        public void Save(string filePath,DataFormat format)
        {
            byte[] bytes = SerializationUtility.SerializeValue(this,format);
            File.WriteAllBytes(filePath, bytes);

            var ss = this.screenshot.EncodeToPNG();
            File.WriteAllBytes(filePath + ".PNG", ss);

            Debug.Log("done saving to: " + filePath);
        }
        public SaveData Load(string filePath,DataFormat format)
        {
            byte[] bytes = File.ReadAllBytes(filePath);

            var save = SerializationUtility.DeserializeValue<SaveData>(bytes, format);
            var ss = File.ReadAllBytes(filePath + ".PNG");
            Texture2D loadTexture = new Texture2D(2, 2);
            loadTexture.LoadImage(ss);
            save.screenshot = loadTexture;
            return save;
        }

    }
}
