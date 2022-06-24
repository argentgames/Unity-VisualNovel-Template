using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortraitPresenter : MonoBehaviour
{
    [SerializeField]
    GameObject phyllis, vendel, leif, hakon, digby, maja, lysander, hermia, gagnef;
    [SerializeField] GameObject wrapper;
    void Awake()
    {
    }
    public void ShowChar(NPC_NAME npc)
    {
        Debug.Log(((int)npc));
        switch (npc)
        {
            case NPC_NAME.HAKON:
                hakon.SetActive(true);
                phyllis.SetActive(false);
                vendel.SetActive(false);
                leif.SetActive(false);
                digby.SetActive(false);
                maja.SetActive(false);
                lysander.SetActive(false);
                hermia.SetActive(false);
                gagnef.SetActive(false);
                ShowPortrait();
                break;
            case NPC_NAME.VENDEL:
                hakon.SetActive(false);
                phyllis.SetActive(false);
                vendel.SetActive(true);
                leif.SetActive(false);
                digby.SetActive(false);
                maja.SetActive(false);
                lysander.SetActive(false);
                hermia.SetActive(false);
                gagnef.SetActive(false);
                ShowPortrait();
                break;
            case NPC_NAME.LEIF:
                hakon.SetActive(false);
                phyllis.SetActive(false);
                vendel.SetActive(false);
                digby.SetActive(false);
                leif.SetActive(true);
                maja.SetActive(false);
                lysander.SetActive(false);
                hermia.SetActive(false);
                gagnef.SetActive(false);
                ShowPortrait();
                break;
            case NPC_NAME.PHYLLIS:
                hakon.SetActive(false);
                phyllis.SetActive(true);
                vendel.SetActive(false);
                digby.SetActive(false);
                leif.SetActive(false);
                maja.SetActive(false);
                lysander.SetActive(false);
                hermia.SetActive(false);
                gagnef.SetActive(false);
                ShowPortrait();
                break;
            case NPC_NAME.DIGBY:
                hakon.SetActive(false);
                phyllis.SetActive(false);
                vendel.SetActive(false);
                digby.SetActive(true);
                leif.SetActive(false);
                maja.SetActive(false);
                lysander.SetActive(false);
                hermia.SetActive(false);
                gagnef.SetActive(false);
                ShowPortrait();
                break;
            case NPC_NAME.LYSANDER:
                hakon.SetActive(false);
                phyllis.SetActive(false);
                vendel.SetActive(false);
                digby.SetActive(false);
                leif.SetActive(false);
                maja.SetActive(false);
                lysander.SetActive(true);
                hermia.SetActive(false);
                gagnef.SetActive(false);
                ShowPortrait();
                break;
            case NPC_NAME.MAJA:
                hakon.SetActive(false);
                phyllis.SetActive(false);
                vendel.SetActive(false);
                digby.SetActive(false);
                leif.SetActive(false);
                maja.SetActive(true);
                lysander.SetActive(false);
                hermia.SetActive(false);
                gagnef.SetActive(false);
                ShowPortrait();
                break;
            case NPC_NAME.HERMIA:
                hakon.SetActive(false);
                phyllis.SetActive(false);
                vendel.SetActive(false);
                digby.SetActive(false);
                leif.SetActive(false);
                maja.SetActive(false);
                lysander.SetActive(false);
                hermia.SetActive(true);
                gagnef.SetActive(false);
                ShowPortrait();
                break;
            case NPC_NAME.GAGNEF:
                hakon.SetActive(false);
                phyllis.SetActive(false);
                vendel.SetActive(false);
                digby.SetActive(false);
                leif.SetActive(false);
                maja.SetActive(false);
                lysander.SetActive(false);
                hermia.SetActive(false);
                gagnef.SetActive(true);
                ShowPortrait();
                break;
            default:
                HidePortrait();
                break;

        }
    }
    public GameObject GetCharGO(NPC_NAME npc)
    {
        switch (npc)
        {
            case NPC_NAME.HAKON:
                return hakon;
            case NPC_NAME.VENDEL:
                return vendel;
            case NPC_NAME.LEIF:
                return leif;
            case NPC_NAME.PHYLLIS:
                return phyllis;
            case NPC_NAME.DIGBY:
                return digby;
            case NPC_NAME.MAJA:
                return maja;
            case NPC_NAME.LYSANDER:
                return lysander;
            case NPC_NAME.HERMIA:
                return hermia;
            case NPC_NAME.GAGNEF:
                return gagnef;
        }
        return null;
    }
    public void HidePortrait()
    {
        wrapper.SetActive(false);
    }
    public void ShowPortrait()
    {
        if (!wrapper.activeSelf)
        {
            wrapper.SetActive(true);
        }

    }
    public bool IsShowingPortrait { get { return wrapper.activeSelf; } }
}
