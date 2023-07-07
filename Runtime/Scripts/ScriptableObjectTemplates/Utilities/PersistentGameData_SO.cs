using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UniRx;
using NaughtyAttributes;

/// <summary>
/// Game progress data that you want to save forever between launches of the game!
/// Some common variables are in this class, such as the text you've seen before, so that
/// a player can choose to skip only seen text.
/// 
/// Extend this class with your own custom game-specific variables!
/// </summary>
namespace com.argentgames.visualnoveltemplate
{
    public abstract class PersistentGameData_SO : SerializedScriptableObject
    {
        public HashSet<string> seenText = new HashSet<string>();
        public HashSet<string> chosenChoices = new HashSet<string>();
        
        /// <summary>
        /// By default, unlockable items start off as LOCKED. If you don't want to lock items,
        /// make sure to set them as unlocked when you start up the game!
        /// </summary>
        /// <typeparam name="UnlockableItem"></typeparam>
        /// <returns></returns>
        public List<UnlockableItem> cgUnlocked = new List<UnlockableItem>();
        public List<UnlockableItem> routeUnlocked = new List<UnlockableItem>();
        public List<UnlockableItem> routeCompleted = new List<UnlockableItem>();
        public bool watchedOP = false;
        public bool watchedCredits = false;
        [SerializeField]
        public HashSet<StringVar> stringVariables = new HashSet<StringVar>();
        public HashSet<IntVar> intVariables = new HashSet<IntVar>();
        public HashSet<FloatVar> floatVariables = new HashSet<FloatVar>();
        public HashSet<BoolVar> boolVariables =  new HashSet<BoolVar>();
        /// <summary>
        /// Used to check for GDPR Consent. Mainly an Advertisements/Mobile thing.
        /// </summary>
        public bool gdprConsent = false;
        public int numGamesPlayed = 0;

        private Dictionary<string, UnlockableItem> cgMap = new Dictionary<string, UnlockableItem>();
        private Dictionary<string, UnlockableItem> routeUnlockedMap = new Dictionary<string, UnlockableItem>();
        private Dictionary<string, UnlockableItem> routeCompletedMap = new Dictionary<string, UnlockableItem>();
        private Dictionary<string,StringVar> stringVarsMap = new Dictionary<string,StringVar>();
        private Dictionary<string,FloatVar> floatVarsMap = new Dictionary<string,FloatVar>();
        private Dictionary<string,IntVar> intVarsMap = new Dictionary<string,IntVar>();
        private Dictionary<string,BoolVar> boolVarsMap = new Dictionary<string,BoolVar>();

        private void OnEnable()
        {
            if (seenText == null)
            {
                seenText = new HashSet<string>();
            }
            if (chosenChoices == null)
            {
                chosenChoices = new HashSet<string>();
            }
            if (stringVariables == null)
            {
                stringVariables = new HashSet<StringVar>();
            }
            if (floatVariables == null)
            {
                floatVariables = new HashSet<FloatVar>();
            }
            if (intVariables == null)
            {
                intVariables = new HashSet<IntVar>();
            }
            if (boolVariables == null)
            {
                boolVariables = new HashSet<BoolVar>();
            }
            PopulateMaps();
            ResetDefaults();
        }
        /// <summary>
        ///  Auto save every time any data changes
        /// </summary>
        public virtual void AutoSave()
        {

        }
        public virtual void PopulateMaps()
        {
            cgMap.Clear();
            routeUnlockedMap.Clear();
            routeCompletedMap.Clear();
            foreach (var cg in cgUnlocked)
            {
                cgMap[cg.internalName] = cg;
            }
            foreach (var route in routeUnlocked)
            {
                routeUnlockedMap[route.internalName] = route;
            }
            foreach (var route in routeCompleted)
            {
                routeCompletedMap[route.internalName] = route;
            }
            foreach (var _var in stringVariables)
            {
                stringVarsMap[_var.internalName] = _var;
            }
            foreach (var _var in floatVariables)
            {
                floatVarsMap[_var.internalName] = _var;
            }
            foreach (var _var in boolVariables)
            {
                boolVarsMap[_var.internalName] = _var;
            }
            foreach (var _var in intVariables)
            {
                intVarsMap[_var.internalName] = _var;
            }
        }
        public string GetStringVar(string _name)
        {
            return stringVarsMap[_name].value;
        }
        public float GetFloatVar(string _name)
        {
            return floatVarsMap[_name].value;
        }
        public bool GetBoolVar(string _name)
        {
            return boolVarsMap[_name].value;
        }
        public int GetIntVar(string _name)
        {
            return intVarsMap[_name].value;
        }
        [NaughtyAttributes.Button]
        public virtual void ResetDefaults()
        {
            seenText.Clear();
            chosenChoices.Clear();
            foreach (var cg in cgUnlocked)
            {
                cg.ResetDefaults();
            }
            foreach (var route in routeUnlocked)
            {
                route.ResetDefaults();
            }
            foreach (var route in routeCompleted)
            {
                route.ResetDefaults();
            }
            PopulateMaps();

            watchedOP = false;
            watchedCredits = false;
            gdprConsent = false;
        }

        public bool IsCGUnlocked(string cgName)
        {
            try
            {
                return cgMap[cgName].unlocked.Value;
            }
            catch
            {
                Debug.LogWarningFormat("Could not locate cg [{0}]", cgName);
                return false;
            }

        }
        public bool IsRouteUnlocked(string routeName)
        {
            try
            {
                return routeUnlockedMap[routeName].unlocked.Value;
            }
            catch
            {
                Debug.LogWarningFormat("Could not locate route [{0}]", routeName);
                return false;
            }

        }
        public bool IsRouteCompleted(string routeName)
        {
            try
            {
                return routeCompletedMap[routeName].unlocked.Value;
            }
            catch
            {
                Debug.LogWarningFormat("Could not locate route [{0}]", routeName);
                return false;
            }
        }
        public virtual void SetUnlockableState(string unlockableName, bool newState)
        {
            if (cgMap.ContainsKey(unlockableName))
            {
                cgMap[unlockableName].SetUnlockState(newState);
            }
            else if (routeUnlockedMap.ContainsKey(unlockableName))
            {
                routeUnlockedMap[unlockableName].SetUnlockState(newState);
            }
            else if (routeCompletedMap.ContainsKey(unlockableName))
            {
                routeCompletedMap[unlockableName].SetUnlockState(newState);
            }
            else
            {
                Debug.LogWarningFormat("Unable to locate unlockable [{0}]. Not setting state!",unlockableName);
            }
        }

        public virtual PersistentGameDataSaveData Save()
        {
            var saveData = new PersistentGameDataSaveData(
                this.seenText, this.chosenChoices,null,null,null,
                this.watchedOP,this.watchedCredits,this.gdprConsent,this.numGamesPlayed,
                this.stringVariables, this.floatVariables, this.intVariables, this.boolVariables
            );
            List<UnlockableItemSaveData> cgsUnlocked = new List<UnlockableItemSaveData>();
            foreach (var cg in cgUnlocked)
            {
                cgsUnlocked.Add(cg.Save());
            }
            List<UnlockableItemSaveData> routesUnlocked = new List<UnlockableItemSaveData>();
            foreach (var route in routeUnlocked)
            {
                routesUnlocked.Add(route.Save());
            }
            List<UnlockableItemSaveData> routesCompleted = new List<UnlockableItemSaveData>();
            foreach (var route in routeCompleted)
            {
                routesCompleted.Add(route.Save());
            }
            saveData.cgUnlocked = cgsUnlocked;
            saveData.routeUnlocked = routesUnlocked;
            saveData.routeCompleted = routesCompleted;
            saveData.stringVars = stringVariables;
            saveData.boolVars = boolVariables;
            saveData.floatVars = floatVariables;
            saveData.intVars = intVariables;


            return saveData;
        }
        public virtual void Load(PersistentGameDataSaveData saveData)
        {
            this.seenText = saveData.seenText;
            this.chosenChoices = saveData.chosenChoices;
            foreach (var cg in saveData.cgUnlocked)
            {
                cgMap[cg.internalName].SetUnlockState(cg.unlockedState);
            }
            foreach (var route in saveData.routeCompleted)
            {
                routeCompletedMap[route.internalName].SetUnlockState(route.unlockedState);
            }
            foreach (var route in saveData.routeUnlocked)
            {
                routeUnlockedMap[route.internalName].SetUnlockState(route.unlockedState);
            }
            this.watchedOP = saveData.watchedOP;
            this.watchedCredits = saveData.watchedCredits;
            this.gdprConsent = saveData.gdprConsent;
            this.numGamesPlayed = saveData.numGamesPlayed;
            this.stringVariables = saveData.stringVars;
            this.boolVariables = saveData.boolVars;
            this.floatVariables = saveData.floatVars;
            this.intVariables = saveData.intVars;
        }

    }

public class PersistentGameDataSaveData {
    public HashSet<string> seenText;
    public HashSet<string> chosenChoices;
    public List<UnlockableItemSaveData> cgUnlocked;
    public List<UnlockableItemSaveData> routeUnlocked;
    public List<UnlockableItemSaveData> routeCompleted;
    public bool watchedOP;
    public bool watchedCredits;
    public bool gdprConsent;
    public int numGamesPlayed;
    public HashSet<StringVar> stringVars;
    public HashSet<FloatVar> floatVars;
    public HashSet<IntVar> intVars;
    public HashSet<BoolVar> boolVars;

    public PersistentGameDataSaveData(HashSet<string> seenText, HashSet<string> chosenChoices,
    List<UnlockableItemSaveData> cgUnlocked, List<UnlockableItemSaveData> routeUnlocked, List<UnlockableItemSaveData> routeCompleted,
    bool watchedOP, bool watchedCredits, bool gdprConsent, int numGamesPlayed,
    HashSet<StringVar> stringVars, HashSet<FloatVar> floatVars, HashSet<IntVar> intVars, HashSet<BoolVar> boolVars)
    {
        this.seenText = seenText;
        this.chosenChoices = chosenChoices;
        this.cgUnlocked = cgUnlocked;
        this.routeUnlocked = routeUnlocked;
        this.routeCompleted = routeCompleted;
        this.watchedOP = watchedOP;
        this.watchedCredits = watchedCredits;
        this.gdprConsent = gdprConsent;
        this.numGamesPlayed = numGamesPlayed;
        this.stringVars = stringVars;
        this.floatVars = floatVars;
        this.intVars = intVars;
        this.boolVars = boolVars;
    }

}
    
    


}
