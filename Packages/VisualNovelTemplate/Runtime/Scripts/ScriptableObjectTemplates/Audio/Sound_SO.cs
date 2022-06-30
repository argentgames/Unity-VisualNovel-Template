using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using FMODUnity;
namespace com.argentgames.visualnoveltemplate
{

    public abstract class Sound_SO : SerializedScriptableObject
    {
        [SerializeField]
        private string internalName;
        [SerializeField]
        private EventReference _event;
        [SerializeField]
        private string closedCaption;

        [SerializeField]
        public bool loop = true;

        public virtual string InternalName
        {
            get
            {
                return internalName;
            }
            set
            {
                internalName = value;
            }
        }
        public virtual EventReference Event
        {
            get
            {
                return _event;
            }
            set
            {
                _event = value;
            }
        }
        public virtual string ClosedCaption
        {
            get
            {
                return closedCaption;
            }
            set
            {
                closedCaption = value;
            }
        }
        public virtual bool IsLoop
        {
            get
            {
                return loop;
            }
        }

    }
}