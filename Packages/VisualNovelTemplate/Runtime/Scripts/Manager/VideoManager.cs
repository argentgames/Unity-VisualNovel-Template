using UnityEngine;
using UnityEngine.Video;
using UniRx;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;
using FMODUnity;

namespace com.argentgames.visualnoveltemplate
{
    public class VideoManager : MonoBehaviour
    {
        public static VideoManager Instance { get; set; }
        VideoPlayer videoPlayer;
        [SerializeField]
        Canvas canvas;
        PlayerControls _playerControls;

        public bool IsVideoPlaying { get { return videoPlayer.isPlaying; } }
        async UniTaskVoid Awake()
        {
            _playerControls = new PlayerControls();

            _playerControls.UI.Click.performed += ctx =>
           {
               if (GameManager.Instance.PersistentGameData.watchedOP && videoPlayer.isPlaying)
               {
                   Debug.Log("skip op");
                   videoPlayer.frame = (long)videoPlayer.frameCount;
                   videoPlayer.Pause();
                   AudioManager.Instance.StopMusic(0);

               }

           };


            Instance = this;

            videoPlayer = GetComponent<VideoPlayer>();

            await UniTask.WaitUntil(() => GameManager.Instance != null);


            videoPlayer.Prepare();
            videoPlayer.frame = 0;

        }
        private void OnDisable()
        {
            _playerControls.Disable();
        }
        void OnEnable()
        {
            _playerControls.Enable();
        }
        [Button]
        public async UniTask PlayVideo()
        {
            videoPlayer.Prepare();
            videoPlayer.frame = 0;
            canvas.gameObject.SetActive(true);
            AudioManager.Instance.PlayMusic("op", 0);
            videoPlayer.Play();
            await UniTask.WaitWhile(() => IsVideoPlaying);
            canvas.gameObject.SetActive(false);
            GameManager.Instance.PersistentGameData.watchedOP = true;
        }


    }
}