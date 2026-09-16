using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UnityFavoriteTool
{
    public sealed partial class QuickAccessWindow
    {
        private const float k_RowHeight = 20f;
        private const float k_ToggleWidth = 20f;
        private const float k_IconSize = 16f;
        private const float k_SplitterHeight = 6f;
        private const float k_HandleWidth = 18f;
        private static readonly GUIContent s_DragHandleTooltip = new GUIContent("", "Drag to reorder favorites");

        private string m_DraggedFavorite;
        private int m_FavoriteDragControl;
        private double m_LastDragRepaint;

        private Vector2 m_FavScrollPos;
        private Vector2 m_RecentScrollPos;
        private float m_SplitRatio = 0.35f;
        private bool m_Dragging;
        private QuickAccessTheme m_Theme;
        private GUIStyle m_HeaderStyle;
        private GUIStyle m_AssetLabelStyle;

        private void OnEnable()
        {
            EnsureLoaded();
            ApplyCurrentTheme();
            InitializeSearch();
        }

        private void OnDisable()
        {
            ShutdownSearch();
            CancelFavoriteDrag();
        }
        private void OnLostFocus() => CancelFavoriteDrag();

        private void CancelFavoriteDrag()
        {
            if (m_FavoriteDragControl != 0 && GUIUtility.hotControl == m_FavoriteDragControl)
                GUIUtility.hotControl = 0;
            m_DraggedFavorite = null;
            m_FavoriteDragControl = 0;
            Repaint();
        }

        private void OnGUI()
        {
            EnsureCurrentThemeAndStyles();
            if (m_DraggedFavorite != null && !s_Favorites.Contains(m_DraggedFavorite))
                CancelFavoriteDrag();

            DrawSearchToolbar();
            float availableHeight = Mathf.Max(0, position.height - k_SearchToolbarHeight - k_SplitterHeight);
            float favoriteHeight = availableHeight * m_SplitRatio;
            float recentHeight = availableHeight - favoriteHeight;

            DrawSection("Favorites", ref m_FavScrollPos, s_Favorites, favoriteHeight, true);

            var splitterRect = GUILayoutUtility.GetRect(
                GUIContent.none,
                GUIStyle.none,
                GUILayout.Height(k_SplitterHeight));
            EditorGUI.DrawRect(splitterRect, m_Theme.Splitter);
            EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeVertical);
            HandleSplitterDrag(splitterRect);

            DrawSection("Recent", ref m_RecentScrollPos, s_Recent, recentHeight, false);
        }

        private void ApplyCurrentTheme()
        {
            m_Theme = QuickAccessTheme.Current;
            titleContent = new GUIContent("QuickAccess", m_Theme.GetFavoriteIcon(true).image);
            m_HeaderStyle = null;
            m_AssetLabelStyle = null;
        }

        private void EnsureCurrentThemeAndStyles()
        {
            if (!ReferenceEquals(m_Theme, QuickAccessTheme.Current))
                ApplyCurrentTheme();

            m_HeaderStyle ??= new GUIStyle(EditorStyles.boldLabel) { fontSize = 12 };
            m_AssetLabelStyle ??= new GUIStyle(EditorStyles.label)
            {
                richText = true,
                fontSize = 11,
                padding = new RectOffset(0, 0, 0, 0)
            };
        }

        private void DrawSection(
            string title,
            ref Vector2 scrollPosition,
            List<string> assets,
            float height,
            bool isFavoriteSection)
        {
            GUILayout.BeginVertical(GUILayout.Height(height));

            var headerRect = GUILayoutUtility.GetRect(0, 22f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(headerRect, m_Theme.HeaderBackground);
            var headerLabelRect = new Rect(
                headerRect.x + 4f,
                headerRect.y + 1f,
                headerRect.width - 8f,
                headerRect.height);
            GUI.Label(headerLabelRect, new GUIContent(title,
                isFavoriteSection && IsSearching ? "Clear search to reorder favorites" : ""), m_HeaderStyle);

            var visibleAssets = GetVisibleAssets(assets);
            if (visibleAssets.Count == 0)
            {
                GUILayout.FlexibleSpace();
                string emptyText = isFavoriteSection ? "No favorites" : "No recent access";
                if (assets.Count > 0 && IsSearching)
                    emptyText = m_SearchDirty ? "Searching..." : "No matching assets";
                EditorGUILayout.LabelField(emptyText, EditorStyles.centeredGreyMiniLabel);
                GUILayout.FlexibleSpace();
            }
            else
            {
                var viewport = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                float contentHeight = visibleAssets.Count * k_RowHeight;
                float contentWidth = Mathf.Max(0, viewport.width - (contentHeight > viewport.height ? 16f : 0f));
                var contentRect = new Rect(0, 0, contentWidth, contentHeight);
                if (isFavoriteSection && !IsSearching)
                    HandleFavoriteDrag(viewport, contentWidth, contentHeight, visibleAssets, ref scrollPosition);
                scrollPosition = GUI.BeginScrollView(viewport, scrollPosition, contentRect);
                DrawAssetList(visibleAssets, contentWidth, isFavoriteSection);
                if (isFavoriteSection && m_DraggedFavorite != null &&
                    new Rect(0, scrollPosition.y, contentWidth, viewport.height).Contains(Event.current.mousePosition))
                {
                    int slot = GetInsertionSlot(Event.current.mousePosition.y, visibleAssets.Count);
                    float lineY = Mathf.Clamp(slot * k_RowHeight, 1f, Mathf.Max(1f, contentHeight - 1f));
                    EditorGUI.DrawRect(new Rect(0, lineY - 1f, contentWidth, 2f), m_Theme.FavoriteTint);
                }
                GUI.EndScrollView();
            }

            GUILayout.EndVertical();
        }

        private void DrawAssetList(List<string> assets, float width, bool isFavoriteSection)
        {
            for (int index = 0; index < assets.Count; index++)
            {
                string assetGuid = assets[index];
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
                if (asset == null) continue;

                var rowRect = new Rect(0, index * k_RowHeight, width, k_RowHeight);
                if (rowRect.Contains(Event.current.mousePosition))
                    EditorGUI.DrawRect(rowRect, m_Theme.Hover);
                if (Selection.activeObject == asset)
                    EditorGUI.DrawRect(rowRect, m_Theme.Selection);

                float handleSpace = isFavoriteSection ? k_HandleWidth : 0f;
                if (isFavoriteSection)
                {
                    var handleRect = new Rect(rowRect.x, rowRect.y, k_HandleWidth, k_RowHeight);
                    int control = GUIUtility.GetControlID(FocusType.Passive);
                    using (new EditorGUI.DisabledScope(IsSearching))
                        DrawNativeDragHandle(handleRect, index, Selection.activeObject == asset);
                    if (IsSearching)
                        GUI.Label(handleRect, new GUIContent("", "Clear search to reorder favorites"), GUIStyle.none);
                    else
                        EditorGUIUtility.AddCursorRect(handleRect, MouseCursor.Pan);
                    if (!IsSearching && Event.current.type == EventType.MouseDown && Event.current.button == 0 &&
                        handleRect.Contains(Event.current.mousePosition))
                    {
                        m_DraggedFavorite = assetGuid;
                        m_FavoriteDragControl = control;
                        GUIUtility.hotControl = control;
                        m_LastDragRepaint = EditorApplication.timeSinceStartup;
                        Event.current.Use();
                    }
                }

                var icon = AssetDatabase.GetCachedIcon(assetPath) as Texture2D
                           ?? EditorGUIUtility.ObjectContent(asset, asset.GetType()).image;
                var iconRect = new Rect(
                    rowRect.x + handleSpace + 4f,
                    rowRect.y + (k_RowHeight - k_IconSize) * 0.5f,
                    k_IconSize,
                    k_IconSize);
                if (icon != null)
                    GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);

                float labelX = iconRect.xMax + 4f;
                float toggleSpace = k_ToggleWidth + 8f;
                float labelWidth = rowRect.xMax - labelX - toggleSpace;
                string fileName = Path.GetFileName(assetPath);
                string directory = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
                string displayText = string.IsNullOrEmpty(directory)
                    ? fileName
                    : $"{fileName}  <color={m_Theme.PathColor}>{directory}</color>";
                GUI.Label(new Rect(labelX, rowRect.y, labelWidth, k_RowHeight), displayText, m_AssetLabelStyle);

                bool isFavorite = s_Favorites.Contains(assetGuid);
                var toggleRect = new Rect(
                    rowRect.xMax - k_ToggleWidth - 4f,
                    rowRect.y + (k_RowHeight - k_IconSize) * 0.5f,
                    k_IconSize,
                    k_IconSize);
                var previousColor = GUI.color;
                GUI.color = isFavorite ? m_Theme.FavoriteTint : m_Theme.InactiveFavoriteTint;
                if (GUI.Button(toggleRect, m_Theme.GetFavoriteIcon(isFavorite), GUIStyle.none))
                    ToggleFavorite(assetGuid);
                GUI.color = previousColor;

                var clickRect = new Rect(rowRect.x + handleSpace, rowRect.y, Mathf.Max(0, rowRect.width - toggleSpace - handleSpace), rowRect.height);
                if (Event.current.type != EventType.MouseDown || Event.current.button != 0 || !clickRect.Contains(Event.current.mousePosition))
                    continue;

                if (Event.current.clickCount == 2)
                    AssetDatabase.OpenAsset(asset);
                else
                {
                    Selection.activeObject = asset;
                    EditorGUIUtility.PingObject(asset);
                }
                Event.current.Use();
            }
        }

        private void DrawNativeDragHandle(Rect rect, int index, bool selected)
        {
            // Use the same renderer as Unity's IMGUI reorderable lists instead
            // of a font glyph or a copied built-in texture/style name.
            // UnityEditorInternal is not a version-stable public API; keep the
            // dependency here so future editor API changes have one call site.
            ReorderableList.defaultBehaviours.DrawElementDraggingHandle(
                rect, index, selected, selected && focusedWindow == this, true);
            GUI.Label(rect, s_DragHandleTooltip, GUIStyle.none);
        }

        private static int GetInsertionSlot(float contentY, int count) =>
            Mathf.Clamp(Mathf.FloorToInt((contentY + k_RowHeight * 0.5f) / k_RowHeight), 0, count);

        private void HandleFavoriteDrag(Rect viewport, float contentWidth, float contentHeight,
            List<string> assets, ref Vector2 scrollPosition)
        {
            if (m_DraggedFavorite == null) return;
            var evt = Event.current;
            if (!s_Favorites.Contains(m_DraggedFavorite) || GUIUtility.hotControl != m_FavoriteDragControl ||
                (evt.type == EventType.KeyDown && evt.keyCode == KeyCode.Escape))
            {
                CancelFavoriteDrag();
                if (evt.type == EventType.KeyDown) evt.Use();
                return;
            }

            var dropRect = new Rect(viewport.x, viewport.y, contentWidth, viewport.height);
            bool inside = dropRect.Contains(evt.mousePosition);
            if (evt.type == EventType.MouseUp && evt.button == 0)
            {
                if (inside)
                {
                    int slot = GetInsertionSlot(evt.mousePosition.y - viewport.y + scrollPosition.y, assets.Count);
                    MoveFavorite(m_DraggedFavorite, slot < assets.Count ? assets[slot] : null);
                }
                CancelFavoriteDrag();
                evt.Use();
                return;
            }

            if (evt.type == EventType.Repaint)
            {
                double now = EditorApplication.timeSinceStartup;
                float elapsed = Mathf.Min(0.05f, (float)(now - m_LastDragRepaint));
                m_LastDragRepaint = now;
                if (inside)
                {
                    float edge = Mathf.Min(24f, viewport.height * 0.25f);
                    float direction = evt.mousePosition.y < viewport.yMin + edge ? -1f :
                        evt.mousePosition.y > viewport.yMax - edge ? 1f : 0f;
                    scrollPosition.y = Mathf.Clamp(scrollPosition.y + direction * 240f * elapsed,
                        0, Mathf.Max(0, contentHeight - viewport.height));
                }
            }
            Repaint();
            if (evt.type == EventType.MouseDrag) evt.Use();
        }

        private void HandleSplitterDrag(Rect splitterRect)
        {
            var currentEvent = Event.current;
            if (currentEvent.type == EventType.MouseDown && splitterRect.Contains(currentEvent.mousePosition))
            {
                m_Dragging = true;
                currentEvent.Use();
            }
            if (m_Dragging && currentEvent.type == EventType.MouseDrag)
            {
                m_SplitRatio = Mathf.Clamp((currentEvent.mousePosition.y - k_SearchToolbarHeight) /
                    Mathf.Max(1, position.height - k_SearchToolbarHeight - k_SplitterHeight), 0.1f, 0.8f);
                Repaint();
                currentEvent.Use();
            }
            if (currentEvent.type == EventType.MouseUp)
                m_Dragging = false;
        }
    }
}
