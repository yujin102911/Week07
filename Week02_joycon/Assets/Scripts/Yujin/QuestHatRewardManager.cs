using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 보상으로 줄 모자를 짝짓는 데이터 구조체
/// </summary>
[Serializable]
public struct FlagHatReward
{
    public FlagId flag;
    public GameObject hatPrefab;
}

public class QuestHatRewardManager : MonoBehaviour
{

    [Header("Flag - hat")]
    [SerializeField] private List<FlagHatReward> rewardList;
    private Dictionary<FlagId, GameObject> _rewardMap;

    private void Awake()
    {
        _rewardMap = new Dictionary<FlagId, GameObject>();
        foreach (var reward in rewardList)
        {
            if (reward.flag != FlagId.None && reward.hatPrefab != null)
            {
                if (!_rewardMap.ContainsKey(reward.flag))
                {
                    _rewardMap.Add(reward.flag, reward.hatPrefab);
                }
                else
                {
                    // 중복된 플래그가 등록되었는지 확인
                    GameLogger.Instance.LogWarning(this, $"{reward.flag}에 보상이 중복 등록");
                }
            }
        }
    }

    private void Start()
    {
        if (QuestRuntime.Instance != null)
        {
            QuestRuntime.Instance.OnFlagRaised += CheckForReward;
        }
        else
        {
            GameLogger.Instance.LogError(this, "QuestRuntime 인스턴스가 없음");
        }
    }
    private void OnDestroy()
    {
        if (QuestRuntime.Instance != null)
        {
            QuestRuntime.Instance.OnFlagRaised -= CheckForReward;
        }
    }

    private void CheckForReward(FlagId raisedFlag)
    {
        if (_rewardMap.TryGetValue(raisedFlag, out GameObject hatPrefab))
        {
            Player player = FindObjectOfType<Player>();
            Vector3 spawnPosition;
            if (player != null)
            {
                // 플레이어 위치
                spawnPosition = player.transform.position;
            }
            else
            {
                // 플레이어를 못찾으면 그냥 (0,0,0)에 생성하고 로그 남김
                GameLogger.Instance.LogWarning(this, "Player를 찾을 수 없어 (0,0,0)에 모자를 생성");
                spawnPosition = Vector3.zero;
            }
            Instantiate(hatPrefab, spawnPosition, Quaternion.identity);
            GameLogger.Instance.LogDebug(this, $"{raisedFlag} 보상: {hatPrefab.name}");
        }
    }

}
