using UnityEngine;

public class DialogueReceiver : MonoBehaviour
{
    public DialogueSO dialogueData;

    // 타임라인에서 신호가 오면 대화를 시작
    public void OnDialogueSignal()
    {
        // 타임라인에서 신호가 오면 대화를 시작
        TimelineDialogueManager.Instance.StartDialogue(dialogueData);
    }
}
