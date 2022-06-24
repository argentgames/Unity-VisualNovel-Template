using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
public class GDPRConsent : MonoBehaviour
{
    [SerializeField]
    Button close;
    private const string ADS_PERSONALIZATION_CONSENT = "Ads";
    // Start is called before the first frame update
    void Start()
    {
        close.OnClickAsObservable().Subscribe(val =>
        {
            GameManager.Instance.Settings.gdprConsent = true;
        }).AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowConsentPopup()
    {
        // Don't attempt to show a dialog if another dialog is already visible
		// if( SimpleGDPR.IsDialogVisible )
			// return;
    }
}
