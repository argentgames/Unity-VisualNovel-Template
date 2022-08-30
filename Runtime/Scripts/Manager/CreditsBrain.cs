using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UniRx;
using System.Threading;

namespace com.argentgames.visualnoveltemplate
{


    public class CreditsBrain : MonoBehaviour
    {
        [SerializeField]
        GameObject goToMove;
        [SerializeField]
        TMP_Text creditsText;
        [SerializeField]
        float scrollingDuration, fadeinDuration, startY, endY;
        PlayerControls _playerControls;
        CancellationTokenSource cts;
        CancellationToken ct;
        // Start is called before the first frame update
        void Awake()
        {
            cts = new CancellationTokenSource();
            ct = cts.Token;
            _playerControls = new PlayerControls();



        }
        private void OnDisable()
        {
            _playerControls.Disable();
        }
        void OnEnable()
        {
            _playerControls.Enable();
        }
        async UniTaskVoid Start()
        {
            AudioManager.Instance.PlayMusic("credits", 0);
            creditsText.alpha = 0;
            // sequence = DOTween.Sequence();
            // sequence.Pause();
            // sequence.Join(creditsText.DOFade(1, fadeinDuration));
            // sequence.Join(goToMove.transform.DOLocalMoveY(endY, scrollingDuration).From(startY).SetEase(Ease.Linear));
            // sequence.AppendInterval(3f);
            // sequence.AppendCallback(() => EndCredits());
            SceneTransitionManager.Instance.FadeIn(1f);
            await UniTask.Delay(System.TimeSpan.FromSeconds(.5f));
            // sequence.Play();

            _playerControls.UI.Click.performed += ctx =>
           {
               if (GameManager.Instance.PersistentGameData.watchedCredits)
               {
                   EndCredits();
                   cts.Cancel();
               }
           };

            MenuManager.Instance.EnableSettingsUIControls();

            // await sequence.WithCancellation(ct);

        }

        // Update is called once per frame
        void Update()
        {

        }

        async UniTaskVoid EndCredits()
        {
            GameManager.Instance.PersistentGameData.watchedCredits = true;
            AudioManager.Instance.StopMusic(0);
            await SceneTransitionManager.Instance.FadeToBlack(2.5f);
            await UniTask.Delay(System.TimeSpan.FromSeconds(2));
            SceneTransitionManager.Instance.LoadScene("MainMenu", 0, 2f);
        }
    }

}