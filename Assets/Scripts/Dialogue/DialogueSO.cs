using UnityEngine;

// 💡 프로젝트에서 Assets -> Create -> Dialogue/Dialogue Data 로 새 대화 에셋을 만들 수 있습니다.
[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Data", order = 1)]
public class DialogueSO : ScriptableObject
{
    [Header("대화 정보")]
    // 💡 대화창에 캐릭터 이름을 표시할 경우를 대비하여 추가
    public string characterName = "이름 없음"; 
    
    [Header("캐릭터 초상화")]
    // 💡 캐릭터 초상화 이미지 (DialogueManager의 characterPortrait에 할당됨)
    public Sprite portrait; 
    
    [Header("대화 문장 목록")]
    [TextArea(3, 10)] // 인스펙터에서 여러 줄 입력을 쉽게 하도록 설정
    // 💡 큐에 들어갈 모든 대화 문장입니다.
    public string[] sentences; 

    // 💡 대화 문장의 개수 확인용
    public int SentenceCount => sentences != null ? sentences.Length : 0;
}