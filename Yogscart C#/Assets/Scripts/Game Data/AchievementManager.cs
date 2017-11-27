using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    Dictionary<string, Achievement> achievements;
    Dictionary<string, float> statistics;

    SaveDataManager saveDataManager;

    public bool canSave = true;
    private List<Achievement> needsSaving;

    void Start()
    {
        saveDataManager = FindObjectOfType<SaveDataManager>();

        achievements = new Dictionary<string, Achievement>();
        statistics = new Dictionary<string, float>();

        needsSaving = new List<Achievement>();

        //Load Achievements Names
        TextAsset textAsset = Resources.Load<TextAsset>("Localisation/English/Achievements");
        XmlDocument xmldoc = new XmlDocument();
        xmldoc.LoadXml(textAsset.text);

        List<int> existingIDs = new List<int>();

        foreach (XmlNode xmlNode in xmldoc.DocumentElement.ChildNodes)
        {
            int achievementID = int.Parse(xmlNode.Attributes["id"].Value);

            //Check ID dosen't exist already
            if (existingIDs.Contains(achievementID))
                throw new System.Exception("Achievement ID already exists!!!");
            else
                existingIDs.Add(achievementID);

            //Create Achievement
            string achievementNameID = xmlNode.Attributes["name"].Value;
            achievements.Add(achievementNameID, new Achievement(achievementID, achievementNameID, xmlNode.Attributes["description"].Value));

            Debug.Log("Creating Achievement: " + achievements[achievementNameID].uniqueName + " - " + achievements[achievementNameID].description);
        }

        //Load Achievements Unlocked From Save
        foreach(Achievement achievement in achievements.Values)
        {
            if (saveDataManager.GetGotAchievement(achievement.uniqueID))
                achievement.Unlock();
        }
    }

    public int GetTotalAchievements()
    {
        return achievements.Count;
    }

    public void UnlockAchievement(string _achievementName)
    {
        Achievement achievement;
        if(achievements.TryGetValue(_achievementName, out achievement))
        {
            if (!achievement.unlocked)
            {
                Debug.Log("Player has unlocked " + achievement.uniqueName);
                achievement.Unlock();
                needsSaving.Add(achievement);

                //Do UI Fanfare
            }
        }
        else
        {
            Debug.Log(_achievementName + " is not a valid achievement");
        }
    }

    public void UpdateStatistic(string _statisticName, float value)
    {
        statistics[_statisticName] = value;
    }

    public float GetStatistic(string _statisticName)
    {
        float returnVal = 0;
        statistics.TryGetValue(_statisticName, out returnVal);

        return returnVal;
    }

    private void Update()
    {
        //Save if we are allowed to
        if(needsSaving.Count > 0 && canSave)
        {
            foreach (Achievement achievement in needsSaving)
            {
                saveDataManager.SetGotAchievement(achievement.uniqueID);
            }

            saveDataManager.Save();
            needsSaving = new List<Achievement>();
        }
    }
}

public class Achievement
{
    public int uniqueID { get; private set; }
    public string uniqueName { get; private set; }

    public string description { get; private set; }
    public bool unlocked { get; private set; }

    public Achievement(int _uniqueID, string _uniqueName, string _description)
    {
        uniqueID = _uniqueID;
        uniqueName = _uniqueName;
        description = _description;
        unlocked = false;
    }

    public void UpdateDescription(string _newDescription)
    {
        description = _newDescription;
    }

    public void Unlock()
    {
        unlocked = true;
    }
}