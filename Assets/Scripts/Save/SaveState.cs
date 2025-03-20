using System;

[System.Serializable]
public class SaveState
{
    private const int HAT_COUNT = 31;

    public int Highscore { get; set; }
    public int Fish { get; set; }
    public DateTime LastSaveTime { get; set; }
    public int CurrentHatIndex { get; set; }
    public int[] UnlockedHatFlag { get; set; } 

    public SaveState()
    {
        Highscore = 0;
        Fish = 0;
        LastSaveTime = DateTime.Now;
        CurrentHatIndex = 0;
        UnlockedHatFlag = new int[HAT_COUNT];
        UnlockedHatFlag[0] = 1; // ✅ Mặc định mở khóa mũ đầu tiên
    }
}
