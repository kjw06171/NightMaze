using UnityEngine;

[System.Serializable]
public class DialogueSentence
{
    [Header("화자 이름 (비우면 내레이션)")]
    public string speakerName;

    [Header("초상화 (없으면 비활성화)")]
    public Sprite portrait;

    [Header("대사 내용")]
    [TextArea(3, 10)]
    public string sentence;
}

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Data", order = 1)]
public class DialogueSO : ScriptableObject
{
    // -----------------------------------------------------------
    // 🔥 새로운 확장형 대사 배열 (여러 캐릭터 지원)
    // -----------------------------------------------------------
    [Header("여러 캐릭터 대사 지원 (확장형)")]
    public DialogueSentence[] dialogueSentences;


    // -----------------------------------------------------------
    // 🔥 기존 구조 (하위 호환)
    // → 기존 대화 파일들이 깨지지 않도록 그대로 유지
    // -----------------------------------------------------------
    [Header("기존 단일 캐릭터 대사 (하위 호환용)")]
    public string characterName = "이름 없음";
    public Sprite portrait;

    [TextArea(3, 10)]
    public string[] sentences;


    // -----------------------------------------------------------
    // 🔥 대사 개수 자동 계산
    // → 새 구조(dialogueSentences)가 있으면 그걸 우선 사용
    // → 없으면 기존 sentences[] 사용
    // -----------------------------------------------------------
    public int SentenceCount
    {
        get
        {
            if (dialogueSentences != null && dialogueSentences.Length > 0)
                return dialogueSentences.Length;

            if (sentences != null)
                return sentences.Length;

            return 0;
        }
    }
}
