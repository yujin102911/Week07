using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class HatData
{
    public HatType hatType;
    public Sprite hatSprite;
    public GameObject hatPrefab;
}

public class PlayerHatController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer hatSpriteRenderer;
    [SerializeField] private HatType currentHatType;
    [SerializeField] private List<HatData> hatDatas;

    //새 모자를 획득했을 때 발생시킬 이벤트
    public event Action<HatType> OnHatAcquired;

    //쓴 적 있는 모자를 보관하기 위함
    private HashSet<HatType> usedHats = new HashSet<HatType>();
    private const string HatUsedPlayerPrefsKey = "HatUsed_";

    public HatType CurrentHatType
    {
        get { return currentHatType; }
        private set { currentHatType = value; }
    }

    private void Awake()
    {
        LoadUsedHats();

        if (currentHatType != HatType.None)
        {
            usedHats.Add(currentHatType);
        }
    }

    public void ChangeHat(HatType hatType)
    {
        if (currentHatType != HatType.None)
        {
            Vector2 dropPosition = (Vector2)transform.position + new Vector2(Player.GetFaceDir() * -1.0f, 0.5f);
            Instantiate(GetHatData(currentHatType).hatPrefab, dropPosition, Quaternion.identity);
        }

        currentHatType = hatType;
        hatSpriteRenderer.sprite = GetHatData(hatType).hatSprite;

        if (hatType != HatType.None && !usedHats.Contains(hatType))
        {
            usedHats.Add(hatType);
            string key = HatUsedPlayerPrefsKey + hatType.ToString();
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();

            //새 모자를 획득했다고 이벤트 발행
            OnHatAcquired?.Invoke(hatType);
        }
    }

    private HatData GetHatData(HatType hatType) => hatDatas.Find(h => h.hatType == hatType);

    public bool IsHatUsed(HatType hatType)
    {
        return usedHats.Contains(hatType);
    }

    /// <summary>
    /// PlayerPrefs에서 획득한 모자 목록을 불러와 usedHats에
    /// </summary>
    private void LoadUsedHats()
    {
        usedHats.Clear();
        foreach (HatType hatType in (HatType[])Enum.GetValues(typeof(HatType)))
        {
            if (hatType == HatType.None) continue;

            string key = HatUsedPlayerPrefsKey + hatType.ToString();
            if (PlayerPrefs.GetInt(key, 0) == 1)
            {
                usedHats.Add(hatType);
            }
        }
    }

    ///// <summary>
    ///// 테스트용: 'R' 키를 누르면 초기화
    ///// </summary>
    //private void Update()
    //{
    //    // R 키가 눌렸는지 확인
    //    if (Input.GetKeyDown(KeyCode.R))
    //    {
    //        ResetUsedHats();
    //    }
    //}

    /// <summary>
    /// 모자 획득 기록을 모두 초기화합니다. (PlayerPrefs, 메모리, UI 포함)
    /// </summary>
    public void ResetUsedHats()
    {
        Debug.Log("모자 획득 기록을 초기화");

        // PlayerPrefs에 저장된 모든 HatType 키 삭제
        foreach (HatType hatType in (HatType[])Enum.GetValues(typeof(HatType)))
        {
            if (hatType == HatType.None) continue;
            string key = HatUsedPlayerPrefsKey + hatType.ToString();
            PlayerPrefs.DeleteKey(key);
        }

        PlayerPrefs.Save();

        // 현재 게임 메모리에 있는 HashSet 비우기
        usedHats.Clear();

        // 인스펙터에 설정된 시작 모자는 획득한 것으로 처리
        if (currentHatType != HatType.None)
        {
            usedHats.Add(currentHatType);
        }

        // 씬에 있는 모든 HatUIElement를 찾아서 알파 값을 즉시 갱신
        HatUIElement[] allHatUI = FindObjectsOfType<HatUIElement>();
        foreach (HatUIElement uiElement in allHatUI)
        {
            uiElement.UpdateAlpha(); // 각 UI 요소의 알파 값 업데이트 실행
        }

        Debug.Log("모자 기록 초기화 및 UI 갱신 완료!");
    }
}