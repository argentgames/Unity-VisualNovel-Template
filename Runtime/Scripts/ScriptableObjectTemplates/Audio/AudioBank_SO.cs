using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using FMODUnity;
using System;
public class AudioBank_SO : SerializedScriptableObject
{
    [BoxGroup("Maps")]
    [InfoBox("DO NOT TOUCH, AUTOMATICALLY POPULATES AND CLEARS ITSELF")]

    public Dictionary<string,Music_SO> musicMap = new Dictionary<string, Music_SO>();
    [BoxGroup("Maps")]
 
    public Dictionary<string,Ambient_SO> ambientMap = new Dictionary<string, Ambient_SO>();
    [BoxGroup("Maps")]
    public Dictionary<string,SFX_SO> sfxMap = new Dictionary<string, SFX_SO>();  
      
    public List<Tuple<string,EventReference>> ambFiles = new List<Tuple<string,EventReference>>();
    public List<Tuple<string,EventReference>> sfxFiles = new List<Tuple<string,EventReference>>();
    public List<Tuple<string,EventReference>> musicFiles = new List<Tuple<string,EventReference>>();

    [Button("Populate maps")]
    public void PopulateMaps()
    {
        musicMap.Clear();
        ambientMap.Clear();
        sfxMap.Clear();
        for (int i=0; i < ambFiles.Count; i++)
        {
            var f = ambFiles[i];
            var s = ScriptableObject.CreateInstance<Ambient_SO>();
            s.InternalName = f.Item1;
            s.Event = f.Item2;
            ambientMap.Add(s.InternalName,s);
        }
        for (int i=0; i < sfxFiles.Count; i++)
        {
            var f = sfxFiles[i];
            var s = ScriptableObject.CreateInstance<SFX_SO>();
            s.InternalName = f.Item1;
            s.Event = f.Item2;
            sfxMap.Add(s.InternalName,s);
        }
        for (int i=0; i < musicFiles.Count; i++)
        {
            var f = musicFiles[i];
            var s = ScriptableObject.CreateInstance<Music_SO>();
            s.InternalName = f.Item1;
            s.Event = f.Item2;
           musicMap.Add(s.InternalName,s);
        }

        
        
    }

}
