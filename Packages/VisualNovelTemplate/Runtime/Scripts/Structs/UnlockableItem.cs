using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
namespace com.argentgames.visualnoveltemplate
{
    public struct UnlockableItem
    {
        public string internalName;
        public string displayName;
        public BoolReactiveProperty unlocked;

        public bool defaultState;
        public string defaultDisplayName;

        public void ResetDefaults()
        {
            unlocked.Value = defaultState;
            displayName = defaultDisplayName;
        }
        public void ToggleUnlockState()
        {
            unlocked.Value = !unlocked.Value;
        }
        public void Unlock()
        {
            unlocked.Value = true;
        }
        public void Lock()
        {
            unlocked.Value = false;
        }
        public void SetUnlockState(bool value)
        {
            unlocked.Value = value;
        }

        public UnlockableItemSaveData Save()
        {
            return new UnlockableItemSaveData(this.internalName,this.unlocked.Value);
        }
        public void Load(bool unlockedState)
        {
            unlocked.Value = unlockedState;
        }
    }
}
