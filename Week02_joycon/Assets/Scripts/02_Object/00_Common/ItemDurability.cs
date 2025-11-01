using UnityEngine;

public class ItemDurability : MonoBehaviour
{
    [Tooltip("이 아이템의 최대 사용 횟수입니다.")]
    [SerializeField] private int maxUses = 1;

    private int _currentUses;

    private void Awake()
    {
        _currentUses = maxUses;
    }

    /// <summary>
    /// 아이템을 1회 사용하고, 모두 소모되었는지 여부를 반환합니다.
    /// </summary>
    /// <returns>사용 후 아이템이 모두 소모되었으면 true, 아니면 false를 반환합니다.</returns>
    public bool Use()
    {
        if (_currentUses <= 0) return true; // 이미 0이면 true 반환

        _currentUses--;
        Debug.Log($"아이템 {gameObject.name} 사용. 남은 횟수: {_currentUses}/{maxUses}");

        return _currentUses <= 0;
    }
}