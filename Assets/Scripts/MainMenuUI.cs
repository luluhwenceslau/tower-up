using UnityEngine;

[DisallowMultipleComponent]
public class MainMenuUI : MonoBehaviour
{
    private static readonly Color[] TitleColors =
    {
        new Color(1f, 0.48f, 0.50f), new Color(1f, 0.69f, 0.42f),
        new Color(1f, 0.83f, 0.43f), new Color(0.56f, 0.83f, 0.62f),
        new Color(0.46f, 0.76f, 0.96f), new Color(0.63f, 0.63f, 0.96f),
        new Color(0.82f, 0.59f, 0.94f), new Color(1f, 0.55f, 0.72f)
    };

    private TowerManager manager;
    private Sprite[] achievementBadges;
    private Sprite unknownAchievementBadge;
    private bool visible = true;
    private int tab;
    private bool showStats;
    private Vector2 scroll;
    private GUIStyle titleStyle;
    private GUIStyle outlineStyle;
    private GUIStyle headingStyle;
    private GUIStyle bodyStyle;
    private GUIStyle detailStyle;
    private GUIStyle tabStyle;
    private Texture2D panelTexture;
    private Texture2D buttonTexture;
    private Texture2D selectedTileTexture;
    private Texture2D lockedTileTexture;
    private Texture2D unlockedTileTexture;
    private Texture2D playIconTexture;
    private Texture2D trophyIconTexture;
    private Texture2D hamburgerIconTexture;
    private Texture2D roundButtonTexture;

    public bool IsVisible => visible;
    public void Initialize(TowerManager towerManager, Sprite[] badges, Sprite unknownBadge)
    {
        manager = towerManager;
        achievementBadges = badges;
        unknownAchievementBadge = unknownBadge;
    }
    public void Show() { visible = true; }
    public void Hide() { visible = false; }

    private void OnGUI()
    {
        if (!visible) return;
        EnsureStyles();

        GUI.color = new Color(0.035f, 0.055f, 0.10f, 0.40f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float panelWidth = Mathf.Min(Screen.width * 0.9f, 980f);
        float panelHeight = Mathf.Min(Screen.height * 0.94f, 1040f);
        Rect panel = new Rect((Screen.width - panelWidth) * 0.5f, (Screen.height - panelHeight) * 0.5f, panelWidth, panelHeight);
        float padding = Mathf.Clamp(panelWidth * 0.075f, 28f, 72f);

        if (!showStats)
        {
            float titleHeight = Mathf.Clamp(Screen.height * 0.18f, 110f, 190f);
            DrawColorfulTitle("TOWER UP", Screen.height * 0.26f, titleHeight);
            DrawHome(new Rect(0f, 0f, Screen.width, Screen.height), padding);
            return;
        }

        GUI.DrawTexture(panel, panelTexture, ScaleMode.StretchToFill, true);
        float statsTitleHeight = Mathf.Clamp(panelHeight * 0.17f, 82f, 138f);
        DrawColorfulTitle("TOWER UP", panel.y + padding * 0.3f, statsTitleHeight);

        float tabsY = panel.y + padding + statsTitleHeight;
        float tabGap = Mathf.Clamp(panelWidth * 0.025f, 14f, 28f);
        float tabWidth = (panelWidth - padding * 2f - tabGap) * 0.5f;
        float tabHeight = Mathf.Clamp(panelHeight * 0.09f, 58f, 78f);
        DrawTab(new Rect(panel.x + padding, tabsY, tabWidth, tabHeight), "RECORDS", 0);
        DrawTab(new Rect(panel.x + padding + tabWidth + tabGap, tabsY, tabWidth, tabHeight), "ACHIEVEMENTS", 1);

        float playHeight = Mathf.Clamp(panelHeight * 0.12f, 76f, 106f);
        Rect playRect = new Rect(panel.center.x - Mathf.Min(panelWidth * 0.28f, 250f), panel.yMax - padding - playHeight,
            Mathf.Min(panelWidth * 0.56f, 500f), playHeight);
        Rect contentRect = new Rect(panel.x + padding, tabsY + tabHeight + tabGap,
            panelWidth - padding * 2f, playRect.y - (tabsY + tabHeight + tabGap) - tabGap);

        if (tab == 0) DrawRecords(contentRect);
        else DrawAchievements(contentRect);

        bool playHovered = playRect.Contains(Event.current.mousePosition);
        GUI.color = playHovered ? new Color(1.12f, 1.12f, 1.12f, 1f) : Color.white;
        Rect raisedPlayRect = Event.current.type == EventType.MouseDown && playHovered
            ? new Rect(playRect.x, playRect.y + 4f, playRect.width, playRect.height)
            : playRect;
        GUI.DrawTexture(new Rect(raisedPlayRect.x, raisedPlayRect.y + 7f, raisedPlayRect.width, raisedPlayRect.height), buttonTexture);
        GUI.DrawTexture(raisedPlayRect, buttonTexture);
        GUI.color = Color.white;
        GUI.Label(raisedPlayRect, "BACK", headingStyle);
        if (GUI.Button(playRect, GUIContent.none, GUIStyle.none)) showStats = false;
    }

    private void DrawHome(Rect screen, float padding)
    {
        float playSize = Mathf.Clamp(Mathf.Min(screen.width * 0.36f, screen.height * 0.25f), 132f, 220f);
        float centerY = screen.y + screen.height * 0.59f;
        Rect playRect = new Rect(screen.center.x - playSize * 0.5f, centerY - playSize * 0.5f, playSize, playSize);
        DrawIconButton(playRect, playIconTexture, () => { if (manager != null) manager.StartGame(); });

        float trophySize = Mathf.Clamp(Mathf.Min(screen.width * 0.20f, screen.height * 0.115f), 82f, 128f);
        Rect trophyRect = new Rect(screen.width - padding - trophySize, padding * 0.7f, trophySize, trophySize);
        DrawIconButton(trophyRect, trophyIconTexture, () => { showStats = true; tab = 0; });
        Rect hamburgerRect = new Rect(trophyRect.x, trophyRect.yMax + 10f, trophySize, trophySize);
        DrawIconButton(hamburgerRect, hamburgerIconTexture, () => { showStats = true; tab = 1; });
    }

    private void DrawIconButton(Rect rect, Texture2D icon, System.Action onClick)
    {
        bool hovered = rect.Contains(Event.current.mousePosition);
        GUI.color = hovered ? new Color(1.12f, 1.12f, 1.12f, 1f) : Color.white;
        Rect raised = Event.current.type == EventType.MouseDown && hovered
            ? new Rect(rect.x, rect.y + 4f, rect.width, rect.height) : rect;
        GUI.DrawTexture(new Rect(raised.x, raised.y + 7f, raised.width, raised.height), roundButtonTexture);
        GUI.DrawTexture(raised, roundButtonTexture);
        GUI.color = Color.white;
        float inset = rect.width * 0.20f;
        GUI.DrawTexture(new Rect(raised.x + inset, raised.y + inset, raised.width - inset * 2f, raised.height - inset * 2f), icon, ScaleMode.ScaleToFit, true);
        if (GUI.Button(rect, GUIContent.none, GUIStyle.none)) onClick?.Invoke();
    }

    private void DrawTab(Rect rect, string text, int index)
    {
        bool selected = tab == index;
        bool hovered = rect.Contains(Event.current.mousePosition);
        GUI.color = selected ? Color.white : hovered ? new Color(1.08f, 1.08f, 1.08f, 1f) : new Color(0.78f, 0.82f, 0.88f, 1f);
        GUI.DrawTexture(rect, selected ? selectedTileTexture : lockedTileTexture);
        GUI.color = Color.white;
        GUI.Label(rect, text, tabStyle);
        if (GUI.Button(rect, GUIContent.none, GUIStyle.none)) tab = index;
    }

    private void DrawRecords(Rect rect)
    {
        string[] labels = { "BEST HEIGHT", "BEST TIME", "BEST PERFECT STREAK", "GAMES PLAYED" };
        string[] values =
        {
            PlayerPrefs.GetInt("TowerUp_BestBlocks", 0) + " floors",
            FormatTime(PlayerPrefs.GetFloat("TowerUp_BestTime", 0f)),
            PlayerPrefs.GetInt("TowerUp_BestPerfectStreak", 0).ToString(),
            PlayerPrefs.GetInt("TowerUp_GamesPlayed", 0).ToString()
        };
        float gap = Mathf.Clamp(rect.height * 0.035f, 12f, 22f);
        float rowHeight = (rect.height - gap * 3f) / 4f;
        for (int i = 0; i < labels.Length; i++)
        {
            Rect row = new Rect(rect.x, rect.y + i * (rowHeight + gap), rect.width, rowHeight);
            GUI.DrawTexture(row, lockedTileTexture);
            float iconWidth = Mathf.Min(76f, row.height * 0.72f);
            GUIStyle recordLabel = new GUIStyle(tabStyle) { alignment = TextAnchor.MiddleCenter, fontSize = Mathf.RoundToInt(Mathf.Clamp(row.height * 0.22f, 19f, 27f)) };
            GUIStyle recordValue = new GUIStyle(headingStyle) { fontSize = Mathf.RoundToInt(Mathf.Clamp(row.height * 0.47f, 32f, 54f)) };
            GUI.Label(new Rect(row.x + 10f, row.y + 4f, row.width - 20f, row.height * 0.35f), labels[i], recordLabel);
            GUI.Label(new Rect(row.x + 10f, row.y + row.height * 0.32f, row.width - 20f, row.height * 0.62f), values[i], recordValue);
        }
    }

    private void DrawAchievements(Rect rect)
    {
        float rowHeight = Mathf.Clamp(rect.height * 0.27f, 96f, 132f);
        float rowGap = 12f;
        float fullHeight = MilestoneAnnouncer.AchievementCount * (rowHeight + rowGap);
        Rect view = new Rect(0, 0, rect.width - 14f, fullHeight);
        scroll = GUI.BeginScrollView(rect, scroll, view);
        for (int i = 0; i < MilestoneAnnouncer.AchievementCount; i++)
        {
            float y = i * (rowHeight + rowGap);
            bool unlocked = MilestoneAnnouncer.IsUnlocked(i);
            GUI.DrawTexture(new Rect(0, y, view.width, rowHeight), unlocked ? unlockedTileTexture : lockedTileTexture);

            float sidePadding = Mathf.Clamp(rowHeight * 0.22f, 18f, 30f);
            float rightWidth = Mathf.Clamp(view.width * 0.28f, 180f, 260f);
            float leftWidth = view.width - rightWidth - sidePadding * 2f;
            Sprite assignedBadge = achievementBadges != null && i < achievementBadges.Length ? achievementBadges[i] : null;
            Sprite badge = unlocked ? (assignedBadge != null ? assignedBadge : GetPlaceholderBadge())
                : (unknownAchievementBadge != null ? unknownAchievementBadge : assignedBadge ?? GetPlaceholderBadge());
            float badgeSize = badge != null ? Mathf.Min(rowHeight * 0.62f, leftWidth * 0.22f) : 0f;
            float textX = sidePadding + (badge != null ? badgeSize + 12f : 0f);
            float textWidth = leftWidth - (badge != null ? badgeSize + 12f : 0f);
            if (badge != null)
            {
                GUI.color = unlocked ? Color.white : new Color(0.58f, 0.64f, 0.72f, 0.72f);
                GameHUD.DrawSpriteIcon(badge, new Rect(sidePadding, y + (rowHeight - badgeSize) * 0.5f, badgeSize, badgeSize));
                GUI.color = Color.white;
            }
            GUIStyle achievementName = new GUIStyle(bodyStyle) { fontSize = Mathf.RoundToInt(Mathf.Clamp(rowHeight * 0.26f, 24f, 34f)) };
            GUIStyle description = new GUIStyle(detailStyle) { fontSize = Mathf.RoundToInt(Mathf.Clamp(rowHeight * 0.17f, 18f, 24f)) };
            achievementName.alignment = TextAnchor.MiddleCenter;
            description.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(textX, y + 8f, textWidth, rowHeight * 0.45f),
                (badge == null ? (unlocked ? "★  " : "◇  ") : "") + MilestoneAnnouncer.GetAchievementName(i), achievementName);
            GUI.Label(new Rect(textX + 2f, y + rowHeight * 0.48f, textWidth - 4f, rowHeight * 0.4f),
                MilestoneAnnouncer.GetAchievementDescription(i), description);

            float rightX = view.width - rightWidth - sidePadding;
            if (unlocked)
            {
                GUI.Label(new Rect(rightX, y, rightWidth, rowHeight), "UNLOCKED!", tabStyle);
            }
            else
            {
                GUIStyle progressLabel = new GUIStyle(detailStyle) { alignment = TextAnchor.MiddleRight, fontSize = Mathf.RoundToInt(Mathf.Clamp(rowHeight * 0.16f, 17f, 22f)) };
                GUI.Label(new Rect(rightX, y + 12f, rightWidth, rowHeight * 0.34f), MilestoneAnnouncer.GetProgressText(i, 0, 0), progressLabel);
                Rect bar = new Rect(rightX, y + rowHeight * 0.60f, rightWidth, 15f);
                GUI.color = new Color(0.16f, 0.20f, 0.28f, 1f);
                GUI.DrawTexture(bar, Texture2D.whiteTexture);
                GUI.color = new Color(1f, 0.77f, 0.34f, 1f);
                GUI.DrawTexture(new Rect(bar.x, bar.y, bar.width * MilestoneAnnouncer.GetProgress(i, 0, 0), bar.height), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }
        }
        GUI.EndScrollView();
    }

    private Sprite GetPlaceholderBadge()
    {
        if (achievementBadges == null) return null;
        for (int i = 0; i < achievementBadges.Length; i++)
            if (achievementBadges[i] != null) return achievementBadges[i];
        return null;
    }

    private void DrawColorfulTitle(string text, float y, float height)
    {
        titleStyle.fontSize = Mathf.RoundToInt(Mathf.Clamp(Screen.width / 6f, 64f, 138f));
        outlineStyle.fontSize = titleStyle.fontSize;
        float totalWidth = 0f;
        for (int i = 0; i < text.Length; i++) totalWidth += titleStyle.CalcSize(new GUIContent(text[i].ToString())).x;
        float x = (Screen.width - totalWidth) * 0.5f;
        for (int i = 0; i < text.Length; i++)
        {
            string character = text[i].ToString();
            float charWidth = titleStyle.CalcSize(new GUIContent(character)).x;
            Rect rect = new Rect(x, y, charWidth, height);
            outlineStyle.normal.textColor = Color.black;
            for (int dx = -2; dx <= 2; dx++)
            for (int dy = -2; dy <= 2; dy++)
                if (dx != 0 || dy != 0) GUI.Label(new Rect(rect.x + dx, rect.y + dy, rect.width, rect.height), character, outlineStyle);
            titleStyle.normal.textColor = TitleColors[i % TitleColors.Length];
            GUI.Label(rect, character, titleStyle);
            x += charWidth;
        }
    }

    private void EnsureStyles()
    {
        if (titleStyle != null) return;
        titleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, padding = new RectOffset(0, 0, 0, 0), normal = { textColor = Color.white } };
        outlineStyle = new GUIStyle(titleStyle);
        headingStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 46, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
        bodyStyle = new GUIStyle(headingStyle) { alignment = TextAnchor.MiddleLeft, normal = { textColor = Color.white } };
        detailStyle = new GUIStyle(bodyStyle) { normal = { textColor = new Color(0.82f, 0.87f, 0.96f) } };
        tabStyle = new GUIStyle(headingStyle) { fontSize = 32, normal = { textColor = new Color(1f, 0.82f, 0.45f) } };
        panelTexture = MakeRoundedTexture(128, new Color(0.035f, 0.065f, 0.10f, 0.96f), new Color(0.93f, 0.76f, 0.49f, 1f), new Color(0.28f, 0.17f, 0.12f, 1f));
        buttonTexture = MakeRoundedTexture(128, new Color(0.78f, 0.40f, 0.19f, 1f), new Color(0.95f, 0.80f, 0.55f, 1f), new Color(0.30f, 0.16f, 0.10f, 1f));
        selectedTileTexture = MakeRoundedTexture(128, new Color(0.17f, 0.30f, 0.42f, 1f), new Color(1f, 0.80f, 0.40f, 1f), new Color(0.28f, 0.17f, 0.12f, 1f));
        lockedTileTexture = MakeRoundedTexture(128, new Color(0.10f, 0.16f, 0.24f, 0.96f), new Color(0.58f, 0.48f, 0.36f, 1f), new Color(0.20f, 0.13f, 0.10f, 1f));
        unlockedTileTexture = MakeRoundedTexture(128, new Color(0.13f, 0.29f, 0.24f, 0.98f), new Color(0.92f, 0.78f, 0.46f, 1f), new Color(0.25f, 0.16f, 0.10f, 1f));
        roundButtonTexture = MakeRoundedTexture(128, new Color(0.88f, 0.57f, 0.34f, 1f), new Color(1f, 0.86f, 0.62f, 1f), new Color(0.34f, 0.20f, 0.12f, 1f));
        playIconTexture = MakePlayIcon(64);
        trophyIconTexture = MakeTrophyIcon(64);
        hamburgerIconTexture = MakeHamburgerIcon(64);
    }

    private static Texture2D MakeRoundedTexture(int size, Color fill, Color border, Color outline)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };
        Color32[] pixels = new Color32[size * size];
        float radius = size * 0.18f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = Mathf.Abs(x - (size - 1) * 0.5f) - (size * 0.5f - radius);
            float dy = Mathf.Abs(y - (size - 1) * 0.5f) - (size * 0.5f - radius);
            float distance = Mathf.Sqrt(Mathf.Max(dx, 0f) * Mathf.Max(dx, 0f) + Mathf.Max(dy, 0f) * Mathf.Max(dy, 0f))
                + Mathf.Min(Mathf.Max(dx, dy), 0f) - radius;
            Color color = distance > 0f ? new Color(0, 0, 0, 0)
                : distance > -5f ? outline
                : distance > -11f ? border
                : fill;
            pixels[y * size + x] = color;
        }
        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        return texture;
    }

    private static Texture2D MakePlayIcon(int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
        Color32[] pixels = new Color32[size * size];
        Vector2 a = new Vector2(size * 0.29f, size * 0.15f);
        Vector2 b = new Vector2(size * 0.29f, size * 0.85f);
        Vector2 c = new Vector2(size * 0.82f, size * 0.50f);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
            if (PointInTriangle(new Vector2(x, y), a, b, c)) pixels[y * size + x] = Color.white;
        texture.SetPixels32(pixels); texture.Apply(false, true); return texture;
    }

    private static Texture2D MakeTrophyIcon(int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
        Color32[] pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            bool cup = y >= 12 && y <= 35 && x >= 17 && x <= 47 && Mathf.Abs(x - 32) <= 8 + (35 - y) * 0.22f;
            bool leftHandle = x >= 8 && x <= 20 && y >= 16 && y <= 31 && (x <= 12 || y <= 19 || y >= 28);
            bool rightHandle = x >= 44 && x <= 56 && y >= 16 && y <= 31 && (x >= 52 || y <= 19 || y >= 28);
            bool stem = x >= 28 && x <= 36 && y >= 34 && y <= 45;
            bool trophyBase = x >= 20 && x <= 44 && y >= 45 && y <= 52;
            if (cup || leftHandle || rightHandle || stem || trophyBase) pixels[(size - 1 - y) * size + x] = Color.white;
        }
        texture.SetPixels32(pixels); texture.Apply(false, true); return texture;
    }

    private static Texture2D MakeHamburgerIcon(int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
        Color32[] pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            bool insideBar = x >= 12 && x <= 52 && ((y >= 15 && y <= 21) || (y >= 29 && y <= 35) || (y >= 43 && y <= 49));
            if (insideBar) pixels[y * size + x] = Color.white;
        }
        texture.SetPixels32(pixels); texture.Apply(false, true); return texture;
    }

    private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = (p.x - b.x) * (a.y - b.y) - (a.x - b.x) * (p.y - b.y);
        float d2 = (p.x - c.x) * (b.y - c.y) - (b.x - c.x) * (p.y - c.y);
        float d3 = (p.x - a.x) * (c.y - a.y) - (c.x - a.x) * (p.y - a.y);
        bool hasNeg = d1 < 0f || d2 < 0f || d3 < 0f;
        bool hasPos = d1 > 0f || d2 > 0f || d3 > 0f;
        return !(hasNeg && hasPos);
    }

    private static string FormatTime(float time)
    {
        int seconds = Mathf.FloorToInt(time);
        return string.Format("{0:00}:{1:00}", seconds / 60, seconds % 60);
    }

    private void OnDestroy()
    {
        if (panelTexture != null) Destroy(panelTexture);
        if (buttonTexture != null) Destroy(buttonTexture);
        if (selectedTileTexture != null) Destroy(selectedTileTexture);
        if (lockedTileTexture != null) Destroy(lockedTileTexture);
        if (unlockedTileTexture != null) Destroy(unlockedTileTexture);
        if (roundButtonTexture != null) Destroy(roundButtonTexture);
        if (playIconTexture != null) Destroy(playIconTexture);
        if (trophyIconTexture != null) Destroy(trophyIconTexture);
        if (hamburgerIconTexture != null) Destroy(hamburgerIconTexture);
    }
}
