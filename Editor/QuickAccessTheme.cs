using UnityEditor;
using UnityEngine;

namespace UnityFavoriteTool
{
    internal sealed class QuickAccessTheme
    {
        private static readonly QuickAccessTheme s_Dark = new(
            new Color(0.22f, 0.22f, 0.22f, 1f),
            new Color(0.15f, 0.15f, 0.15f, 1f),
            new Color(0.3f, 0.3f, 0.3f, 0.4f),
            new Color(0.17f, 0.36f, 0.53f, 0.5f),
            new Color(1f, 0.85f, 0.2f),
            new Color(0.6f, 0.6f, 0.6f),
            "#888888",
            "d_");

        private static readonly QuickAccessTheme s_Light = new(
            new Color(0.76f, 0.76f, 0.76f, 1f),
            new Color(0.58f, 0.58f, 0.58f, 1f),
            new Color(0f, 0f, 0f, 0.08f),
            new Color(0.17f, 0.45f, 0.75f, 0.28f),
            new Color(0.85f, 0.58f, 0.05f),
            new Color(0.45f, 0.45f, 0.45f),
            "#666666",
            "");

        private readonly string m_IconPrefix;

        private QuickAccessTheme(
            Color headerBackground,
            Color splitter,
            Color hover,
            Color selection,
            Color favoriteTint,
            Color inactiveFavoriteTint,
            string pathColor,
            string iconPrefix)
        {
            HeaderBackground = headerBackground;
            Splitter = splitter;
            Hover = hover;
            Selection = selection;
            FavoriteTint = favoriteTint;
            InactiveFavoriteTint = inactiveFavoriteTint;
            PathColor = pathColor;
            m_IconPrefix = iconPrefix;
        }

        internal static QuickAccessTheme Current => EditorGUIUtility.isProSkin ? s_Dark : s_Light;

        internal Color HeaderBackground { get; }
        internal Color Splitter { get; }
        internal Color Hover { get; }
        internal Color Selection { get; }
        internal Color FavoriteTint { get; }
        internal Color InactiveFavoriteTint { get; }
        internal string PathColor { get; }

        internal GUIContent GetFavoriteIcon(bool isFavorite)
        {
            string iconName = isFavorite ? "Favorite Icon" : "Favorite";
            return EditorGUIUtility.IconContent(m_IconPrefix + iconName);
        }
    }
}
