using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class GameOverUI : MonoBehaviour
{
    private bool isVisible;
    private float finalTime;
    private int finalBlocks;
    private int finalPerfectStreak;
    private float previousBestTime;
    private int previousBestBlocks;
    private int previousBestPerfectStreak;
    private Sprite clockIcon;
    private Sprite blockIcon;
    private GUIStyle titleStyle;
    private GUIStyle outlineStyle;
    private GUIStyle resultStyle;
    private GUIStyle recordStyle;
    private GUIStyle achievementStyle;
    private GUIStyle lockedStyle;
    private GUIStyle dividerStyle;
    private GUIStyle metricStarStyle;
    private Texture2D restartButtonTexture;
    private Texture2D restartArrowTexture;
    private Texture2D panelTexture;
    private static readonly Color[] TitleColors =
    {
        new Color(1f, 0.48f, 0.50f),
        new Color(1f, 0.69f, 0.42f),
        new Color(1f, 0.83f, 0.43f),
        new Color(0.56f, 0.83f, 0.62f),
        new Color(0.46f, 0.76f, 0.96f),
        new Color(0.63f, 0.63f, 0.96f),
        new Color(0.82f, 0.59f, 0.94f),
        new Color(1f, 0.55f, 0.72f)
    };

    public void SetClockIcon(Sprite icon)
    {
        clockIcon = icon;
    }

    public void SetBlockIcon(Sprite icon)
    {
        blockIcon = icon;
    }

    public void Show(
        float elapsedTime,
        int blocks,
        int perfectStreak,
        float bestTime,
        int bestBlocks,
        int bestPerfectStreak
    )
    {
        isVisible = true;
        finalTime = elapsedTime;
        finalBlocks = blocks;
        finalPerfectStreak = perfectStreak;
        previousBestTime = bestTime;
        previousBestBlocks = bestBlocks;
        previousBestPerfectStreak = bestPerfectStreak;
    }

    private void OnGUI()
    {
        if (!isVisible)
            return;

        EnsureStyles();

        float panelWidth = Mathf.Min(Screen.width * 0.9f, 980f);
        float panelHeight = Mathf.Min(Screen.height * 0.96f, 1120f);
        Rect panel = new Rect((Screen.width - panelWidth) * 0.5f, (Screen.height - panelHeight) * 0.5f, panelWidth, panelHeight);
        GUI.DrawTexture(panel, panelTexture, ScaleMode.StretchToFill, true);

        float padding = GetPanelPadding(panelWidth);
        float gap = Mathf.Clamp(panelHeight * 0.02f, 12f, 24f);
        float bottom = panel.yMax - padding;
        float y = panel.y + padding;
        float titleHeight = Mathf.Clamp(panelHeight * 0.15f, 88f, 140f);
        titleStyle.fontSize = Mathf.RoundToInt(Mathf.Clamp(panelWidth / 10f, 48f, 78f));
        outlineStyle.fontSize = titleStyle.fontSize;
        DrawColorfulTitle("GAME OVER", y, titleHeight);
        y += titleHeight + gap;

        float metricsHeight = Mathf.Clamp(panelHeight * 0.11f, 68f, 92f);
        DrawMetricColumns(new[] { FormatTime(finalTime), finalBlocks.ToString(), finalPerfectStreak.ToString() }, y, metricsHeight, resultStyle, 48f);
        y += metricsHeight + gap * 0.65f;
        float dividerHeight = 34f;
        DrawRecordDivider(y, dividerHeight);
        y += dividerHeight + gap * 0.65f;

        float recordsHeight = Mathf.Clamp(panelHeight * 0.1f, 56f, 76f);
        DrawMetricColumns(new[]
        {
            previousBestTime > 0f ? FormatTime(previousBestTime) : "--:--",
            previousBestBlocks > 0 ? previousBestBlocks.ToString() : "--",
            previousBestPerfectStreak > 0 ? previousBestPerfectStreak.ToString() : "--"
        }, y, recordsHeight, recordStyle, 40f);
        y += recordsHeight + gap;

        float gridWidth = panelWidth - padding * 2f;
        float tileGap = Mathf.Clamp(panelWidth * 0.018f, 10f, 18f);
        float tileWidth = (gridWidth - tileGap) * 0.5f;
        float restartSize = Mathf.Clamp(panelWidth * 0.14f, 106f, 150f);
        float restartY = bottom - restartSize;
        float tileHeight = Mathf.Max(42f, (restartY - gap - y - tileGap) * 0.5f);
        for (int i = 0; i < MilestoneAnnouncer.Count; i++)
        {
            int row = i / 2;
            int column = i % 2;
            Rect tile = new Rect(panel.x + padding + column * (tileWidth + tileGap), y + row * (tileHeight + tileGap), tileWidth, tileHeight);
            DrawAchievement(tile, i);
        }

        DrawRestartButton(panel.x + (panelWidth - restartSize) * 0.5f, restartY, restartSize);
    }

    private void EnsureStyles()
    {
        if (titleStyle != null)
            return;

        int titleSize = Mathf.Clamp(Screen.width / 9, 48, 120);
        titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = titleSize,
            fontStyle = FontStyle.Bold,
            padding = new RectOffset(0, 0, 0, 0),
            normal = { textColor = Color.white }
        };
        outlineStyle = new GUIStyle(titleStyle);
        resultStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = 48,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
        recordStyle = new GUIStyle(resultStyle)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 40,
            fontStyle = FontStyle.Bold
        };
        dividerStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 30,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(1f, 0.82f, 0.36f) }
        };
        metricStarStyle = new GUIStyle(dividerStyle)
        {
            fontSize = 36,
            normal = { textColor = new Color(1f, 0.82f, 0.36f) }
        };
        restartButtonTexture = CreateCartoonButtonTexture();
        restartArrowTexture = CreateRestartArrowTexture();
        achievementStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 23,
            fontStyle = FontStyle.Bold,
            wordWrap = true,
            normal = { textColor = Color.white }
        };
        lockedStyle = new GUIStyle(achievementStyle) { fontSize = 25 };
        panelTexture = CreatePanelTexture();
    }

    private void DrawColorfulTitle(string text, float y, float height)
    {
        float totalWidth = 0f;
        for (int i = 0; i < text.Length; i++)
            totalWidth += titleStyle.CalcSize(new GUIContent(text[i].ToString())).x;

        float x = (Screen.width - totalWidth) * 0.5f;
        for (int i = 0; i < text.Length; i++)
        {
            string character = text[i].ToString();
            float width = titleStyle.CalcSize(new GUIContent(character)).x;
            Rect rect = new Rect(x, y, width, height);

            outlineStyle.normal.textColor = Color.black;
            for (int offsetX = -2; offsetX <= 2; offsetX++)
            for (int offsetY = -2; offsetY <= 2; offsetY++)
            {
                if (offsetX != 0 || offsetY != 0)
                    GUI.Label(new Rect(x + offsetX, y + offsetY, width, height), character, outlineStyle);
            }

            titleStyle.normal.textColor = TitleColors[i % TitleColors.Length];
            GUI.Label(rect, character, titleStyle);
            x += width;
        }
    }

    private void DrawMetricColumns(string[] values, float y, float rowHeight, GUIStyle style, float iconSize)
    {
        Rect panel = GetPanelRect();
        float padding = GetPanelPadding(panel.width);
        float columnWidth = (panel.width - padding * 2f) / 3f;
        for (int i = 0; i < 3; i++)
        {
            float centerX = panel.x + padding + columnWidth * (i + 0.5f);
            float iconTextGap = 5f;
            float maximumTextWidth = Mathf.Max(30f, columnWidth - iconSize - iconTextGap - 8f);
            float textWidth = Mathf.Min(maximumTextWidth, style.CalcSize(new GUIContent(values[i])).x + 8f);
            float groupWidth = iconSize + iconTextGap + textWidth;
            float left = centerX - groupWidth * 0.5f;
            Rect iconRect = new Rect(left, y + (rowHeight - iconSize) * 0.5f, iconSize, iconSize);
            if (i == 0) GameHUD.DrawSpriteIcon(clockIcon, iconRect);
            else if (i == 1) GameHUD.DrawSpriteIcon(blockIcon, iconRect);
            else GUI.Label(iconRect, "★", metricStarStyle);
            style.alignment = TextAnchor.MiddleLeft;
            GUI.Label(new Rect(left + iconSize + iconTextGap, y, textWidth, rowHeight), values[i], style);
        }
    }

    private void DrawAchievement(Rect rect, int index)
    {
        bool unlocked = MilestoneAnnouncer.IsUnlocked(index);
        achievementStyle.fontSize = Mathf.RoundToInt(Mathf.Clamp(rect.height * 0.3f, 24f, 34f));
        lockedStyle.fontSize = Mathf.RoundToInt(Mathf.Clamp(rect.height * 0.32f, 25f, 36f));
        Color old = GUI.color;
        GUI.color = unlocked ? new Color(0.23f, 0.42f, 0.31f, 0.9f) : new Color(0.11f, 0.17f, 0.24f, 0.82f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = new Color(0.94f, 0.82f, 0.54f, 0.9f);
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 2f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.yMax - 2f, rect.width, 2f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.y, 2f, rect.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.xMax - 2f, rect.y, 2f, rect.height), Texture2D.whiteTexture);
        GUI.color = Color.white;
        string content = unlocked ? "★  " + MilestoneAnnouncer.GetMessage(index) : MilestoneAnnouncer.GetFloor(index) + "  ???";
        GUI.Label(new Rect(rect.x + 5f, rect.y + 3f, rect.width - 10f, rect.height - 6f), content, unlocked ? achievementStyle : lockedStyle);
        GUI.color = old;
    }

    private Rect GetPanelRect()
    {
        float width = Mathf.Min(Screen.width * 0.9f, 980f);
        float height = Mathf.Min(Screen.height * 0.96f, 1120f);
        return new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
    }

    private static float GetPanelPadding(float panelWidth)
    {
        return Mathf.Clamp(panelWidth * 0.105f, 38f, 92f);
    }

    private static Texture2D CreatePanelTexture()
    {
        const int size = 128;
        var pixels = new Color32[size * size];
        const float radius = 18f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = Mathf.Abs(x - 63.5f) - (63.5f - radius);
            float dy = Mathf.Abs(y - 63.5f) - (63.5f - radius);
            float distance = Mathf.Sqrt(Mathf.Max(dx, 0f) * Mathf.Max(dx, 0f) + Mathf.Max(dy, 0f) * Mathf.Max(dy, 0f)) + Mathf.Min(Mathf.Max(dx, dy), 0f) - radius;
            Color color = distance > 0f ? new Color(0, 0, 0, 0) : distance > -5f ? new Color(0.28f, 0.17f, 0.12f, 0.98f) : distance > -9f ? new Color(0.93f, 0.76f, 0.49f, 0.98f) : new Color(0.035f, 0.065f, 0.10f, 0.88f);
            pixels[y * size + x] = color;
        }
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        return texture;
    }

    private void DrawRecordDivider(float y, float height)
    {
        const float lineWidth = 300f;
        const float starGap = 24f;
        const float lineHeight = 2f;
        float center = Screen.width * 0.5f;
        float lineY = y + height * 0.5f - lineHeight * 0.5f;
        float halfWidth = Mathf.Min(lineWidth, Screen.width * 0.7f) * 0.5f;
        Color previousColor = GUI.color;
        GUI.color = new Color(1f, 0.82f, 0.36f, 0.8f);
        GUI.DrawTexture(new Rect(center - halfWidth, lineY, halfWidth - starGap, lineHeight), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(center + starGap, lineY, halfWidth - starGap, lineHeight), Texture2D.whiteTexture);
        GUI.color = previousColor;
        GUI.Label(new Rect(center - 30f, y, 60f, height), "★", dividerStyle);
    }

    private void DrawRestartButton(float x, float y, float size)
    {
        Rect buttonRect = new Rect(x, y, size, size);
        bool hovered = buttonRect.Contains(Event.current.mousePosition);
        bool clicked = GUI.Button(buttonRect, GUIContent.none, GUIStyle.none);
        bool pressed = Event.current.type == EventType.MouseDown && buttonRect.Contains(Event.current.mousePosition);
        Rect drawRect = pressed ? new Rect(buttonRect.x, buttonRect.y + 3f, size, size) : buttonRect;
        Color previousColor = GUI.color;

        GUI.color = new Color(0.18f, 0.12f, 0.09f, 0.45f);
        GUI.DrawTexture(new Rect(drawRect.x, drawRect.y + 7f, size, size), restartButtonTexture);
        GUI.color = hovered ? new Color(1.12f, 1.12f, 1.12f, 1f) : Color.white;
        GUI.DrawTexture(drawRect, restartButtonTexture);
        GUI.color = previousColor;

        GUI.DrawTexture(Inset(drawRect, 25f), restartArrowTexture, ScaleMode.ScaleToFit, true);

        if (clicked)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private static Rect Inset(Rect rect, float amount)
    {
        return new Rect(rect.x + amount, rect.y + amount, rect.width - amount * 2f, rect.height - amount * 2f);
    }

    private static Texture2D CreateCartoonButtonTexture()
    {
        const int size = 128;
        const float cornerRadius = 24f;
        Color32[] pixels = new Color32[size * size];
        Color32 transparent = new Color32(0, 0, 0, 0);
        Color32 outline = new Color32(73, 43, 29, 255);
        Color32 innerFrame = new Color32(245, 218, 174, 255);
        Color32 topFill = new Color32(218, 157, 105, 255);
        Color32 bottomFill = new Color32(162, 96, 61, 255);

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = Mathf.Abs(x - (size - 1) * 0.5f) - (size * 0.5f - cornerRadius);
            float dy = Mathf.Abs(y - (size - 1) * 0.5f) - (size * 0.5f - cornerRadius);
            float outsideX = Mathf.Max(dx, 0f);
            float outsideY = Mathf.Max(dy, 0f);
            float distance = Mathf.Sqrt(outsideX * outsideX + outsideY * outsideY)
                + Mathf.Min(Mathf.Max(dx, dy), 0f) - cornerRadius;

            Color32 pixel = transparent;
            if (distance <= 0f)
            {
                if (distance > -10f)
                    pixel = outline;
                else if (distance > -18f)
                    pixel = innerFrame;
                else
                {
                    float verticalBlend = 1f - (float)y / (size - 1);
                    pixel = Color32.Lerp(topFill, bottomFill, verticalBlend);
                }
            }

            pixels[y * size + x] = pixel;
        }

        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };
        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        return texture;
    }

    private static Texture2D CreateRestartArrowTexture()
    {
        const int size = 128;
        const float center = 64f;
        const float radius = 38f;
        const float strokeRadius = 5f;
        const float startAngle = 40f;
        const float endAngle = 310f;
        var pixels = new Color32[size * size];
        Color32 clear = new Color32(255, 255, 255, 0);
        Color32 white = new Color32(255, 255, 255, 255);
        Vector2 centerPoint = new Vector2(center, center);
        float startRadians = startAngle * Mathf.Deg2Rad;
        float endRadians = endAngle * Mathf.Deg2Rad;
        Vector2 startPoint = centerPoint + new Vector2(Mathf.Cos(startRadians), Mathf.Sin(startRadians)) * radius;
        Vector2 endPoint = centerPoint + new Vector2(Mathf.Cos(endRadians), Mathf.Sin(endRadians)) * radius;
        Vector2 tangent = new Vector2(-Mathf.Sin(endRadians), Mathf.Cos(endRadians));
        Vector2 radial = new Vector2(Mathf.Cos(endRadians), Mathf.Sin(endRadians));
        Vector2 arrowTip = endPoint + tangent * 15f;
        Vector2 arrowBase = endPoint - tangent * 9f;
        Vector2 arrowSideA = arrowBase + radial * 12f;
        Vector2 arrowSideB = arrowBase - radial * 12f;

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            Vector2 point = new Vector2(x + 0.5f, y + 0.5f);
            Vector2 fromCenter = point - centerPoint;
            float angle = Mathf.Atan2(fromCenter.y, fromCenter.x) * Mathf.Rad2Deg;
            if (angle < 0f)
                angle += 360f;

            bool onArc = angle >= startAngle && angle <= endAngle;
            bool onStroke = onArc && Mathf.Abs(fromCenter.magnitude - radius) <= strokeRadius;
            bool onRoundedCap = Vector2.Distance(point, startPoint) <= strokeRadius
                || Vector2.Distance(point, endPoint) <= strokeRadius;
            bool onArrowHead = IsInsideTriangle(point, arrowTip, arrowSideA, arrowSideB);
            bool onRoundedArrowHead = Vector2.Distance(point, arrowTip) <= 2.5f
                || Vector2.Distance(point, arrowSideA) <= 2.5f
                || Vector2.Distance(point, arrowSideB) <= 2.5f;
            pixels[y * size + x] = onStroke || onRoundedCap || onArrowHead || onRoundedArrowHead ? white : clear;
        }

        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };
        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        return texture;
    }

    private static bool IsInsideTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
    {
        float first = Cross(b - a, point - a);
        float second = Cross(c - b, point - b);
        float third = Cross(a - c, point - c);
        return (first >= 0f && second >= 0f && third >= 0f)
            || (first <= 0f && second <= 0f && third <= 0f);
    }

    private static float Cross(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    private void OnDestroy()
    {
        if (restartButtonTexture != null)
            Destroy(restartButtonTexture);
        if (restartArrowTexture != null)
            Destroy(restartArrowTexture);
        if (panelTexture != null)
            Destroy(panelTexture);
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
