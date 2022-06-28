using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.argentgames.visualnoveltemplate
{
    public struct Dialogue
{
    public string expression;
    public string speaker;
    public string text;
    public NPC_SO npc;
    public float duration;

    public Dialogue(string expression, string speaker, string text, NPC_SO npc, float duration)
    {
        this.expression = expression;
        this.speaker = speaker;
        this.text = text;
        this.duration = duration;
        this.npc = npc;
    }
}
}
