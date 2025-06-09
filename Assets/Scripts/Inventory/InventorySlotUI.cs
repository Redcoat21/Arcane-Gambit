using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using Player;

public class InventoryItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private InventoryItem item;

    public Image icon;
    public TextMeshProUGUI quantityText;

    public void Setup(InventoryItem item)
    {
        this.item = item;
        icon.sprite = item.itemData.icon;
        quantityText.text = "x" + item.quantity;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        InventoryTooltip.Instance.Show(item.itemData, item.quantity);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InventoryTooltip.Instance.Hide();
    }
}
