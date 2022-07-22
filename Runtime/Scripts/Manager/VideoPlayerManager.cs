using UnityEngine;
using UnityEngine.Video;
using UniRx;
using UnityEngine.UI;
// using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;
using FMODUnity;
using NaughtyAttributes;
namespace com.argentgames.visualnoveltemplate
{
    public class VideoPlayerManager : MonoBehaviour
    {
        public static VideoPlayerManager Instance { get; set; }
        VideoPlayer videoPlayer;
        [Tooltip("Canvas that contains the image that shows the video to the player.")]
        [SerializeField]
        Canvas canvas;
        PlayerControls _playerControls;
        [SerializeField]
        VideoBank_SO videoBank;
        Video_SO currentVideo = null;
        public bool IsVideoPlaying { get { return videoPlayer.isPlaying; } }
        async UniTaskVoid Awake()
        {
            _playerControls = new PlayerControls();

            _playerControls.UI.Click.performed += ctx =>
           {
               if (currentVideo == null)
               {
                   return;
               }
               if (currentVideo.isSkippableWithoutWatching || currentVideo.HasWatchedOnce)
               {
                   if (videoPlayer.isPlaying)
                   {
                        StopVideo();
                   }
               }

           };


            Instance = this;

            videoPlayer = GetComponentInChildren<VideoPlayer>();

            await UniTask.WaitUntil(() => GameManager.Instance != null);

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
        public async UniTask PlayVideo(string videoName)
        {
            currentVideo = videoBank.GetVideo(videoName);
            videoPlayer.clip = currentVideo.videoClip;
            videoPlayer.Prepare();
            videoPlayer.frame = 0;
            canvas.gameObject.SetActive(true);
            AudioManager.Instance.PlayMusic(currentVideo.audioName, 0);
            videoPlayer.Play();
            await UniTask.WaitWhile(() => IsVideoPlaying);
            canvas.gameObject.SetActive(false);
            currentVideo.watchCount += 1;
        }
        public void StopVideo()
        {
            videoPlayer.frame = (long)videoPlayer.frameCount;
            videoPlayer.Pause();
            AudioManager.Instance.StopMusic(0);
            currentVideo = null;
        }


    }
}