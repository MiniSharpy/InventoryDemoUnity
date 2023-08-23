using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Grid : VisualElement
{
    public new class UxmlFactory : UxmlFactory<Grid, UxmlTraits> { }
    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        // Editor seems to output the incorrect values and be quite finicky, especially if the attribute description name isn't
        // lower-case and hyphen-separated.
        UxmlIntAttributeDescription _columns = new() { name = "columns", defaultValue = 8 };
        UxmlIntAttributeDescription _rows = new() { name = "rows", defaultValue = 8 };
        UxmlColorAttributeDescription _slotColour = new() { name = "slot-colour", defaultValue = Color.grey };
        UxmlIntAttributeDescription _border = new() { name = "border", defaultValue = 2 }; // If this is too large, slots become rectangular.
        UxmlColorAttributeDescription _borderColour = new() { name = "border-colour", defaultValue = Color.black };
        UxmlColorAttributeDescription _backgroundColour = new() { name = "background-colour", defaultValue = Color.grey };

        public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
        {
            get { yield break; }
        }

        public override void Init(VisualElement visualElement, IUxmlAttributes attributeBag, CreationContext creationContext)
        {
            base.Init(visualElement, attributeBag, creationContext);
            Grid grid = visualElement as Grid;

            grid.Clear();

            grid.Columns = _columns.GetValueFromBag(attributeBag, creationContext);
            grid.Rows = _rows.GetValueFromBag(attributeBag, creationContext);
            grid.SlotColour = _slotColour.GetValueFromBag(attributeBag, creationContext);
            grid.Border = _border.GetValueFromBag(attributeBag, creationContext);
            grid.BorderColour = _borderColour.GetValueFromBag(attributeBag, creationContext);
            grid.BackgroundColour = _backgroundColour.GetValueFromBag(attributeBag, creationContext);
            grid.CreateGUI();
        }
    }

    public int Columns { get; set; }
    public int Rows { get; set; }
    public Color SlotColour { get; set; }
    public int Border { get; set; }
    public Color BorderColour { get; set; }
    public Color BackgroundColour { get; set; } // Useful to help prevent obvious bleed through from elements rendered underneath. More so when slots aren't randomly colours.

    public float SlotSize => childCount > 0 ? this[0].resolvedStyle.width : float.NaN;

    public void CreateGUI()
    {
        name = "Grid";
        style.flexDirection = FlexDirection.Row;
        style.flexWrap = Wrap.Wrap;
        style.borderTopWidth = Border;
        style.borderLeftWidth = Border;
        style.borderBottomColor = BorderColour;
        style.borderTopColor = BorderColour;
        style.borderLeftColor = BorderColour;
        style.borderRightColor = BorderColour;
        style.backgroundColor = BackgroundColour;

        GenerateRows(Rows);
    }

    public void GenerateRows(int numberOfRows)
    {
        int totalSlots = Columns * numberOfRows;
        for (int i = 0; i < totalSlots; i++)
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
            }

            slot.style.backgroundColor = Random.ColorHSV(); // Just to make it easier to distinguish cells.

            slot.style.borderBottomColor = BorderColour;
            slot.style.borderTopColor = BorderColour;
            slot.style.borderLeftColor = BorderColour;
            slot.style.borderRightColor = BorderColour;

            slot.style.borderTopWidth = 0;
            slot.style.borderRightWidth = Border;
            slot.style.borderBottomWidth = Border;
            slot.style.borderLeftWidth = 0;

            Add(slot);
        }
    }
}
