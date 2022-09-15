using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace com.argentgames.visualnoveltemplate
{
    public abstract class SaveLoadSlot : MonoBehaviour
    {
        /// <summary>
        /// EmptyText used for saying the saveslot is empty...
        /// </summary>
        public TMP_Text emptyText, date;
        public Image screenshot;
        public int saveIndex = 0;
        public SaveData saveData;
        public Button button;
        // Start is called before the first frame update
        void Awake()
        {
            emptyText.text = "Empty Slot";
            date.text = "";
        }

        /// <summary>
        /// If a save exists, load up any displayable data so the player knows what save will be loaded
        /// if they select this slot!
        /// </summary>
        /// <param name="saveData"></param>
        public virtual void SetExistingLoadData()
        {
            if (saveData == null)
            {
                Debug.LogError("why is our saveData empty when we are tryin gto set existing load data???");
                return;
            }
            if (screenshot != null && saveData.screenshot != null)
            {
                Debug.Log(screenshot);
                Debug.Log(saveData.screenshot);
                screenshot.sprite = Sprite.Create(saveData.screenshot,
                new Rect(0, 0, saveData.screenshot.width, saveData.screenshot.height), Vector2.zero);
            }

            if (date != null)
            {
                date.text = saveData.dateTime;
            }

            if (emptyText != null)
            {
                emptyText.text = "";
            }


        }
        /// <summary>
        /// When the player clicks on this slot, we will save all the data.
        /// </summary>
        public abstract void RunSave();
        /// <summary>
        /// When the player clicks on this slot, we load all the data in.
        /// We then probably also want to reload the Ingame scene and run the game from 
        /// our existing save data.
        /// </summary>
        public abstract void RunLoad();



    }

}
