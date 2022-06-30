using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using FMODUnity;
namespace com.argentgames.visualnoveltemplate
{
    public class Ambient_SO : Sound_SO
    {

        [SerializeField]
        private new bool loop = true;
        [SerializeField]
        private int channel = 0;
        public int Channel
        {
            get { return channel; }
        }

    }
}