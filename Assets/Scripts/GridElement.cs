using System.Collections.Generic;
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
                SlotElement slot = new(this);
                row.Add(slot);
            }
            Add(row);
        }

        // Ensures there's the top gutter. Best do every time in case this is the first time rows are added.
        if (childCount > 0)
        {
            this[0].style.marginTop = Gutter;
        }
    }

    public SlotElement GetChild(int index)
    {
        int x = index % Columns;
        int y = index / Columns;
        return (SlotElement)this[y][x]; // Can be certain of the cast.
    }

    public int GetSlotIndex(VisualElement slot)
    {
        VisualElement row = slot.parent;
        VisualElement grid = slot.parent.parent;
        int y = grid.IndexOf(row);
        int x = row.IndexOf(slot);

        return Columns * y + x;
    }
}
