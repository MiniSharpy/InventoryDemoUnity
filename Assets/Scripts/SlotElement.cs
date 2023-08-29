using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// This primarily exist to hold helper methods and to allow checking if valid element.
/// </summary>
public class SlotElement : VisualElement
{
    public SlotElement(GridElement grid) : base()
    {
        name = "Slot";
        style.width = Length.Percent(100f / grid.Columns);
        if (float.IsNaN(grid.SlotSize))
        {
            // Hack the height to be the same as the width. This is increasingly less accurate as the border increases, creating rectangular slots.
            // This is mainly useful for a visual reference in the UI builder, the slots are made square once the width is actually resolved at
            // runtime.
            // https://forum.unity.com/threads/ui-builder-problems-with-scale-images.927032/#post-6067764
            style.height = 0;
            style.paddingTop = Length.Percent(100f / grid.Columns);
        }
        else
        {
            style.height = grid.SlotSize;
            style.paddingTop = 0;
        }

        style.backgroundColor = grid.SlotColour; // Random.ColorHSV(); // Just to make it easier to distinguish cells
        style.marginRight = grid.Gutter;
    }

    public void DisplayItem(Item item)
    {
        if (item == null) // Clear icon.
        {
            style.backgroundImage = null;
            return;
        }

        if (item.Icon == null) // Use fallback icon.
        {
            style.backgroundImage = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/Icon.png");
            return;

        }

        style.backgroundImage = item.Icon;
    }

    public void ShowSelectionBorder()
    {
        int borderPx = 2;
        style.borderBottomColor = Color.white;
        style.borderTopColor = Color.white;
        style.borderLeftColor = Color.white;
        style.borderRightColor = Color.white;
        style.borderBottomWidth = borderPx;
        style.borderTopWidth = borderPx;
        style.borderLeftWidth = borderPx;
        style.borderRightWidth = borderPx;
    }

    public void HideSelectionBorder()
    {
        style.borderBottomWidth = 0;
        style.borderTopWidth = 0;
        style.borderLeftWidth = 0;
        style.borderRightWidth = 0;
    }
}
