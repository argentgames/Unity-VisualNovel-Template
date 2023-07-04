using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
namespace com.argentgames.visualnoveltemplate
{
    [System.Serializable]
    public struct StringVar
    {
        public string internalName;
        public string value;
        public StringVar(string _name,string _val){this.internalName = _name; this.value = _val;}
    }
    public class FloatVar
    {
        public string internalName;
        public float value;
    }
    public class IntVar
    {
        public string internalName;
        public int value;
    }
    public class BoolVar
    {
        public string internalName;
        public bool value;
    }
}
