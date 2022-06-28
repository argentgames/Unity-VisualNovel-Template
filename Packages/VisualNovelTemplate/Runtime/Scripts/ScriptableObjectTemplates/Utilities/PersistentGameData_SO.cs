using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UniRx;
namespace com.argentgames.visualnoveltemplate
{
    public class PersistentGameData_SO : SerializedScriptableObject
    {
        public HashSet<string> seenText = new HashSet<string>();
        public HashSet<string> chosenChoices = new HashSet<string>();
        public List<UnlockableItem> cgUnlocked = new List<UnlockableItem>();
        public List<UnlockableItem> routeUnlocked = new List<UnlockableItem>();
        public List<UnlockableItem> routeCompleted = new List<UnlockableItem>();
        public bool watchedOP = false;
        public bool watchedCredits = false;
        public bool gdprConsent = false;

        private Dictionary<string, UnlockableItem> cgMap = new Dictionary<string, UnlockableItem>();
        private Dictionary<string, UnlockableItem> routeUnlockedMap = new Dictionary<string, UnlockableItem>();
        private Dictionary<string, UnlockableItem> routeCompletedMap = new Dictionary<string, UnlockableItem>();

        private void OnEnable()
        {
            PopulateMaps();
        }
        private void PopulateMaps()
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
        }
        public void ResetDefaults()
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
        public void SetUnlockableState(string unlockableName, bool newState)
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

        public void Save()
        {

        }
        public void Load()
        {

        }

    }

    
    


}
