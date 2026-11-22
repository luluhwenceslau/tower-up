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
    private Texture2D homeIconTexture;
    private Texture2D gamepadIconTexture;

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

        if (!showStats)
        {
            float titleHeight = Mathf.Clamp(Screen.height * 0.18f, 110f, 190f);
            DrawColorfulTitle("TOWER UP", Screen.height * 0.26f, titleHeight);
            DrawHome(new Rect(0f, 0f, Screen.width, Screen.height), Mathf.Clamp(Screen.width * 0.075f, 22f, 72f));
            return;
        }

        if (tab == 1)
            DrawAchievementsFullScreen();
        else
            DrawRecordsFullScreen();
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

    private void DrawRecordsFullScreen()
    {
        float margin = Mathf.Clamp(Screen.width * 0.055f, 18f, 56f);
        float titleHeight = Mathf.Clamp(Screen.height * 0.16f, 104f, 168f);
        DrawColorfulTitle("TOWER UP", Screen.height * 0.015f, titleHeight);

        float backHeight = Mathf.Clamp(Mathf.Min(Screen.width * 0.9f, 980f) * 0.14f, 106f, 150f);
        float availableFrameHeight = Screen.height - titleHeight - backHeight - margin * 2f;
        float frameHeight = Screen.height > Screen.width
            ? Mathf.Min(availableFrameHeight, Screen.width * 1.05f)
            : availableFrameHeight;
        Rect frame = new Rect(margin, titleHeight + margin * 0.75f, Screen.width - margin * 2f, frameHeight);
        DrawSimplePanel(frame);

        float inset = Mathf.Clamp(frame.width * 0.055f, 14f, 40f);
        float gap = Mathf.Clamp(frame.width * 0.035f, 10f, 24f);
        float cellWidth = (frame.width - inset * 2f - gap) * 0.5f;
        float cellHeight = Mathf.Min((frame.height - inset * 2f - gap) * 0.5f, cellWidth * 0.82f);
        string[] values =
        {
            PlayerPrefs.GetInt("TowerUp_BestBlocks", 0).ToString(),
            FormatTime(PlayerPrefs.GetFloat("TowerUp_BestTime", 0f)),
            PlayerPrefs.GetInt("TowerUp_BestPerfectStreak", 0).ToString(),
            PlayerPrefs.GetInt("TowerUp_GamesPlayed", 0).ToString()
        };

        for (int i = 0; i < values.Length; i++)
        {
            int column = i % 2;
            int row = i / 2;
            Rect card = new Rect(frame.x + inset + column * (cellWidth + gap), frame.y + inset + row * (cellHeight + gap), cellWidth, cellHeight);
            DrawSimpleCard(card);
            float iconSize = Mathf.Clamp(Mathf.Min(card.width * 0.34f, card.height * 0.40f), 40f, 92f);
            Rect iconRect = new Rect(card.center.x - iconSize * 0.5f, card.y + card.height * 0.11f, iconSize, iconSize);
            DrawRecordIcon(i, iconRect);

            GUIStyle valueStyle = new GUIStyle(headingStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.RoundToInt(Mathf.Clamp(Mathf.Min(card.height * 0.34f, card.width * 0.25f), 28f, 58f)),
                normal = { textColor = Color.white }
            };
            Rect valueRect = new Rect(card.x + 8f, card.y + card.height * 0.58f, card.width - 16f, card.height * 0.34f);
            float valueTextWidth = valueStyle.CalcSize(new GUIContent(values[i])).x;
            if (valueTextWidth > valueRect.width) valueStyle.fontSize = Mathf.Max(22, Mathf.RoundToInt(valueStyle.fontSize * valueRect.width / valueTextWidth));
            GUI.Label(valueRect, values[i], valueStyle);
        }

        float homeSize = Mathf.Clamp(Mathf.Min(Screen.width * 0.9f, 980f) * 0.14f, 106f, 150f);
        Rect homeRect = new Rect((Screen.width - homeSize) * 0.5f, Screen.height - homeSize - margin * 0.3f, homeSize, homeSize);
        DrawHomeNavigationButton(homeRect);
        if (GUI.Button(homeRect, GUIContent.none, GUIStyle.none)) showStats = false;
    }

    private void DrawRecordIcon(int index, Rect rect)
    {
        if (index == 0)
        {
            SpriteRenderer blockRenderer = manager != null && manager.blockPrefab != null ? manager.blockPrefab.GetComponent<SpriteRenderer>() : null;
            GameHUD.DrawSpriteIcon(blockRenderer != null ? blockRenderer.sprite : null, rect);
        }
        else if (index == 1)
        {
            GameHUD.DrawSpriteIcon(manager != null ? manager.clockIcon : null, rect);
        }
        else if (index == 2)
        {
            GUIStyle star = new GUIStyle(headingStyle) { fontSize = Mathf.RoundToInt(rect.height * 0.9f), normal = { textColor = Color.white } };
            GUI.Label(rect, "★", star);
        }
        else
        {
            GUI.DrawTexture(rect, gamepadIconTexture, ScaleMode.ScaleToFit, true);
        }
    }

    private static void DrawSimplePanel(Rect rect)
    {
        Color previous = GUI.color;
        GUI.color = new Color(0.035f, 0.065f, 0.095f, 0.92f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = new Color(0.58f, 0.68f, 0.76f, 0.78f);
        float thickness = Mathf.Clamp(rect.width * 0.003f, 1.5f, 3f);
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, thickness), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.y, thickness, rect.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), Texture2D.whiteTexture);
        GUI.color = previous;
    }

    private static void DrawSimpleCard(Rect rect)
    {
        Color previous = GUI.color;
        GUI.color = new Color(0.075f, 0.12f, 0.16f, 0.98f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = new Color(0.34f, 0.44f, 0.51f, 0.9f);
        float thickness = Mathf.Clamp(rect.width * 0.006f, 1f, 2f);
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, thickness), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.y, thickness, rect.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), Texture2D.whiteTexture);
        GUI.color = previous;
    }

    private void DrawSimpleButton(Rect rect, string label)
    {
        DrawSimpleCard(rect);
        GUIStyle style = new GUIStyle(headingStyle)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.RoundToInt(Mathf.Clamp(rect.height * 0.40f, 24f, 42f)),
            normal = { textColor = Color.white }
        };
        GUI.Label(rect, label, style);
    }

    private void DrawHomeNavigationButton(Rect rect)
    {
        bool hovered = rect.Contains(Event.current.mousePosition);
        bool pressed = Event.current.type == EventType.MouseDown && hovered;
        Rect drawRect = pressed ? new Rect(rect.x, rect.y + 3f, rect.width, rect.height) : rect;
        GUI.color = new Color(0.18f, 0.12f, 0.09f, 0.45f);
        GUI.DrawTexture(new Rect(drawRect.x, drawRect.y + 7f, rect.width, rect.height), roundButtonTexture);
        GUI.color = hovered ? Color.white : Color.white;
        GUI.DrawTexture(drawRect, roundButtonTexture);
        GUI.color = Color.white;
        float inset = rect.width * 0.22f;
        GUI.DrawTexture(new Rect(drawRect.x + inset, drawRect.y + inset, rect.width - inset * 2f, rect.height - inset * 2f), homeIconTexture, ScaleMode.ScaleToFit, true);
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
            GUIStyle recordLabel = new GUIStyle(tabStyle) { alignment = TextAnchor.MiddleCenter, fontSize = Mathf.RoundToInt(Mathf.Clamp(row.height * 0.30f, 22f, 34f)) };
            GUIStyle recordValue = new GUIStyle(headingStyle) { fontSize = Mathf.RoundToInt(Mathf.Clamp(row.height * 0.55f, 32f, 62f)) };
            Rect labelRect = new Rect(row.x + 10f, row.y + 2f, row.width - 20f, row.height * 0.39f);
            Rect valueRect = new Rect(row.x + 10f, row.y + row.height * 0.39f, row.width - 20f, row.height * 0.59f);
            float labelWidth = recordLabel.CalcSize(new GUIContent(labels[i])).x;
            if (labelWidth > labelRect.width) recordLabel.fontSize = Mathf.Max(18, Mathf.RoundToInt(recordLabel.fontSize * labelRect.width / labelWidth));
            float valueWidth = recordValue.CalcSize(new GUIContent(values[i])).x;
            if (valueWidth > valueRect.width) recordValue.fontSize = Mathf.Max(22, Mathf.RoundToInt(recordValue.fontSize * valueRect.width / valueWidth));
            GUI.Label(labelRect, labels[i], recordLabel);
            GUI.Label(valueRect, values[i], recordValue);
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

    private void DrawAchievementsFullScreen()
    {
        GUIStyle header = new GUIStyle(titleStyle)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.RoundToInt(Mathf.Clamp(Screen.width / 17f, 34f, 62f)),
            normal = { textColor = Color.white }
        };
        float margin = Mathf.Clamp(Screen.width * 0.045f, 18f, 54f);
        float headerHeight = Mathf.Clamp(Screen.height * 0.12f, 64f, 104f);
        float backHeight = Mathf.Clamp(Mathf.Min(Screen.width * 0.9f, 980f) * 0.14f, 106f, 150f);
        GUI.Label(new Rect(margin, margin * 0.55f, Screen.width - margin * 2f, headerHeight), "ACHIEVEMENTS", header);

        Rect listRect = new Rect(margin, margin * 0.55f + headerHeight, Screen.width - margin * 2f,
            Screen.height - (margin * 1.1f + headerHeight + backHeight + margin));
        float rowGap = Mathf.Clamp(listRect.height * 0.018f, 7f, 14f);
        float rowHeight = Mathf.Clamp((listRect.height - rowGap * (MilestoneAnnouncer.AchievementCount - 1)) / MilestoneAnnouncer.AchievementCount, 72f, 108f);
        float contentHeight = MilestoneAnnouncer.AchievementCount * rowHeight + (MilestoneAnnouncer.AchievementCount - 1) * rowGap;
        Rect view = new Rect(0f, 0f, listRect.width - 16f, contentHeight);
        scroll = GUI.BeginScrollView(listRect, scroll, view);

        for (int i = 0; i < MilestoneAnnouncer.AchievementCount; i++)
        {
            float y = i * (rowHeight + rowGap);
            bool unlocked = MilestoneAnnouncer.IsUnlocked(i);
            Sprite assignedBadge = achievementBadges != null && i < achievementBadges.Length ? achievementBadges[i] : null;
            Sprite badge = unlocked ? (assignedBadge != null ? assignedBadge : GetPlaceholderBadge()) : unknownAchievementBadge;
            Rect row = new Rect(0f, y, view.width, rowHeight);
            DrawSimpleCard(row);

            float badgeSize = Mathf.Min(rowHeight * 0.88f, 104f);
            float side = Mathf.Clamp(rowHeight * 0.15f, 14f, 24f);
            Rect badgeHolder = new Rect(side, y + (rowHeight - badgeSize) * 0.5f, badgeSize, badgeSize);
            Color previous = GUI.color;
            GUI.color = new Color(0.12f, 0.18f, 0.23f, 1f);
            GUI.DrawTexture(badgeHolder, Texture2D.whiteTexture);
            GUI.color = new Color(0.38f, 0.47f, 0.54f, 1f);
            float edge = 1.5f;
            GUI.DrawTexture(new Rect(badgeHolder.x, badgeHolder.y, badgeHolder.width, edge), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(badgeHolder.x, badgeHolder.yMax - edge, badgeHolder.width, edge), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(badgeHolder.x, badgeHolder.y, edge, badgeHolder.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(badgeHolder.xMax - edge, badgeHolder.y, edge, badgeHolder.height), Texture2D.whiteTexture);
            GUI.color = previous;
            if (badge != null)
            {
                float imageInset = Mathf.Max(3f, badgeSize * 0.06f);
                GUI.color = Color.white;
                GameHUD.DrawSpriteIcon(badge, new Rect(badgeHolder.x + imageInset, badgeHolder.y + imageInset,
                    badgeSize - imageInset * 2f, badgeSize - imageInset * 2f));
                GUI.color = Color.white;
            }

            float textX = badgeHolder.xMax + Mathf.Clamp(rowHeight * 0.18f, 16f, 26f);
            float textWidth = view.width - textX - side;
            string rowTitle = unlocked ? MilestoneAnnouncer.GetAchievementName(i) : MilestoneAnnouncer.GetTargetText(i);
            GUIStyle mainText = new GUIStyle(bodyStyle)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = Mathf.RoundToInt(Mathf.Clamp(rowHeight * 0.40f, 24f, 42f)),
                normal = { textColor = Color.white }
            };
            Rect mainRect = new Rect(textX, y + rowHeight * 0.08f, textWidth, rowHeight * (unlocked ? 0.50f : 0.60f));
            float measuredTitleWidth = mainText.CalcSize(new GUIContent(rowTitle)).x;
            if (measuredTitleWidth > mainRect.width) mainText.fontSize = Mathf.Max(21, Mathf.RoundToInt(mainText.fontSize * mainRect.width / measuredTitleWidth));
            GUI.Label(mainRect, rowTitle, mainText);

            if (!unlocked)
            {
                GUIStyle progress = new GUIStyle(detailStyle)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = Mathf.RoundToInt(Mathf.Clamp(rowHeight * 0.25f, 19f, 28f)),
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = Color.white }
                };
                GUI.Label(new Rect(textX, y + rowHeight * 0.60f, textWidth, rowHeight * 0.30f),
                    MilestoneAnnouncer.GetCurrentProgressText(i, 0, 0), progress);
            }
            else
            {
                string description = MilestoneAnnouncer.GetAchievementDescription(i);
                GUIStyle subtitle = new GUIStyle(detailStyle)
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = Mathf.RoundToInt(Mathf.Clamp(rowHeight * 0.21f, 17f, 23f)),
                    normal = { textColor = new Color(0.84f, 0.89f, 0.94f) }
                };
                Rect subtitleRect = new Rect(textX, y + rowHeight * 0.56f, textWidth, rowHeight * 0.34f);
                float subtitleWidth = subtitle.CalcSize(new GUIContent(description)).x;
                if (subtitleWidth > subtitleRect.width) subtitle.fontSize = Mathf.Max(16, Mathf.RoundToInt(subtitle.fontSize * subtitleRect.width / subtitleWidth));
                GUI.Label(subtitleRect, description, subtitle);
            }
        }
        GUI.EndScrollView();

        float homeSize = Mathf.Clamp(Mathf.Min(Screen.width * 0.9f, 980f) * 0.14f, 106f, 150f);
        Rect homeRect = new Rect((Screen.width - homeSize) * 0.5f, Screen.height - homeSize - margin * 0.35f, homeSize, homeSize);
        DrawHomeNavigationButton(homeRect);
        if (GUI.Button(homeRect, GUIContent.none, GUIStyle.none)) showStats = false;
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
            outlineStyle.hover.textColor = Color.black;
            outlineStyle.active.textColor = Color.black;
            outlineStyle.focused.textColor = Color.black;
            outlineStyle.hover.textColor = Color.black;
            outlineStyle.active.textColor = Color.black;
            outlineStyle.focused.textColor = Color.black;
            for (int dx = -2; dx <= 2; dx++)
            for (int dy = -2; dy <= 2; dy++)
                if (dx != 0 || dy != 0) GUI.Label(new Rect(rect.x + dx, rect.y + dy, rect.width, rect.height), character, outlineStyle);
            Color letterColor = TitleColors[i % TitleColors.Length];
            titleStyle.normal.textColor = letterColor;
            titleStyle.hover.textColor = letterColor;
            titleStyle.active.textColor = letterColor;
            titleStyle.focused.textColor = letterColor;
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
        homeIconTexture = MakeHomeIcon(64);
        gamepadIconTexture = MakeGamepadIcon(64);
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

    private static Texture2D MakeHomeIcon(int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
        Color32[] pixels = new Color32[size * size];
        Vector2 a = new Vector2(size * 0.12f, size * 0.48f);
        Vector2 b = new Vector2(size * 0.50f, size * 0.88f);
        Vector2 c = new Vector2(size * 0.88f, size * 0.48f);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            bool roof = PointInTriangle(new Vector2(x, y), a, b, c);
            bool body = x >= size * 0.22f && x <= size * 0.78f && y >= size * 0.18f && y <= size * 0.55f;
            bool door = x >= size * 0.43f && x <= size * 0.57f && y >= size * 0.18f && y <= size * 0.39f;
            if ((roof || body) && !door) pixels[y * size + x] = Color.white;
        }
        texture.SetPixels32(pixels); texture.Apply(false, true); return texture;
    }

    private static Texture2D MakeGamepadIcon(int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
        Color32[] pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            bool body = x >= 14 && x <= 50 && y >= 20 && y <= 43;
            bool leftGrip = x >= 10 && x <= 23 && y >= 34 && y <= 51;
            bool rightGrip = x >= 41 && x <= 54 && y >= 34 && y <= 51;
            bool dpad = (x >= 18 && x <= 30 && y >= 29 && y <= 33) || (x >= 22 && x <= 26 && y >= 25 && y <= 37);
            bool buttons = (x == 43 || x == 50) && (y == 30 || y == 36);
            int pixel = y * size + x;
            if (body || leftGrip || rightGrip) pixels[pixel] = Color.white;
            if (dpad || buttons) pixels[pixel] = new Color32(0, 0, 0, 0);
        }
        texture.SetPixels32(pixels); texture.Apply(false, true); return texture;
    }

    private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float first = (p.x - b.x) * (a.y - b.y) - (a.x - b.x) * (p.y - b.y);
        float second = (p.x - c.x) * (b.y - c.y) - (b.x - c.x) * (p.y - c.y);
        float third = (p.x - a.x) * (c.y - a.y) - (c.x - a.x) * (p.y - a.y);
        bool negative = first < 0f || second < 0f || third < 0f;
        bool positive = first > 0f || second > 0f || third > 0f;
        return !(negative && positive);
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
        if (homeIconTexture != null) Destroy(homeIconTexture);
        if (gamepadIconTexture != null) Destroy(gamepadIconTexture);
        if (hamburgerIconTexture != null) Destroy(hamburgerIconTexture);
    }
}
