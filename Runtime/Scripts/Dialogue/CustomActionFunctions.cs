using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
namespace com.argentgames.visualnoveltemplate
{
    /// <summary>
    /// Ink functions by default are resolved immediately and continue to the next line.
    /// We define functions in this class when we want a function to complete and pause 
    /// after it is done being run. We might choose to automatically proceed to the next line
    /// for certain functions.
    /// </summary>
    public abstract class CustomActionFunctions : MonoBehaviour
    {
        public abstract UniTask ActionFunction(string text,CancellationToken ct);
    }
}
