using UnityEngine;
using System;

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
    private Action pauseAction;

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

    public void Show() { isVisible = true; }
    public void SetPauseAction(Action action) { pauseAction = action; }

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

        timerStyle.fontSize = Mathf.RoundToInt(Mathf.Clamp(Screen.width / 10f, 30f, 52f));
        counterStyle.fontSize = timerStyle.fontSize;
        const float top = 8f;
        float pauseSize = Mathf.Clamp(Screen.width * 0.15f, 56f, 70f);
        float iconSize = Mathf.Clamp(Screen.width * 0.12f, 36f, 50f);
        Rect pauseRect = new Rect(16f, top + (iconSize - pauseSize) * 0.5f, pauseSize, pauseSize);
        float pauseBarWidth = pauseSize * 0.16f;
        float pauseBarHeight = pauseSize * 0.48f;
        float pauseGap = pauseSize * 0.12f;
        float pauseBarX = pauseRect.center.x - pauseGap * 0.5f - pauseBarWidth;
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(pauseBarX, pauseRect.center.y - pauseBarHeight * 0.5f, pauseBarWidth, pauseBarHeight), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(pauseBarX + pauseBarWidth + pauseGap, pauseRect.center.y - pauseBarHeight * 0.5f, pauseBarWidth, pauseBarHeight), Texture2D.whiteTexture);
        if (GUI.Button(pauseRect, GUIContent.none, GUIStyle.none)) pauseAction?.Invoke();

        float timerTextWidth = Mathf.Clamp(Screen.width * 0.27f, 90f, 190f);
        float timerLeft = pauseRect.xMax + 18f;

        DrawSpriteIcon(clockIcon, new Rect(timerLeft, top, iconSize, iconSize));
        GUI.Label(
            new Rect(timerLeft + iconSize + 7f, top - 4f, timerTextWidth, iconSize + 8f),
            FormatTime(elapsedTime),
            timerStyle
        );

        float counterIconSize = Mathf.Clamp(Screen.width * 0.105f, 32f, 44f);
        float counterGap = 5f;
        float counterRight = Screen.width - 16f;
        string counterText = ": " + stackedBlocks;
        float counterTextWidth = Mathf.Max(48f, counterStyle.CalcSize(new GUIContent(counterText)).x + 4f);
        float counterLeft = counterRight - counterTextWidth - counterGap - counterIconSize;
        GUI.Label(
            new Rect(counterLeft + counterIconSize + counterGap, top - 4f, counterTextWidth, counterIconSize + 8f),
            counterText,
            counterStyle
        );
        DrawSpriteIcon(blockIcon, new Rect(counterLeft, top + (iconSize - counterIconSize) * 0.5f, counterIconSize, counterIconSize));

        if (perfectStreak > 0)
        {
            float streakY = Mathf.Max(top + counterIconSize + 70f, Screen.height * 0.34f);
            float streakWidth = Mathf.Clamp(Screen.width * 0.72f, 220f, 360f);
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
