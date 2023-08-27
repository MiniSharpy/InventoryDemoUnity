using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class GridElement : VisualElement
{
    public new class UxmlFactory : UxmlFactory<GridElement, UxmlTraits> { }
    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        // Editor seems to output the incorrect values and be quite finicky, especially if the attribute description name isn't
        // lower-case and hyphen-separated.
        UxmlIntAttributeDescription _columns = new() { name = "columns", defaultValue = 8 };
        UxmlIntAttributeDescription _rows = new() { name = "rows", defaultValue = 8 };
        UxmlIntAttributeDescription _gutter = new() { name = "gutter", defaultValue = 2 };
        UxmlColorAttributeDescription _gutterColour = new() { name = "gutter-colour", defaultValue = Color.black };
        UxmlColorAttributeDescription _slotColour = new() { name = "slot-colour", defaultValue = Color.grey };

        public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
        {
            get { yield break; }
        }

        public override void Init(VisualElement visualElement, IUxmlAttributes attributeBag, CreationContext creationContext)
        {
            base.Init(visualElement, attributeBag, creationContext);
            GridElement gridElement = visualElement as GridElement;

            gridElement.Clear();

            gridElement.Columns = _columns.GetValueFromBag(attributeBag, creationContext);
            gridElement.Rows = _rows.GetValueFromBag(attributeBag, creationContext);
            gridElement.Gutter = _gutter.GetValueFromBag(attributeBag, creationContext);
            gridElement.GutterColour = _gutterColour.GetValueFromBag(attributeBag, creationContext);
            gridElement.SlotColour = _slotColour.GetValueFromBag(attributeBag, creationContext);
            gridElement.CreateGUI();
        }
    }

    public int Columns { get; set; }
    public int Rows { get; set; }
    public int Gutter { get; set; }
    public Color GutterColour { get; set; }
    public Color SlotColour { get; set; }

    public float SlotSize => childCount > 0 && this[0].childCount > 0 ? this[0][0].resolvedStyle.width : float.NaN;

    public void CreateGUI()
    {
        name = "GridElement";
        style.backgroundColor = GutterColour;

        AddRows(Rows);
    }

    public void AddRows(int numberOfRows)
    {
        for (int i = 0; i < numberOfRows; i++)
        {
            VisualElement row = new();
            row.name = "Row";
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginBottom = Gutter;
            row.style.marginLeft = Gutter;

            for (int j = 0; j < Columns; j++)
            {
                VisualElement slot = new();
                slot.name = "Slot";

                slot.style.width = Length.Percent(100f / Columns);
                if (float.IsNaN(SlotSize))
                {
                    // Hack the height to be the same as the width. This is increasingly less accurate as the border increases, creating rectangular slots.
                    // This is mainly useful for a visual reference in the UI builder, the slots are made square once the width is actually resolved at
                    // runtime.
                    // https://forum.unity.com/threads/ui-builder-problems-with-scale-images.927032/#post-6067764
                    slot.style.height = 0;
                    slot.style.paddingTop = Length.Percent(100f / Columns);
                }
                else
                {
                    slot.style.height = SlotSize;
                    slot.style.paddingTop = 0;
                }

                slot.style.backgroundColor = SlotColour; // Random.ColorHSV(); // Just to make it easier to distinguish cells

                slot.style.marginRight = Gutter;

                row.Add(slot);
            }
            Add(row);
        }

        if (childCount > 0) // Ensures correctly configured even if no rows were initially created.
        {
            this[0].style.marginTop = Gutter;
        }
    }

    public VisualElement GetChild(int index)
    {
        int x = index % Columns;
        int y = index / Columns;
        return this[y][x];
    }

    public Vector2Int GetSlotIndex(VisualElement slot)
    {
        VisualElement row = slot.parent;
        VisualElement grid = slot.parent.parent;
        int y = grid.IndexOf(row);
        int x = row.IndexOf(slot);
        return new(x, y);
    }

    public void DisplayItem(Item item, int index)
    {
        VisualElement slot = GetChild(index);
        if (item == null)
        {
            slot.style.backgroundImage = null;
            return;
        }

        if (item.Icon != null)
        {
            slot.style.backgroundImage = item.Icon;
        }
        else
        {
            slot.style.backgroundImage = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/Icon.png");
        }
    }

    public void DisplayedSelected(int index)
    {
        VisualElement slot = GetChild(index);
        slot.style.borderBottomWidth = 2;
        slot.style.borderTopWidth = 2;
        slot.style.borderLeftWidth = 2;
        slot.style.borderRightWidth = 2;
        slot.style.borderBottomColor = Color.white;
        slot.style.borderTopColor = Color.white;
        slot.style.borderLeftColor = Color.white;
        slot.style.borderRightColor = Color.white;
    }
}
