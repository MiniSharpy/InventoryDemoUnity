using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Inventory : MonoBehaviour
{
    private List<Item> HeldItems = new();

    private void OnEnable()
    {
        // Set height to to the default grid's height so that a whole number of cells fit vertically.
        // Height takes a moment to be set so use a callback function.
        var uiDocument = GetComponent<UIDocument>();

        Grid grid = uiDocument.rootVisualElement.Q("Grid") as Grid;
        if (grid == null) { Debug.Log("Can't find grid!"); return; }

        var scrollView = uiDocument.rootVisualElement.Q("ScrollableGrid") as ScrollView;
        if (scrollView == null) { Debug.Log("Can't find scroll view!"); return; }

        grid.RegisterCallback<GeometryChangedEvent>(MakeSlotsActuallySquare);


        void MakeSlotsActuallySquare(GeometryChangedEvent evt)
        {
            if (float.IsNaN(grid.SlotSize))
            {
                Debug.Log("Slot size is NaN!");
                grid.UnregisterCallback<GeometryChangedEvent>(MakeSlotsActuallySquare); // Just to prevent endless callback.
                return;
            }

            for (int i = 0; i < grid.childCount; i++)
            {
                grid[i].style.height = grid.SlotSize;
                grid[i].style.paddingTop = 0;
            }

            grid.UnregisterCallback<GeometryChangedEvent>(MakeSlotsActuallySquare);
            scrollView.RegisterCallback<GeometryChangedEvent>(SetMaxHeightForScroll);
        }

        void SetMaxHeightForScroll(GeometryChangedEvent evt)
        {
            // Set size of scroll view, not entire inventory, so that padding doesn't mess with the formatting.
            scrollView.style.height = grid.resolvedStyle.height;

            grid.GenerateRows(3); // Test
            scrollView.UnregisterCallback<GeometryChangedEvent>(SetMaxHeightForScroll);
        }
    }
}
