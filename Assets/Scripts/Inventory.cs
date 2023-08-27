using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Inventory : MonoBehaviour
{
    [field: SerializeField]
    public List<Item> Items { get; private set; }

    private VisualElement ItemBeingMoved { get; set; }
    private VisualElement Root { get; set; }
    private GridElement GridElement { get; set; }
    private VisualElement MoveElement { get; set; }


    // Height should only be set once, and before any extra slots besides the default have been added to ensure scroll height
    // is roughly what's expected and fits a whole number of cells vertically.
    void Awake()
    {
        MoveElement = new VisualElement();
        MoveElement.style.position = Position.Absolute;
        MoveElement.style.display = DisplayStyle.None;
        MoveElement.pickingMode = PickingMode.Ignore;

        // TODO: To implement multiple inventories this should grab the specific part of the UI Document that it needs and then get specific elements
        // to prevent conflicts between similarly named elements.
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) { Debug.LogError("Can't find UIDocument!"); return; }
        Root = uiDocument.rootVisualElement;
        uiDocument.rootVisualElement.Add(MoveElement);
        Root.RegisterCallback<MouseMoveEvent>(OnMouseMoveEvent);
        //Root.RegisterCallback<MouseUpEvent>(OnMouseUpEvent);

        GridElement = uiDocument.rootVisualElement.Q("GridElement") as GridElement;
        if (GridElement == null) { Debug.LogError("Can't find gridElement!"); return; }
        int neededPadded = GridElement.Columns * GridElement.Rows - Items.Count;
        Items.AddRange(Enumerable.Repeat<Item>(null, neededPadded));

        var scrollView = uiDocument.rootVisualElement.Q("ScrollableGrid") as ScrollView;
        if (scrollView == null) { Debug.LogError("Can't find scroll view!"); return; }

        // Styling takes a moment to be resolved so use a callback function to be able to access
        // valid values when available.
        GridElement.RegisterCallback<GeometryChangedEvent>(MakeSlotsSquareAndSetScrollHeight);

        void MakeSlotsSquareAndSetScrollHeight(GeometryChangedEvent evt)
        {
            if (float.IsNaN(GridElement.SlotSize))
            {
                Debug.Log("Slot size is NaN!");
                GridElement.UnregisterCallback<GeometryChangedEvent>(MakeSlotsSquareAndSetScrollHeight); // Just to prevent endless callback in an error.
                return;
            }

            for (int i = 0; i < GridElement.childCount; i++)
            {
                for (int j = 0; j < GridElement[i].childCount; j++)
                {
                    VisualElement slot = GridElement[i][j];
                    slot.style.height = GridElement.SlotSize;
                    slot.style.paddingTop = 0;
                    slot.RegisterCallback<MouseDownEvent>(OnMouseDownEvent);
                    slot.RegisterCallback<MouseUpEvent>(OnMouseUpEvent);
                }
            }

            GridElement.UnregisterCallback<GeometryChangedEvent>(MakeSlotsSquareAndSetScrollHeight);

            // With the slots now square the scroll view's height needs to be set to limit the number of elements displayed.
            // At certain resolutions and scaling a geometry changed event doesn't get called again and so a delay needs to
            // occur before the height can be set to the newly resolved height.
            // 1440p seems to be the main culprit when doing scale with screen size (but this might depend on gridElement settings
            // like border), but constant pixel size mostly stops the event being called. If I had to guess, because of
            // the inaccuracies of scale with screen size there are subtle changes that result in the event being called.
            scrollView.schedule.Execute(() =>
            {
                // Set size of scroll view, not entire inventory, so that padding doesn't mess with the formatting.
                // Also relies on the gridElement containing all its own guttering and what not to perfectly fit vertically with scroll.
                scrollView.style.height = GridElement.resolvedStyle.height;
                UpdateUI();
            }).ExecuteLater(1);
        }
    }

    void UpdateUI()
    {
        if (GridElement == null) { Debug.LogError("Can't find grid element!"); return; }

        // Generate needed rows before attempting to index into them.
        // Should rows be added when needed, or shortly before all slots are used?
        int currentNumberOfSlots = GridElement.childCount * GridElement.Columns;
        if (Items.Count > currentNumberOfSlots)
        {
            int requiredRows = (int)MathF.Ceiling((Items.Count - currentNumberOfSlots) / (float)GridElement.Columns);
            GridElement.AddRows(requiredRows);
            int neededPadded = GridElement.Columns * requiredRows;
            Items.AddRange(Enumerable.Repeat<Item>(null, neededPadded));
        }

        // Update slots.
        for (int i = 0; i < Items.Count; i++)
        {
            Item item = Items[i];
            GridElement.DisplayItem(item, i);
        }
        //Debug.Log("UI Updated!");
    }

    private void OnMouseDownEvent(MouseDownEvent evt)
    {
        VisualElement slot = evt.target as VisualElement;
        MoveElement.style.display = DisplayStyle.Flex;
        MoveElement.style.width = slot.resolvedStyle.width;
        MoveElement.style.height = slot.resolvedStyle.height;
        MoveElement.style.backgroundImage = slot.resolvedStyle.backgroundImage;
        ItemBeingMoved = slot;
    }
    private void OnMouseMoveEvent(MouseMoveEvent evt)
    {
        Vector2 mousePosition = new Vector2(evt.mousePosition.x, evt.mousePosition.y);
        Vector2 mousePositionRelativeToPanel = RuntimePanelUtils.ScreenToPanel(Root.panel, mousePosition);
        MoveElement.style.top = mousePositionRelativeToPanel.y;
        MoveElement.style.left = mousePositionRelativeToPanel.x;
    }

    private void OnMouseUpEvent(MouseUpEvent evt)
    {
        Debug.Log("Triggered!");
        if (ItemBeingMoved == null) { return; }

        MoveElement.style.display = DisplayStyle.None;

        // Not a slot
        VisualElement target = evt.target as VisualElement;
        if (target.name != "Slot")
        {
            ItemBeingMoved = null;
            return;
        }

        VisualElement slot = evt.target as VisualElement;
        Vector2Int sourceIndex = GridElement.GetSlotIndex(ItemBeingMoved);
        Vector2Int targetIndex = GridElement.GetSlotIndex(slot);
        TransferObject(GridElement.Columns * sourceIndex.y + sourceIndex.x, GridElement.Columns * targetIndex.y + targetIndex.x);
        UpdateUI();
        ItemBeingMoved = null;
    }

    private void TransferObject(int source, int target)
    {
        if (target >= Items.Count) { Debug.LogError($"Target index {target} cannot be greater than available item slots!"); return; }
        (Items[source], Items[target]) = (Items[target], Items[source]);
        //Debug.Log($"Transfer {source} to {target}");
    }


}
