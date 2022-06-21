using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
public class CGPresenter : MonoBehaviour
{
    [SerializeField]
    GameObject cgHolder, giantCG;
    List<Button> cgs = new List<Button>();
    // Start is called before the first frame update
    void Awake()
    {
        for (int i = 0 ; i < cgHolder.transform.childCount; i++)
        {
            cgs.Add(cgHolder.transform.GetChild(i).GetComponentInChildren<Button>());
        }
        foreach (var cg in cgs)
        {
            Debug.Log(cg);
            var cgImg = cg.gameObject.transform.GetComponent<CGGalleryCG>().CG;
            cg.OnClickAsObservable().Subscribe(val =>
            {
                giantCG.GetComponent<Image>().sprite = cgImg;
                giantCG.SetActive(true);
            });
        }
        giantCG.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
