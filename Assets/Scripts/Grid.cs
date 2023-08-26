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
        UxmlIntAttributeDescription _gutter = new() { name = "gutter", defaultValue = 2 };
        UxmlColorAttributeDescription _gutterColour = new() { name = "gutter-colour", defaultValue = Color.black };

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
            grid.Gutter = _gutter.GetValueFromBag(attributeBag, creationContext);
            grid.GutterColour = _gutterColour.GetValueFromBag(attributeBag, creationContext);
            grid.CreateGUI();
        }
    }

    public int Columns { get; set; }
    public int Rows { get; set; }
    public int Gutter { get; set; }
    public Color GutterColour { get; set; }

    public float SlotSize => childCount > 0 && this[0].childCount > 0 ? this[0][0].resolvedStyle.width : float.NaN;

    public void CreateGUI()
    {
        name = "Grid";
        style.backgroundColor = GutterColour;

        GenerateRows(Rows);
    }

    public void GenerateRows(int numberOfRows)
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

                slot.style.backgroundColor = Random.ColorHSV(); // Just to make it easier to distinguish cells.

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
}
