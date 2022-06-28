using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.argentgames.visualnoveltemplate
{
    public struct DialogueHistoryLine
{
    public string speaker;
    public string line;
    public DialogueHistoryLine(string line)
    {
        this.speaker = "";
        this.line = line;
    }
    public DialogueHistoryLine(string speaker, string line)
    {
        this.speaker = speaker;
        this.line = line;
    }


}
}
