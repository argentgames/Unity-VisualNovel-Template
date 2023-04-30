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
    /// 
    /// We provide a few convenience functions so we can skip them...
    /// </summary>
    public abstract class CustomActionFunctions : MonoBehaviour
    {
        public IEnumerator IE_Delay;
        public bool isRunningDelay = false;
        public bool IsRunningDelay => isRunningDelay;
        public IEnumerator RunDelay(float duration)
        {
            isRunningDelay = true;
            yield return new WaitForSeconds(duration);
            isRunningDelay = false;
        }
        public void CancelDelay()
        {
            Debug.Log("try to run cancel delay");
            if (IE_Delay != null)
            {
                StopCoroutine(IE_Delay);
                isRunningDelay = false;
            }
        }
        public abstract UniTask ActionFunction(string text,CancellationToken ct);
    }
}
