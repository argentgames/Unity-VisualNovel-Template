using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace com.argentgames.visualnoveltemplate
{
    public abstract class SaveLoadSlot : MonoBehaviour
    {
        public TMP_Text emptyText, date;
        public Image screenshot;
        public int saveIndex = 0;
        // Start is called before the first frame update
        void Awake()
        {
            emptyText.text = "Empty Slot";
            date.text = "";
        }

        public abstract void RunSave();
        public abstract void RunLoad(SaveData saveData);


    }

}
