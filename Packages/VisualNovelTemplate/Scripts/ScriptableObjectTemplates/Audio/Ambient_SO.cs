using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using FMODUnity;
public class Ambient_SO : Sound_SO
{
    [SerializeField]
    private string internalName;
    [SerializeField]
    private EventReference _event;
    [SerializeField]
    private bool loop = true;
    [SerializeField]
    private int channel = 0;
    public override string InternalName {
        get
        {
            return internalName;
        }
        set
        {
            internalName = value;
        }
    }
    public override EventReference Event {
        get
        {
            return _event;
        }
        set
        {
            _event = value;
        }
    }
    public override bool IsLoop {
        get
        {
            return loop;
        }
    }
    public int Channel {
        get { return channel;}
    }

}
