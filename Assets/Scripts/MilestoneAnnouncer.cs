using UnityEngine;

[DisallowMultipleComponent]
public class MilestoneAnnouncer : MonoBehaviour
{
    private static readonly int[] Floors = { 25, 50, 75, 120, 300 };
    private static readonly string[] FloorNames =
    {
        "Above the Clouds!", "Air Traffic!", "Edge of the Atmosphere!", "To Infinity!", "Tower Legend"
    };
    private static readonly string[] AchievementNames =
    {
        "Above the Clouds!", "Air Traffic!", "Edge of the Atmosphere!", "To Infinity!", "Tower Legend",
        "Perfect Placement", "On a Roll", "Persistent Builder"
    };
    private static readonly string[] AchievementDescriptions =
    {
        "Reach 25 floors", "Reach 50 floors", "Reach 75 floors", "Reach space at 120 floors", "Reach 300 floors",
        "Make your first perfect placement", "Make 5 perfect placements in a row", "Play 10 games"
    };
    private const string Key = "TowerUp_Achievement_";
    private const float MessageDuration = 3.2f;
    private const float FadeInDuration = 0.3f;
    private const float FadeOutStart = 2.55f;
    private int nextMilestoneIndex;
    private string activeMessage;
    private float shownAt;
    private bool isVisible;
    private GUIStyle messageStyle;
    private Vector2 messageScreenPosition;

    private void Awake() => messageScreenPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

    public static int Count => Floors.Length;
    public static int AchievementCount => AchievementNames.Length;
    public static int GetFloor(int index) => Floors[index];
    public static string GetMessage(int index) => FloorNames[index];
    public static string GetAchievementName(int index) => AchievementNames[index];
    public static string GetAchievementDescription(int index) => AchievementDescriptions[index];
    private static string GetKey(int index) => Key + (index < Floors.Length ? Floors[index] : index == 5 ? "PerfectPlacement" : index == 6 ? "OnARoll" : "PersistentBuilder");
    public static bool IsUnlocked(int index) => PlayerPrefs.GetInt(GetKey(index), 0) != 0;

    public static float GetProgress(int index, int floors, int streak)
    {
        if (index < Floors.Length)
        {
            int target = Floors[index];
            return Mathf.Clamp01((float)PlayerPrefs.GetInt("TowerUp_BestBlocks", floors) / target);
        }
        if (index == 5) return IsUnlocked(index) ? 1f : 0f;
        if (index == 6) return Mathf.Clamp01((float)PlayerPrefs.GetInt("TowerUp_BestPerfectStreak", streak) / 5f);
        return Mathf.Clamp01(PlayerPrefs.GetInt("TowerUp_GamesPlayed", 0) / 10f);
    }

    public static string GetProgressText(int index, int floors, int streak)
    {
        if (index < Floors.Length)
        {
            int current = Mathf.Min(PlayerPrefs.GetInt("TowerUp_BestBlocks", floors), Floors[index]);
            return current + " / " + Floors[index] + " floors";
        }
        if (index == 5) return IsUnlocked(index) ? "1 / 1" : "0 / 1";
        if (index == 6) return Mathf.Min(PlayerPrefs.GetInt("TowerUp_BestPerfectStreak", streak), 5) + " / 5";
        return Mathf.Min(PlayerPrefs.GetInt("TowerUp_GamesPlayed", 0), 10) + " / 10 games";
    }

    public static void Unlock(int index)
    {
        PlayerPrefs.SetInt(GetKey(index), 1);
        PlayerPrefs.Save();
    }

    public static void RecordPerfect(int streak)
    {
        Unlock(5);
        if (streak >= 5) Unlock(6);
    }

    public void ResetRun() { nextMilestoneIndex = 0; isVisible = false; }

    public static int GetNextAchievement(int floors, int streak)
    {
        int bestHeight = Mathf.Max(PlayerPrefs.GetInt("TowerUp_BestBlocks", 0), floors);
        for (int i = 0; i < Floors.Length; i++)
        {
            if (bestHeight >= Floors[i])
            {
                if (!IsUnlocked(i)) Unlock(i);
            }
            else
            {
                return i;
            }
        }

        if (PlayerPrefs.GetInt("TowerUp_BestPerfectStreak", streak) >= 5 && !IsUnlocked(6)) Unlock(6);
        if (PlayerPrefs.GetInt("TowerUp_GamesPlayed", 0) >= 10 && !IsUnlocked(7)) Unlock(7);
        if (!IsUnlocked(5)) return 5;
        if (!IsUnlocked(6)) return 6;
        if (!IsUnlocked(7)) return 7;
        return -1;
    }

    public static bool IsFloorAchievement(int index) => index >= 0 && index < Floors.Length;
    public static string GetTargetText(int index)
    {
        if (IsFloorAchievement(index)) return Floors[index] + " FLOORS";
        if (index == 6) return "5 PERFECT PLACEMENTS";
        if (index == 7) return "10 GAMES";
        return "1 PERFECT PLACEMENT";
    }

    public void UpdateProgress(int landedBlocks)
    {
        if (isVisible) return;
        while (nextMilestoneIndex < Floors.Length && landedBlocks >= Floors[nextMilestoneIndex])
        {
            int floor = Floors[nextMilestoneIndex];
            Unlock(nextMilestoneIndex);
            activeMessage = "🚀 " + FloorNames[nextMilestoneIndex];
            nextMilestoneIndex++;
            shownAt = Time.unscaledTime;
            isVisible = true;
            return;
        }
    }

    public void Hide() => isVisible = false;
    public void SetDisplayPosition(Camera camera, Vector3 worldPosition)
    {
        if (camera == null) return;
        Vector3 screen = camera.WorldToScreenPoint(worldPosition);
        messageScreenPosition = new Vector2(screen.x, Screen.height - screen.y);
    }

    private void OnGUI()
    {
        if (!isVisible) return;
        float elapsed = Time.unscaledTime - shownAt;
        if (elapsed >= MessageDuration) { isVisible = false; return; }
        if (messageStyle == null) messageStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        float fadeIn = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / FadeInDuration));
        float fadeOut = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((elapsed - FadeOutStart) / (MessageDuration - FadeOutStart)));
        float alpha = Mathf.Min(fadeIn, fadeOut);
        messageStyle.fontSize = Mathf.RoundToInt(Mathf.Clamp(Screen.width / 15f, 48f, 86f));
        Rect rect = new Rect(12f, Mathf.Clamp(messageScreenPosition.y - 70f, 8f, Screen.height - 148f), Screen.width - 24f, 140f);
        Color old = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, alpha * 0.8f);
        GUI.Label(new Rect(rect.x + 3f, rect.y + 3f, rect.width, rect.height), activeMessage, messageStyle);
        GUI.color = new Color(1f, 1f, 1f, alpha);
        GUI.Label(rect, activeMessage, messageStyle);
        GUI.color = old;
    }
}
