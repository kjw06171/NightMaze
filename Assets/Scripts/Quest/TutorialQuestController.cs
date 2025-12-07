using UnityEngine;

/// <summary>
/// 튜토리얼 전용 로직 담당:
/// - WASD 감지
/// - 촛불 토글 감지
/// - 완료 시 QuestManager에 보고
/// </summary>
public class TutorialQuestController : MonoBehaviour
{
    private const string MOVE_ID = "TUTORIAL_MOVE";
    private const string CANDLE_PICKUP_ID = "CANDLE";
    private const string CANDLE_TOGGLE_ID = "CANDLE_TOGGLE";

    void Update()
    {
        HandleMoveTutorial();
        HandleCandleToggleTutorial();
    }

    private void HandleMoveTutorial()
    {
        if (QuestManager.Instance.IsQuestDone(MOVE_ID)) return;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            QuestManager.Instance.CompleteQuest(MOVE_ID);
        }
    }

    private void HandleCandleToggleTutorial()
    {
        if (!QuestManager.Instance.IsQuestDone(CANDLE_PICKUP_ID)) return;
        if (QuestManager.Instance.IsQuestDone(CANDLE_TOGGLE_ID)) return;

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            QuestManager.Instance.CompleteQuest(CANDLE_TOGGLE_ID);
        }
    }
}
