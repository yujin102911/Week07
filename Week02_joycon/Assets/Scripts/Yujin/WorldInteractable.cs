using UnityEngine;
using UnityEngine.Events;

public class WorldInteractable : MonoBehaviour
{
    #region Public Fields
    [Tooltip("Required Item Id")]
    public string requiredItemId;
    [Tooltip("if Success, will item consume")]
    public bool consumeItemOnSuccess = false; //������ ��� �� ���������� �ƴ���
    [Tooltip("SeccessEvent")]
    public UnityEvent OnInteractionSuccess;
    ///[Tooltip("���� �� ������ �̺�Ʈ")]
    ///public UnityEvent OnInteractionFail;

    #endregion

    #region Public Methods
    ///<summary>�÷��̾ ��ȣ�ۿ��� �õ��� �� ȣ��</summary>
    ///<param name="heldItemId">�÷��̾� 0�� ������ ������ ID</param>
    public bool AttemptInteraction(string heldItemId, PlayerCarrying player)
    {
        if (string.IsNullOrEmpty(requiredItemId)) //���� �ʿ��� �������� ���ٸ�
        {
            GameLogger.Instance.LogDebug(this, $"{gameObject.name}�� �Ǽ� ��ȣ�ۿ� ����");
            OnInteractionSuccess?.Invoke();
            return true;
        }

        if (!string.IsNullOrEmpty(heldItemId) && requiredItemId == heldItemId) //���� �ʿ��� �������� �ְ� �װ� ��� �ִ� �����۰� ��ġ�ϴٸ�
        {
            GameLogger.Instance.LogDebug(this, $"{gameObject.name}�� {heldItemId}�� ��ȣ�ۿ� ����");
            OnInteractionSuccess?.Invoke();
            if (consumeItemOnSuccess) //���� ��� �� �������� �Ҹ��ؾ� �Ѵٸ�
            {
                player.ConsumeItem(0); //0�� ���� ������ �Ҹ�
            }

            return true;
        }
        else //��ġ���� �ʴ´ٸ�
        {
            GameLogger.Instance.LogDebug(this, $"{gameObject.name}�� ��ȣ�ۿ� ������ {requiredItemId}�� �����ϴ�.");
            //OnIntercationFail?.Invoke();
            return false;
        }
    }
    #endregion
}
