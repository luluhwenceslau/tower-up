using UnityEngine;

[DisallowMultipleComponent]
public class GameHUD : MonoBehaviour
{
    private float elapsedTime;
    private int stackedBlocks;
    private bool isVisible = true;
    private GUIStyle timerStyle;
    private GUIStyle counterStyle;
    private Texture2D clockIcon;
    private Texture2D blockIcon;

    public void SetProgress(float time, int blocks)
    {
        elapsedTime = time;
        stackedBlocks = blocks;
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

            clockIcon = CreateClockIcon();
            blockIcon = CreateBlockIcon();
        }

        const float top = 18f;
        float iconSize = 52f;
        float timerTextWidth = 180f;
        float timerGroupWidth = iconSize + 12f + timerTextWidth;
        float timerLeft = (Screen.width - timerGroupWidth) * 0.5f;

        GUI.DrawTexture(new Rect(timerLeft, top, iconSize, iconSize), clockIcon);
        GUI.Label(
            new Rect(timerLeft + iconSize + 12f, top - 4f, timerTextWidth, iconSize + 8f),
            FormatTime(elapsedTime),
            timerStyle
        );

        float counterRight = Screen.width - 25f;
        float counterTextWidth = 95f;
        float counterLeft = counterRight - counterTextWidth - 12f - iconSize;
        GUI.Label(
            new Rect(counterLeft, top - 4f, counterTextWidth, iconSize + 8f),
            stackedBlocks.ToString("00"),
            counterStyle
        );
        GUI.DrawTexture(new Rect(counterRight - iconSize, top, iconSize, iconSize), blockIcon);
    }

    private void OnDestroy()
    {
        if (clockIcon != null)
            Destroy(clockIcon);
        if (blockIcon != null)
            Destroy(blockIcon);
    }

    private static Texture2D CreateClockIcon()
    {
        const int size = 32;
        var icon = CreateTransparentIcon(size);
        Color32 outline = new Color32(18, 45, 72, 255);
        Color32 face = new Color32(255, 255, 255, 255);

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float distance = Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f));
            if (distance <= 14f)
                icon.SetPixel(x, y, outline);
            if (distance <= 11f)
                icon.SetPixel(x, y, face);
        }

        DrawLine(icon, 16, 16, 16, 8, outline);
        DrawLine(icon, 16, 16, 22, 19, outline);
        icon.Apply();
        return icon;
    }

    private static Texture2D CreateBlockIcon()
    {
        const int size = 32;
        var icon = CreateTransparentIcon(size);
        Color32 outline = new Color32(18, 45, 72, 255);
        Color32 face = new Color32(255, 255, 255, 255);

        for (int y = 6; y <= 25; y++)
        for (int x = 6; x <= 25; x++)
        {
            bool border = x == 6 || x == 25 || y == 6 || y == 25;
            icon.SetPixel(x, y, border ? outline : face);
        }

        DrawLine(icon, 16, 7, 16, 24, outline);
        DrawLine(icon, 7, 16, 24, 16, outline);
        icon.Apply();
        return icon;
    }

    private static Texture2D CreateTransparentIcon(int size)
    {
        var icon = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave
        };
        var clearPixels = new Color32[size * size];
        icon.SetPixels32(clearPixels);
        return icon;
    }

    private static void DrawLine(Texture2D texture, int x0, int y0, int x1, int y1, Color color)
    {
        int steps = Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0));
        for (int i = 0; i <= steps; i++)
        {
            float t = steps == 0 ? 0f : (float)i / steps;
            int x = Mathf.RoundToInt(Mathf.Lerp(x0, x1, t));
            int y = Mathf.RoundToInt(Mathf.Lerp(y0, y1, t));
            texture.SetPixel(x, y, color);
        }
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
