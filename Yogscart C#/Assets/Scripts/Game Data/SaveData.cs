using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    //V1 - Inital Setup
    public const int saveVersion = 1;

    /// <summary>
    /// NOTE!
    /// Always add new values to the bottom to avoid breaking old saves.
    /// </summary>

    // Which save version is this
    public int localVersion;

    public bool hasDoneNameInput;

    // Unlocked Characters
    public int unlockedCharacters; //Use bit shifting so having Lewis (1st character unlocked) is 1 << 0, Simon 1 << 1, Duncan 1 << 2 .etc;

    // Unlocked Hats
    public int unlockedHats; //No Hat 1 << 0, Witches Hat 1 << 1, Santa Hat 1 << 2 .etc;

    // Unlocked Karts
    public int unlockedKarts;

    // Unlocked Wheels
    public int unlockedWheels;

    // Tournament Ranks
    public Rank[][] tournamentRanks;

    //Insane Unlocked
    public bool insaneUnlocked;

    // Track Times
    public List<float> trackTimes;

    // Player Name
    public string playerName;

    // Stream Mode
    public bool streamMode;

    // Mouse Scale
    public float mouseScale;

    // Controller Scale
    public float controllerScale;

    //Achievements
    public bool[] collectedAchievements;

    //Statistics

    // How many pieces of Lapis has the player collected
    public int lapisCount;

    // How many joffocakes have been used
    public int joffosUsed;

    // How many dirtblocks have been fired
    public int dirtBlocksFired;

    // How many eggs have been fired
    public int eggsFired;

    // How many JR have been fired
    public int jrsFired;

    // How many dirtblocks have hit a player
    public int dirtBlockHit;

    // How many eggs have hit a player
    public int eggsHit;

    // How many jrs have hit a player
    public int jrsHit;


}
