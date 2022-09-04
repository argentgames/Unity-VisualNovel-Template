using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

namespace com.argentgames.visualnoveltemplate
{
public class HistoryPresenter : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject historyContentPrefab;
    public GameObject contentHolder;
    public ScrollRect scrollRect;
    public Scrollbar scrollbar;
    string previousSpeaker = "";
    public Canvas canvas;
    [SerializeField]
    int maxHistoryLines = 25;

    async UniTaskVoid OnEnable()
    {
        try
        {
            var startIDX = Mathf.Clamp(DialogueSystemManager.Instance.currentSessionDialogueHistory.Count - maxHistoryLines, 0, DialogueSystemManager.Instance.currentSessionDialogueHistory.Count - maxHistoryLines);
            Debug.Log("startIDX for history is: " + startIDX.ToString());
            for (int i = startIDX; i < DialogueSystemManager.Instance.currentSessionDialogueHistory.Count - 1; i++)
            {
                var line = DialogueSystemManager.Instance.currentSessionDialogueHistory[i];
                CreateHistoryObject(line.speaker, line.line);
            }

            // UIExtensions.UpdateLayout(canvas.transform);

            // Canvas.ForceUpdateCanvases();

            contentHolder.GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
            contentHolder.GetComponent<ContentSizeFitter>().SetLayoutVertical();

            // scrollRect.content.GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical() ;
            // scrollRect.content.GetComponent<ContentSizeFitter>().SetLayoutVertical() ;
            await UniTask.Yield();
            scrollRect.verticalNormalizedPosition = 0;
            // scrollbar.value = 0;
        }
        catch
        {
            Debug.Log("no ds dialogeushistory exists");
        }


    }
    void Awake()
    {
        for (int i = 0; i < contentHolder.transform.childCount; i++)
        {
            Destroy(contentHolder.transform.GetChild(i).gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void CreateHistoryObject(string speakerName, string content)
    {
        var s = Instantiate(historyContentPrefab, contentHolder.transform);
        if (speakerName == previousSpeaker || speakerName == "narrator")
        {
            speakerName = "";
            s.transform.GetChild(0).gameObject.SetActive(false);
        }
        else if (speakerName == "mc")
        {
            speakerName = (string)DialogueSystemManager.Instance.Story.variablesState["mc_name"];
        }
        s.transform.GetChild(0).GetComponent<TMP_Text>().text = speakerName;
        s.transform.GetChild(1).GetComponent<TMP_Text>().text = content;
    }
}
}