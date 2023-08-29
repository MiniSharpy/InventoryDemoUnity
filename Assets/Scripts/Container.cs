using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Container : MonoBehaviour
{
    /*STATIC*/
    // These static fields wouldn't work for split screen, though it could be designed around by only
    // allowing once person to alter inventories at once, or having one fullscreen inventory they both
    // have to interact with simultaneously.
    private static int SelectedItem { get; set; }
    private static VisualElement MoveElement { get; set; }
    private static Container SourceContainer { get; set; }

    static void TransferItemBetweenContainers(Container sourceContainer, int sourceIndex, Container targetContainer, int targetIndex)
    {
        if (targetIndex >= targetContainer.Items.Count) { Debug.LogError($"Target index {targetIndex} cannot be greater than available item slots!"); return; }
        (sourceContainer.Items[sourceIndex], targetContainer.Items[targetIndex]) = (targetContainer.Items[targetIndex], sourceContainer.Items[sourceIndex]);
    }

    /*INSTANCE*/
    [field: SerializeField]
    private VisualTreeAsset ContainerUI { get; set; }

    [field: SerializeField]
    public List<Item> Items { get; private set; }
    private UIDocument UIDocument { get; set; }
    private GridElement GridElement { get; set; }

    // Height should only be set once, and before any extra slots besides the default have been added to ensure scroll height
    // is roughly what's expected and fits a whole number of cells vertically.
    void Awake()
    {
        // Check if move element exists, otherwise create it.
        if (MoveElement == null)
        {
            MoveElement = new();
        }
        MoveElement.style.position = Position.Absolute;
        MoveElement.style.display = DisplayStyle.None;
        MoveElement.pickingMode = PickingMode.Ignore;

        // Getting the UI document at runtime makes setting up a container easier as you do not need to ensure the UIDocument is correctly serialised.
        // This can be heavy though as it requires searching all objects for the necessary one.
        // Works under the assumption there's only one, which seems to be the recommended way to do UI, but isn't necessarily always true.
        UIDocument = FindObjectOfType<UIDocument>();

        if (UIDocument == null) { Debug.LogError("Can't find UIDocument!"); return; }
        UIDocument.rootVisualElement.Add(MoveElement);
        UIDocument.rootVisualElement.RegisterCallback<MouseUpEvent>(EndDrag); // This is needed as a fall back case for invalid drops. Could use different evt.target to mean different behaviour. E.G. If other UI elements reset, but otherwise drop into world?

        // Create the container UI.
        VisualElement containerElement = ContainerUI.Instantiate();
        UIDocument.rootVisualElement.Q("PlayerUI").Add(containerElement);

        // Get elements that need to be altered.
        GridElement = containerElement.Q("GridElement") as GridElement;
        if (GridElement == null) { Debug.LogError("Can't find gridElement!"); return; }
        int neededPadded = GridElement.Columns * GridElement.Rows - Items.Count;
        Items.AddRange(Enumerable.Repeat<Item>(null, neededPadded));

        var scrollView = containerElement.Q("ScrollableGrid") as ScrollView;
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
                    slot.RegisterCallback<MouseDownEvent>(BeginDrag);
                    slot.RegisterCallback<MouseUpEvent>(EndDrag);
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
            GridElement.GetChild(i).DisplayItem(item);
        }
    }

    private void BeginDrag(MouseDownEvent evt)
    {
        SlotElement slot = evt.target as SlotElement;

        if (slot == null) { return; }

        // Update move element fields
        MoveElement.style.display = DisplayStyle.Flex;
        MoveElement.style.width = slot.resolvedStyle.width;
        MoveElement.style.height = slot.resolvedStyle.height;
        MoveElement.style.backgroundImage = slot.resolvedStyle.backgroundImage;

        // Mark slot as selected
        slot.ShowSelectionBorder();

        // Record selected item and container.
        SelectedItem = GridElement.GetSlotIndex(slot);
        SourceContainer = this;

        // Want mouse movement all over screen, not just container UI.
        UIDocument.rootVisualElement.RegisterCallback<MouseMoveEvent>(Drag);
        MoveElement.style.left = evt.mousePosition.x;
        MoveElement.style.top = evt.mousePosition.y;
    }
    private void Drag(MouseMoveEvent evt)
    {
        MoveElement.style.left = evt.mousePosition.x;
        MoveElement.style.top = evt.mousePosition.y;
    }
    private void EndDrag(MouseUpEvent evt)
    {
        UIDocument.rootVisualElement.UnregisterCallback<MouseMoveEvent>(Drag);
        MoveElement.style.display = DisplayStyle.None;

        SlotElement slot = evt.target as SlotElement;

        // Exit early if dropping on invalid VisualElement.
        if (slot == null) { goto EarlyExit; }

        // Exit early if invalid source container.
        if (SourceContainer == null) { goto EarlyExit; }

        // Exit early if invalid index.
        if (SelectedItem < 0 || SelectedItem >= SourceContainer.Items.Count) { goto EarlyExit; }

        // Move items around.
        int sourceIndex = SelectedItem;
        int targetIndex = GridElement.GetSlotIndex(slot);
        TransferItemBetweenContainers(SourceContainer, sourceIndex, this, targetIndex);
        SourceContainer.GridElement.GetChild(SelectedItem).HideSelectionBorder();
        SourceContainer.UpdateUI(); // Need to update both otherwise the icon remains behind.
        UpdateUI();
        SelectedItem = -1;
        SourceContainer = null;

    EarlyExit:
        if (SourceContainer != null && (SelectedItem >= 0 && SelectedItem < SourceContainer.Items.Count))
        {
            SourceContainer.GridElement.GetChild(SelectedItem).HideSelectionBorder();
            SourceContainer.UpdateUI(); // Need to update both otherwise the icon remains behind.
        }
        SourceContainer = null;
        SelectedItem = -1;
        return;
    }

    private void TransferObject(int sourceIndex, int targetIndex)
    {
        if (targetIndex >= Items.Count) { Debug.LogError($"Target index {targetIndex} cannot be greater than available item slots!"); return; }
        (Items[sourceIndex], Items[targetIndex]) = (Items[targetIndex], Items[sourceIndex]);
    }


}
