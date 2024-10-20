using Newtonsoft.Json;
using System;

[Serializable]
public class ItemInstanceData
{
    public string instanceId;
    public int baseItemId;
    public int quantity;

    // 필요한 경우 추가 필드
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

    // baseItem의 ID를 반환하는 프로퍼티
    public int baseItemId => baseItem.id;
}
