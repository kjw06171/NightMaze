using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text;
using UnityEngine.SceneManagement;

/// <summary>
/// 퀘스트 데이터 관리 + UI 담당
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("UI 연결")]
    public TextMeshProUGUI questText;

    [Header("퀘스트 표시 방식")]
    public QuestDisplayMode displayMode = QuestDisplayMode.AllAtOnce;

    [Header("전체 퀘스트 목록 (씬 구분 없이 모두 포함해야 함)")]
    public List<QuestItemData> initialQuestItems = new List<QuestItemData>();

    private Dictionary<string, bool> keyQuests = new Dictionary<string, bool>();

    private int requiredKeyCount = 0;
    private bool isQuestCompleted = false;

    public bool IsQuestCompleted => isQuestCompleted;

    // 🔥 씬 번호 보정값 (Lv_00_2에서는 5번부터 시작하려고 offset 적용)
    private int sceneQuestOffset = 0;

    private const string MOVE_TUTORIAL_ID = "TUTORIAL_MOVE";
    private const string CANDLE_PICKUP_ID = "CANDLE";
    private const string CANDLE_TOGGLE_ID = "CANDLE_TOGGLE";


    // ------------------------------------
    // 🔥 Awake: 싱글톤 + 씬 유지
    // ------------------------------------
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            SceneManager.sceneLoaded += OnSceneLoaded;

            // 첫 시작 초기화
            InitializeQuests();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }


    // ------------------------------------
    // 🔥 씬 로드 시 자동 호출
    // ------------------------------------
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetSceneOffset(scene.name);    // 🔥 씬 번호 오프셋 설정
        UpdateQuestUI();
    }


    // ------------------------------------
    // 🔥 씬에 따라 번호 오프셋 설정
    // ------------------------------------
    private void SetSceneOffset(string sceneName)
    {
        if (sceneName == "Lv_00_1")
            sceneQuestOffset = 0;  // 1번부터
        else if (sceneName == "Lv_00_2")
            sceneQuestOffset = 4;  // 5번부터 (정원에서 1~4 했으니까)
        else
            sceneQuestOffset = 0;

        Debug.Log($"[QuestManager] Scene Offset = {sceneQuestOffset}");
    }

    public void AddProgress(string questID, int amount = 1)
    {
        var quest = initialQuestItems.Find(q => q.questID == questID);
        if (quest == null)
        {
            Debug.LogError($"존재하지 않는 진행형 퀘스트: {questID}");
            return;
        }

        quest.currentCount += amount;

        if (quest.currentCount >= quest.targetCount)
        {
            quest.currentCount = quest.targetCount;
            CompleteQuest(questID);
        }

        UpdateQuestUI();
    }



    // ------------------------------------
    // 🔥 최초 퀘스트 초기화
    // ------------------------------------
    private void InitializeQuests()
    {
        keyQuests.Clear();
        requiredKeyCount = 0;

        foreach (var item in initialQuestItems)
        {
            keyQuests[item.questID] = false;

            if (item.questID != MOVE_TUTORIAL_ID &&
                item.questID != CANDLE_PICKUP_ID &&
                item.questID != CANDLE_TOGGLE_ID)
            {
                requiredKeyCount++;
            }
        }
    }


    // ------------------------------------
    // 🔥 퀘스트 완료 여부
    // ------------------------------------
    public bool IsQuestDone(string questID)
    {
        return keyQuests.ContainsKey(questID) && keyQuests[questID];
    }


    public void CompleteQuest(string questID)
    {
        if (!keyQuests.ContainsKey(questID))
        {
            Debug.LogError($"[QuestManager] 존재하지 않는 퀘스트 ID: {questID}");
            return;
        }

        if (!keyQuests[questID])
        {
            keyQuests[questID] = true;
            UpdateQuestUI();
            CheckMainQuestProgress();
        }
    }


    private void CheckMainQuestProgress()
    {
        int count = 0;

        foreach (var item in initialQuestItems)
        {
            if (item.questID == MOVE_TUTORIAL_ID ||
                item.questID == CANDLE_PICKUP_ID ||
                item.questID == CANDLE_TOGGLE_ID)
                continue;

            if (keyQuests[item.questID])
                count++;
        }

        isQuestCompleted = (count == requiredKeyCount);
    }


    // ------------------------------------
    // 🔥 UI 업데이트
    // ------------------------------------
    private void UpdateQuestUI()
    {
        if (questText == null) return;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("🔑 메인 퀘스트");

        int index = 1;

        // ==========================================================
        // 🔥 Sequential 모드 (진행형 퀘스트 지원하는 개선 버전)
        // ==========================================================
        if (displayMode == QuestDisplayMode.Sequential)
        {
            foreach (var item in initialQuestItems)
            {
                bool done = keyQuests.GetValueOrDefault(item.questID, false);

                // 진행형 퀘스트라면?
                if (item.targetCount > 1)
                {
                    // 아직 완료 안된 진행형 → 여기서 UI 출력 후 멈춤
                    if (!done)
                    {
                        sb.AppendLine($"{sceneQuestOffset + index}. {item.displayName} ({item.currentCount}/{item.targetCount})");
                        questText.text = sb.ToString();
                        return;
                    }
                }
                else
                {
                    // 일반 퀘스트
                    if (!done)
                    {
                        sb.AppendLine($"{sceneQuestOffset + index}. {item.displayName}");
                        questText.text = sb.ToString();
                        return;
                    }
                }

                index++;
            }

            // 모든 퀘스트 완료 시
            sb.AppendLine("✨ 모든 퀘스트 완료!");
            questText.text = sb.ToString();
            return;
        }

        // ==========================================================
        // 🔥 AllAtOnce 모드
        // ==========================================================
        index = 1;
        foreach (var item in initialQuestItems)
        {
            bool done = keyQuests.GetValueOrDefault(item.questID, false);

            string display;

            // 진행형 퀘스트
            if (item.targetCount > 1)
            {
                string progress = $"{item.currentCount}/{item.targetCount}";

                display = done
                    ? $"<color=#62B76B><b>{sceneQuestOffset + index}. {item.displayName} 완료 ({progress})</b></color>"
                    : $"{sceneQuestOffset + index}. {item.displayName} ({progress})";
            }
            else
            {
                // 일반 퀘스트
                display = done
                    ? $"<color=#62B76B><b>{sceneQuestOffset + index}. {item.displayName} 완료</b></color>"
                    : $"{sceneQuestOffset + index}. {item.displayName}";
            }

            sb.AppendLine(display);
            index++;
        }

        questText.text = sb.ToString();
    }


}
