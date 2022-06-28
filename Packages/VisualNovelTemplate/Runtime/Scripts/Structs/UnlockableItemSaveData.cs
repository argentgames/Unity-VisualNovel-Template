using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.argentgames.visualnoveltemplate
{
    public struct UnlockableItemSaveData
    {
        public string internalName;
        public bool unlockedState;
        public UnlockableItemSaveData(string _name, bool _state)
        {
            this.internalName = _name;
            this.unlockedState = _state;
        }
    }
}
