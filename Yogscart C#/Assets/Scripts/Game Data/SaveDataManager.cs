using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveDataManager : MonoBehaviour
{
    private SaveData saveData;
    private CurrentGameData gameData;

    private const string saveLocation = "/YogscartSaveData.gd";

    //------------------------------------Changeable Methods-------------------------------------
    public void ResetSave()
    {
        Debug.Log("Resetting Save!");
        saveData = new SaveData();

        //Perform Save Resetting Here
        saveData.localVersion = SaveData.saveVersion;
        saveData.hasDoneNameInput = false;
        saveData.lapisCount = 0;
        saveData.eggsHit = 0;

        //Do Character Unlocking
        saveData.unlockedCharacters = 0;
        for(int i = 0; i < gameData.characters.Length; i++)
        {
            if(gameData.characters[i].unlocked == UnlockedState.FromStart)
            {
                UnlockCharacter(i);
            }
        }

        //Do Hat Unlocking
        saveData.unlockedHats = 0;
        for (int i = 0; i < gameData.hats.Length; i++)
        {
            if (gameData.hats[i].unlocked == UnlockedState.FromStart)
            {
                UnlockHat(i);
            }
        }

        //Do Kart Unlocking
        saveData.unlockedKarts = 0;
        for (int i = 0; i < gameData.karts.Length; i++)
        {
            if (gameData.karts[i].unlocked == UnlockedState.FromStart)
            {
                UnlockKart(i);
            }
        }

        //Do Wheel Unlocking
        saveData.unlockedWheels = 0;
        for (int i = 0; i < gameData.wheels.Length; i++)
        {
            if (gameData.wheels[i].unlocked == UnlockedState.FromStart)
            {
                UnlockWheel(i);
            }
        }

        //Tournament Ranks
        List<Rank[]> tournamentRanks = new List<Rank[]>();
        foreach (Tournament tournament in gameData.tournaments)
            tournamentRanks.Add(new Rank[] { Rank.NoRank, Rank.NoRank, Rank.NoRank, Rank.NoRank });
        saveData.tournamentRanks = tournamentRanks.ToArray();

        //Insane Unlocked
        saveData.insaneUnlocked = false;

        //Track Times
        saveData.trackTimes = new List<float>();
        foreach (Tournament tournament in gameData.tournaments)
            foreach (Track track in tournament.tracks)
                saveData.trackTimes.Add(0);

        saveData.playerName = "Player";
        saveData.streamMode = false;
        saveData.mouseScale = 1f;
        saveData.controllerScale = 1f;
        saveData.collectedAchievements = new bool[0];

        //Save the new Game Data
        SaveGame();
    }

    private void UpdateSave()
    {
        //Ensure Save Data has the correct values
        saveData.localVersion = SaveData.saveVersion;

        //Save the Game Data
        SaveGame();
    }

    //------------------------------------Non-Changeable Methods-------------------------------------
    public void Save()
    {
        //Ensure Save Data has the correct values
        saveData.localVersion = SaveData.saveVersion;

        //Ensure we have the correct number of achievements
        List<bool> copy = saveData.collectedAchievements.ToList();
        while (copy.Count < FindObjectOfType<AchievementManager>().GetTotalAchievements())
        {
            copy.Add(false);
        }
        saveData.collectedAchievements = copy.ToArray();

        //Save the Game Data
        SaveGame();
    }

    private void SaveGame()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + saveLocation);
        bf.Serialize(file, saveData);
        file.Close();
    }

    private void LoadGame()
    {
        bool saveLoaded = false;

        try
        {
            if (File.Exists(Application.persistentDataPath + saveLocation))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + saveLocation, FileMode.Open);
                saveData = (SaveData)bf.Deserialize(file);
                file.Close();

                if (saveData.localVersion != SaveData.saveVersion)
                {
                    UpdateSave();
                }

                saveLoaded = true;
            }
        }
        catch (System.Exception err)
        {
            Debug.Log("ERROR LOADING SAVE! " + err.Message);
        }

        if (!saveLoaded)
        {
            ResetSave();
        }
        else
        {
            Debug.Log("Save Loaded!");

            Debug.Log("Version:" + saveData.localVersion);        
            Debug.Log("Unlocked Characters:" + System.Convert.ToString(saveData.unlockedCharacters, 2));
            Debug.Log("Unlocked Hats:" + System.Convert.ToString(saveData.unlockedHats, 2));
            Debug.Log("Unlocked Karts:" + System.Convert.ToString(saveData.unlockedKarts, 2));
            Debug.Log("Unlocked Wheels:" + System.Convert.ToString(saveData.unlockedWheels, 2));

            foreach (Rank[] tournament in saveData.tournamentRanks)
            {
                string tournamentRanks = "";
                foreach (Rank rank in tournament)
                {
                    tournamentRanks += rank.ToString() + ", ";
                }

                Debug.Log("Tournament Rank: " + tournamentRanks);
            }

            Debug.Log("Insane Unlocked:" + saveData.insaneUnlocked);

            int trackCount = 0;
            foreach (Tournament tournament in gameData.tournaments)
            {
                foreach (Track track in tournament.tracks)
                {
                    Debug.Log(track.name + " Best Time:" + saveData.trackTimes[trackCount]);
                    trackCount++;
                }
            }

            Debug.Log("Player Name:" + saveData.playerName);
            Debug.Log("Stream Mode:" + saveData.streamMode);
            Debug.Log("Mouse Scale:" + saveData.mouseScale);
            Debug.Log("Controller Scale:" + saveData.controllerScale);

            string achivementsString = "";
            foreach (bool achivement in saveData.collectedAchievements)
            {
                achivementsString += achivement.ToString() + ", ";
            }
            Debug.Log("Achivements:" + achivementsString);

            Debug.Log("Lapis Amount:" + saveData.lapisCount);
            Debug.Log("Egg Hits:" + saveData.eggsHit);
        }
    }

    public void Start()
    {
        gameData = FindObjectOfType<CurrentGameData>();

        //Load the game on start
        LoadGame();
    }

    //Update Save Methods

    //Character Unlocking SET/GET
    public void UnlockCharacter(int _characterID)       { saveData.unlockedCharacters |= (1 << _characterID); }
    public bool GetCharacterUnlocked(int _characterID)  { return (saveData.unlockedCharacters & (1 << _characterID)) != 0; }

    //Hat Unlocking SET/GET
    public void UnlockHat(int _hatID)                   { saveData.unlockedHats |= (1 << _hatID); }
    public bool GetHatUnlocked(int _hatID)              { return (saveData.unlockedHats & (1 << _hatID)) != 0; }

    //Kart Unlocking SET/GET
    public void UnlockKart(int _kartID)                 { saveData.unlockedKarts |= (1 << _kartID); }
    public bool GetKartUnlocked(int _kartID)            { return (saveData.unlockedKarts & (1 << _kartID)) != 0; }

    //Wheel Unlocking SET/GET
    public void UnlockWheel(int _wheelID)               { saveData.unlockedWheels |= (1 << _wheelID); }
    public bool GetWheelUnlocked(int _wheelID)          { return (saveData.unlockedWheels & (1 << _wheelID)) != 0; }

    //Tournament Rank Set/GET
    public void SetTournamentRank(int _tournamentID, int _difficulty, Rank _rank) { saveData.tournamentRanks[_tournamentID][_difficulty] = _rank;  }
    public Rank[] GetTournamentRanks(int _tournamentID) { return saveData.tournamentRanks[_tournamentID]; }

    //Unlocked Insane Rank Set/GET
    public void SetUnlockedInsane(bool _unlockedInsane) { saveData.insaneUnlocked = _unlockedInsane; }
    public bool GetUnlockedInsane() { return saveData.insaneUnlocked; }

    //Track Time Set/GET
    public void SetTrackTime(int _trackID, float _newTime) { saveData.trackTimes[_trackID] = _newTime; }
    public float GetTrackTime(int _trackID) { return saveData.trackTimes[_trackID]; }

    //PlayerName SET/GET
    public void SetPlayerName(string _newPlayerName) { saveData.playerName = _newPlayerName; saveData.hasDoneNameInput = true; }
    public string GetPlayerName() { return saveData.playerName; }
    public bool GetHasDonePlayerName() { return saveData.hasDoneNameInput; }

    //Stream Mode SET/GET
    public void SetStreamMode(bool _streamMode) { saveData.streamMode = _streamMode; }
    public bool GetStreamMode() { return saveData.streamMode; }

    //Mouse Scale SET/GET
    public void SetMouseScale(float _mouseScale) { saveData.mouseScale = _mouseScale; }
    public float GetMouseScale() { return saveData.mouseScale; }

    //Controller Scale SET/GET
    public void SetControllerScale(float _controllerScale) { saveData.controllerScale = _controllerScale; }
    public float GetControllerScale() { return saveData.controllerScale; }

    //Achievements SET/GET
    public void SetGotAchievement(int _achievementID)
    {
        if (saveData.collectedAchievements.Length < FindObjectOfType<AchievementManager>().GetTotalAchievements())
        {
            List<bool> copy = saveData.collectedAchievements.ToList();
            while (copy.Count < FindObjectOfType<AchievementManager>().GetTotalAchievements())
            {
                copy.Add(false);
            }
            saveData.collectedAchievements = copy.ToArray();
        }

        saveData.collectedAchievements[_achievementID] = true;
    }
    public bool GetGotAchievement(int _achievementID) { return saveData.collectedAchievements.Length > _achievementID && saveData.collectedAchievements[_achievementID]; }

    //--------------------------------------------------- Statistics ---------------------------------------------------

    //Lapis Count SET/GET
    public void SetLapisAmount(int _newLapis)
    {
        saveData.lapisCount = _newLapis;

        //Do Achievement Check
        AchievementManager achievementManager = FindObjectOfType<AchievementManager>();
        if (_newLapis >= 50) { achievementManager.UnlockAchievement("50Lapis"); }
        if (_newLapis >= 500) { achievementManager.UnlockAchievement("500Lapis"); }
        if (_newLapis >= 1000) { achievementManager.UnlockAchievement("1000Lapis"); }
    }
    public int GetLapisAmount() { return saveData.lapisCount; }

    //Eggs Hit SET/GET
    public void SetEggsHit(int _newHit)
    {
        saveData.eggsHit = _newHit;

        //Do Achievement Check
        AchievementManager achievementManager = FindObjectOfType<AchievementManager>();
        if (_newHit >= 10) { achievementManager.UnlockAchievement("10EggStrike"); }
        if (_newHit >= 50) { achievementManager.UnlockAchievement("50EggStrike"); }
        if (_newHit >= 100) { achievementManager.UnlockAchievement("100EggStrike"); }
        if (_newHit >= 100) { achievementManager.UnlockAchievement("200EggStrike"); }
    }
    public int GetEggsHit() { return saveData.eggsHit; }

}
