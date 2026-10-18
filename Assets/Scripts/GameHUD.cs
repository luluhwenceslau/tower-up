using UnityEngine;

[DisallowMultipleComponent]
public class GameHUD : MonoBehaviour
{
    private float elapsedTime;
    private int stackedBlocks;
    private int perfectStreak;
    private bool isVisible = true;
    private GUIStyle timerStyle;
    private GUIStyle counterStyle;
    private GUIStyle streakStyle;
    private Sprite clockIcon;
    private Sprite blockIcon;

    public void SetProgress(float time, int blocks, int streak)
    {
        elapsedTime = time;
        stackedBlocks = blocks;
        perfectStreak = streak;
    }

    public void Hide()
    {
        isVisible = false;
    }

    private void OnGUI()
    {
        if (!isVisible)
            return;

        if (timerStyle == null)
        {
            timerStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 40,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            counterStyle = new GUIStyle(timerStyle)
            {
                alignment = TextAnchor.MiddleLeft
            };

            streakStyle = new GUIStyle(counterStyle)
            {
                alignment = TextAnchor.MiddleRight,
                fontSize = 54,
                normal = { textColor = Color.white }
            };

        }

        const float top = 18f;
        const float counterTop = 42f;
        float iconSize = 52f;
        float timerTextWidth = 180f;
        float timerGroupWidth = iconSize + 12f + timerTextWidth;
        float timerLeft = (Screen.width - timerGroupWidth) * 0.5f;

        DrawSpriteIcon(clockIcon, new Rect(timerLeft, top, iconSize, iconSize));
        GUI.Label(
            new Rect(timerLeft + iconSize + 12f, top - 4f, timerTextWidth, iconSize + 8f),
            FormatTime(elapsedTime),
            timerStyle
        );

        float counterIconSize = 44f;
        float counterGap = 6f;
        float counterRight = Screen.width - 28f;
        string counterText = ": " + stackedBlocks;
        float counterTextWidth = Mathf.Max(100f, counterStyle.CalcSize(new GUIContent(counterText)).x + 4f);
        float counterLeft = counterRight - counterTextWidth - counterGap - counterIconSize;
        GUI.Label(
            new Rect(counterLeft + counterIconSize + counterGap, counterTop - 4f, counterTextWidth, counterIconSize + 8f),
            counterText,
            counterStyle
        );
        DrawSpriteIcon(blockIcon, new Rect(counterLeft, counterTop, counterIconSize, counterIconSize));

        if (perfectStreak > 0)
        {
            float streakY = Mathf.Max(counterTop + counterIconSize + 70f, Screen.height * 0.34f);
            float streakWidth = 250f;
            GUI.Label(
                new Rect(Screen.width - streakWidth - 28f, streakY, streakWidth, 68f),
                "★  " + perfectStreak,
                streakStyle
            );
        }
    }

    public void SetClockIcon(Sprite icon)
    {
        clockIcon = icon;
    }

    public void SetBlockIcon(Sprite icon)
    {
        blockIcon = icon;
    }

    public static void DrawSpriteIcon(Sprite sprite, Rect rect)
    {
        if (sprite == null || sprite.texture == null)
            return;

        Rect spriteRect = sprite.rect;
        Rect uv = new Rect(
            spriteRect.x / sprite.texture.width,
            spriteRect.y / sprite.texture.height,
            spriteRect.width / sprite.texture.width,
            spriteRect.height / sprite.texture.height
        );
        GUI.DrawTextureWithTexCoords(rect, sprite.texture, uv, true);
    }

    private static string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
        int hours = totalSeconds / 3600;
        int minutes = totalSeconds % 3600 / 60;
        int remainingSeconds = totalSeconds % 60;

        return hours > 0
            ? string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, remainingSeconds)
            : string.Format("{0:00}:{1:00}", minutes, remainingSeconds);
    }
}
