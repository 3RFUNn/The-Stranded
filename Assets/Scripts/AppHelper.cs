using System.Collections.Generic;
using UnityEngine;

public static class AppHelper
{
    // Example of static variables to hold data
    public static int playerScore = 0;               // Player's score
    public static string playerName = "Guest";       // Player's name
    public static bool isSoundEnabled = true;        // Sound setting
    public static List<string> unlockedLevels = new List<string>(); // List of unlocked levels
    public static bool stopCrushing = false;

    public static bool gameEnded = false;

    public static bool hintDisplayed = false; 

    public static bool hasKey = false;

    public static int PlayerHealth = 100;

    public static int CurrentLevel = 1;

    public static bool HasTalkedFinalNPC = false;

    // Example of a method to reset the data
    public static void ResetData()
    {
        playerScore = 0;
        playerName = "Guest";
        isSoundEnabled = true;
        unlockedLevels.Clear();
        Debug.Log("AppHelper data has been reset.");
    }

    // Example of a method to log all data
    public static void LogData()
    {
        Debug.Log($"Player Name: {playerName}");
        Debug.Log($"Player Score: {playerScore}");
        Debug.Log($"Sound Enabled: {isSoundEnabled}");
        Debug.Log($"Unlocked Levels: {string.Join(", ", unlockedLevels)}");
    }
}
