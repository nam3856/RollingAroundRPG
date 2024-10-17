// DroppableSlot.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class DroppableSlot : MonoBehaviour, IDropHandler
{
    public EquipmentSlot slotType; // �ش� ������ ���� (��� ������ ���)
    public Image iconImage; // ���Կ� ǥ�õ� ������
    public Image highlightImage;

    private void Awake()
    {
        if (highlightImage != null)
        {
            highlightImage.gameObject.SetActive(false);
        }
    }
    // ���Կ� �������� ����� �� ȣ��
    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem draggable = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (draggable != null && draggable.item != null)
        {
            BaseItem droppedItem = draggable.item;

            // ��ӵ� �������� ��� �������̰�, ���� ������ �´��� Ȯ��
            if (droppedItem is EquipmentItem equipment)
            {
                if (equipment.slot == slotType)
                {
                    UIManager uI = FindObjectOfType<UIManager>();
                    uI.EquipItem(equipment);
                }
                else
                {
                    Debug.LogWarning($"�ش� ���Կ� ���� �ʴ� ��� ������: {equipment.itemName}");
                }
            }
            else
            {
                Debug.LogWarning("��� �������� �ƴմϴ�.");
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (highlightImage != null)
        {
            highlightImage.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (highlightImage != null)
        {
            highlightImage.gameObject.SetActive(false);
        }
    }

    private void EquipItem(EquipmentItem equipment, DraggableItem draggable)
    {
        // ������ ������ �������� �ִٸ� ������
        if (iconImage.sprite != null)
        {
            // ���� ��� �������� �κ��丮�� ��ȯ�ϰų� �ٸ� ���� ���� ����
            Debug.Log($"���� ��� ������ ������: {iconImage.sprite.name}");
        }

        // ���ο� ��� ������ ����
        iconImage.sprite = equipment.icon;
        Debug.Log($"��� ������ ����: {equipment.itemName}");

        // �巡���� ������ ���� ���� �Ǵ� �κ��丮 ������Ʈ
        draggable.item = null;
        draggable.GetComponent<Image>().sprite = null;
    }
}
