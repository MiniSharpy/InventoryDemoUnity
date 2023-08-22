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
        uiDocument.rootVisualElement.RegisterCallback<GeometryChangedEvent>(GeometryChangedCallback);

        void GeometryChangedCallback(GeometryChangedEvent evt)
        {
            var uiDocument = GetComponent<UIDocument>();
            Grid grid = uiDocument.rootVisualElement.Q("Grid") as Grid;
            if (grid == null) { Debug.Log("Can't find grid!"); return; }

            // Set size of scroll view, not entire thing, so that padding doesn't mess with the formatting.
            var scrollView = uiDocument.rootVisualElement.Q("ScrollableGrid") as ScrollView;
            if (scrollView == null) { Debug.Log("Can't find scroll view!"); return; }
            scrollView.style.height = grid.resolvedStyle.height;

            grid.GenerateRows(3);
            uiDocument.rootVisualElement.UnregisterCallback<GeometryChangedEvent>(GeometryChangedCallback);
        }
    }
}
