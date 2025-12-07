using UnityEngine;

[System.Serializable]
public class QuestItemData
{
    public string questID;
    public string displayName;
    public string prerequisiteID;

    
    
    public bool isProgressQuest = false;
    public int currentCount = 0;   // 현재 진행도
    public int targetCount = 1;    // 필요한 개수 (예: 3개)
}

public enum QuestDisplayMode
{
    AllAtOnce,
    Sequential
}
