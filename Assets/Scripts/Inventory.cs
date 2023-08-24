using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Inventory : MonoBehaviour
{
    private List<Item> HeldItems = new();

    private void OnEnable()
    {
        // Set height to the default grid's height so that a whole number of cells fit vertically.
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) { Debug.Log("Can't find UIDocument!"); return; }

        var grid = uiDocument.rootVisualElement.Q("Grid") as Grid;
        if (grid == null) { Debug.Log("Can't find grid!"); return; }

        var scrollView = uiDocument.rootVisualElement.Q("ScrollableGrid") as ScrollView;
        if (scrollView == null) { Debug.Log("Can't find scroll view!"); return; }

        // Styling takes a moment to be resolved so use a callback function to be able to access
        // valid values when available.
        grid.RegisterCallback<GeometryChangedEvent>(MakeSlotsSquareAndSetScrollHeight);


        void MakeSlotsSquareAndSetScrollHeight(GeometryChangedEvent evt)
        {
            if (float.IsNaN(grid.SlotSize))
            {
                Debug.Log("Slot size is NaN!");
                grid.UnregisterCallback<GeometryChangedEvent>(MakeSlotsSquareAndSetScrollHeight); // Just to prevent endless callback in an error.
                return;
            }

            for (int i = 0; i < grid.childCount; i++)
            {
                for (int j = 0; j < grid[i].childCount; j++)
                {
                    grid[i][j].style.height = grid.SlotSize;
                    grid[i][j].style.paddingTop = 0;
                }
            }
            grid.UnregisterCallback<GeometryChangedEvent>(MakeSlotsSquareAndSetScrollHeight);

            // With the slots now square the scroll view's height needs to be set to limit the number of elements displayed.
            // At certain resolutions and scaling a geometry changed event doesn't get called again and so a delay needs to
            // occur before the height can be set to the newly resolved height.
            // 1440p seems to be the main culprit when doing scale with screen size (but this might depend on grid settings
            // like border), but constant pixel size mostly stops the event being called. If I had to guess, because of
            // the inaccuracies of scale with screen size there are subtle changes that result in the event being called.
            scrollView.schedule.Execute(() =>
            {
                // Set size of scroll view, not entire inventory, so that padding doesn't mess with the formatting.
                scrollView.style.height = grid.resolvedStyle.height;
                grid.GenerateRows(3); // Test
            }).ExecuteLater(10);

        }
    }
}
