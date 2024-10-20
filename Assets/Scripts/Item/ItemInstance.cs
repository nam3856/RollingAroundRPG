using Newtonsoft.Json;
using System;

[Serializable]
public class ItemInstanceData
{
    public string instanceId;
    public int baseItemId;
    public int quantity;

    // �ʿ��� ��� �߰� �ʵ�
}

[Serializable]
public class ItemInstance
{
    public string instanceId;

    [JsonIgnore]
    public BaseItem baseItem;

    public int quantity;

    public ItemInstance(BaseItem baseItem, int quantity = 1)
    {
        this.instanceId = Guid.NewGuid().ToString();
        this.baseItem = baseItem;
        this.quantity = quantity;
    }

    // baseItem�� ID�� ��ȯ�ϴ� ������Ƽ
    public int baseItemId => baseItem.id;
}
