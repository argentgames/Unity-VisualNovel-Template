using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace com.argentgames.visualnoveltemplate
{
    public class ADVPortraitPresenter : PortraitPresenter
    {
        public override UniTaskVoid ShowChar(string npcName)
        {
            var npcso = GameManager.Instance.GetNPC(npcName);

            // instantiate npcso.portraitSprite and parent it to portraitHolder
            // hide any existing objs on portraitHolder
            // show npcso.portraitSprite
            // destroy any hidden objs on portraitHolder. If we are using addressables, make sure to release the address...
            throw new System.NotImplementedException();
        }

    }
}
