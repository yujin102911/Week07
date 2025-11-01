using UnityEngine;

[DisallowMultipleComponent]
public sealed class Deliverable2D : MonoBehaviour
{
    [SerializeField] string receiverId = "NPC_A"; // Interactable Id�� �����ϰ�
    [SerializeField] string itemId = "Key_A";
    [SerializeField] bool consumeOnDelivery = true;

    public string ReceiverId => receiverId;
    public string ItemId => itemId;
    public bool Consume => consumeOnDelivery;
}