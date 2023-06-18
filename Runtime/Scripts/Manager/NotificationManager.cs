using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.argentgames.visualnoveltemplate;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
namespace com.argentgames.visualnoveltemplate
{
    public class NotificationManager : MonoBehaviour
    {
        public static NotificationManager Instance;
        [SerializeField]
        Canvas canvas;
        [SerializeField]
        GameObject textNotificationParent;

        TMP_Text textNotificationText;
        AnimateObjectsToggleEnable textNotificationAnimationController;

        [SerializeField]
        bool overrideTextNotificationShownDuration = false;
        [SerializeField]
        float textNotificationShownDuration = 1.4f;

        void Awake()
        {
            Instance = this;
            
            textNotificationText = textNotificationParent.GetComponentInChildren<TMP_Text>();
                     textNotificationAnimationController = textNotificationParent.GetComponentInChildren<AnimateObjectsToggleEnable>();
 
            
        }
        async UniTaskVoid Start()
        {
            await UniTask.WaitUntil(() => textNotificationAnimationController != null,cancellationToken: this.GetCancellationTokenOnDestroy());
            await textNotificationAnimationController.Disable(0);
            canvas.gameObject.SetActive(false);

        }

        [Sirenix.OdinInspector.Button]

        public void ShowTextNotification(string content)
        {
            textNotificationText.text = content;
            StartCoroutine(I_ShowTextNotification());
        }
        IEnumerator I_ShowTextNotification()
        {
            canvas.gameObject.SetActive(true);
            textNotificationAnimationController.Enable(textNotificationAnimationController.enableAnimationDuration);
            yield return new WaitUntil(() => textNotificationAnimationController.AnimationComplete);
            if (!overrideTextNotificationShownDuration)
            {
                yield return new WaitForSeconds(GameManager.Instance.DefaultConfig.textNotificationShownDuration);
            }
            else
            {
                yield return new WaitForSeconds(textNotificationShownDuration);
            }
            textNotificationAnimationController.Disable(textNotificationAnimationController.disableAnimationDuration);
            yield return new WaitUntil(() => textNotificationAnimationController.AnimationComplete);
            canvas.gameObject.SetActive(false);
            
        }
    }
}
