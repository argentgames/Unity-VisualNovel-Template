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
                      videoPlayer.Stop();
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
        [Sirenix.OdinInspector.Button]
        public async UniTask PlayVideo(string videoName)
        {
            _playerControls.VNGameplay.Disable();

            canvas.gameObject.SetActive(true);
            currentVideo = videoBank.GetVideo(videoName);
            videoPlayer.clip = currentVideo.videoClip;
            videoPlayer.Prepare();
            videoPlayer.frame = 0;
            
            await UniTask.WaitUntil(() => videoPlayer.isPrepared);
            
            AudioManager.Instance.PlayMusic(currentVideo.audioName, 0);
            videoPlayer.Play();
            Debug.LogFormat("right after play current video is: {0}",currentVideo);
            await UniTask.WaitWhile(() => IsVideoPlaying);
            currentVideo.watchCount += 1;
            
            StopVideo();
        }
        public void StopVideo()
        {
            Debug.LogFormat("current video is: {0}",currentVideo);
            _playerControls.VNGameplay.Enable();
            
            canvas.gameObject.SetActive(false);
            videoPlayer.frame = (long)videoPlayer.frameCount;
            videoPlayer.Stop();
            AudioManager.Instance.StopMusic(0);
            
            currentVideo = null;
        }
        public void PauseVideo()
        {
            videoPlayer.Pause();
        }
        public void ResumeVideo()
        {
            
        }


    }
}