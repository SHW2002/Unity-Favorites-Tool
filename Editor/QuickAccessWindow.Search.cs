using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UnityFavoriteTool
{
    public sealed partial class QuickAccessWindow
    {
        private const float k_SearchToolbarHeight = 22f;
        [SerializeField] private string m_SearchText = "";
        private SearchField m_SearchField;
        private HashSet<string> m_SearchMatches;
        private bool m_SearchDirty;
        private double m_SearchDue;
        private static readonly Regex s_SearchTokens = new Regex("(?:[^\\s\"]+|\"[^\"]*\")+", RegexOptions.Compiled);
        private static readonly string[] s_TypeNames =
            { "All Types", "Scenes", "Prefabs", "Materials", "Textures", "Scripts", "ScriptableObjects", "Audio", "Animations" };
        private static readonly string[] s_TypeFilters =
            { "", "SceneAsset", "Prefab", "Material", "Texture", "MonoScript", "ScriptableObject", "AudioClip", "AnimationClip" };

        private bool IsSearching => !string.IsNullOrWhiteSpace(m_SearchText);

        private void InitializeSearch()
        {
            m_SearchField = new SearchField();
            EditorApplication.update += UpdateSearch;
            EditorApplication.projectChanged += InvalidateSearch;
            InvalidateSearch();
        }

        private void ShutdownSearch()
        {
            EditorApplication.update -= UpdateSearch;
            EditorApplication.projectChanged -= InvalidateSearch;
        }

        private void InvalidateSearch()
        {
            m_SearchDirty = true;
            m_SearchDue = EditorApplication.timeSinceStartup + 0.15;
        }

        private void SetSearchText(string value)
        {
            if (m_SearchText == value) return;
            m_SearchText = value;
            m_SearchMatches = null;
            m_FavScrollPos = m_RecentScrollPos = Vector2.zero;
            CancelFavoriteDrag();
            InvalidateSearch();
        }

        private void UpdateSearch()
        {
            if (!m_SearchDirty || EditorApplication.timeSinceStartup < m_SearchDue) return;
            m_SearchDirty = false;
            m_SearchMatches = IsSearching
                ? new HashSet<string>(AssetDatabase.FindAssets(m_SearchText))
                : null;
            Repaint();
        }

        private List<string> GetVisibleAssets(List<string> assets)
        {
            return assets.FindAll(guid => (!IsSearching || (m_SearchMatches != null && m_SearchMatches.Contains(guid))) &&
                AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid)) != null);
        }

        // Preserve quoted name tokens while replacing only explicit type filters.
        private static string WithTypeFilter(string query, string type)
        {
            var remaining = new List<string>();
            bool skipTypeValue = false;
            foreach (Match match in s_SearchTokens.Matches(query))
            {
                string token = match.Value;
                if (skipTypeValue) { skipTypeValue = false; continue; }
                if (token.StartsWith("t:", StringComparison.OrdinalIgnoreCase))
                {
                    skipTypeValue = token.Length == 2;
                    continue;
                }
                remaining.Add(token);
            }
            if (!string.IsNullOrEmpty(type)) remaining.Add("t:" + type);
            return string.Join(" ", remaining);
        }

        private void DrawSearchToolbar()
        {
            var toolbar = GUILayoutUtility.GetRect(0, k_SearchToolbarHeight, GUILayout.ExpandWidth(true));
            GUI.Box(toolbar, GUIContent.none, EditorStyles.toolbar);
            const float menuWidth = 64f;
            var searchRect = new Rect(toolbar.x + 4, toolbar.y + 2, Mathf.Max(1, toolbar.width - menuWidth - 10), 18);
            SetSearchText(m_SearchField.OnToolbarGUI(searchRect, m_SearchText));
            var menuRect = new Rect(toolbar.xMax - menuWidth - 2, toolbar.y + 1, menuWidth, 20);
            if (GUI.Button(menuRect, new GUIContent("Type", "Filter favorites and recent assets by type"), EditorStyles.toolbarDropDown))
            {
                var menu = new GenericMenu();
                for (int i = 0; i < s_TypeNames.Length; i++)
                {
                    string type = s_TypeFilters[i];
                    menu.AddItem(new GUIContent(s_TypeNames[i]), false,
                        () => SetSearchText(WithTypeFilter(m_SearchText, type)));
                }
                menu.DropDown(menuRect);
            }
        }
    }
}
