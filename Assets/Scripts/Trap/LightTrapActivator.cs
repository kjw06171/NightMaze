using UnityEngine;

public class LightTrapActivator2D : MonoBehaviour
{
    // 유니티 인스펙터에서 연결할 RollingRock2D 스크립트
    // (돌 오브젝트에 붙은 스크립트와 연결해야 합니다.)
    public RollingRock2D rollingRock;

    // 💡 플레이어가 범위 안에 있는지 여부 (playerInRange)
    private bool playerInRange = false; 
    
    // 트랩이 한 번만 발동되도록 제어하는 변수
    private bool trapActivated = false; 

    void Start()
    {
        if (rollingRock != null)
        {
            // 돌을 초기 위치로 설정하고 숨김
            rollingRock.InitializePosition();
        }
    }

    void Update()
    {
        // 💡 요청하신 조건문: 범위 안에 있고, E 키를 눌렀으며, 아직 발동되지 않았다면
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !trapActivated)
        {
            ActivateTrap();
        }
    }

    // 💡 2D 충돌 감지 (플레이어가 범위 안에 들어왔을 때)
    private void OnTriggerEnter2D(Collider2D other)
    {
        // "Player" 태그를 가진 오브젝트인지 확인
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("E 키를 눌러 트랩을 발동할 수 있습니다.");
        }
    }

    // 💡 2D 충돌 해제 (플레이어가 범위 밖으로 나갔을 때)
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("플레이어가 범위를 벗어났습니다.");
        }
    }

    // 트랩 발동 로직 (PickUp()처럼 별도 함수로 분리)
    private void ActivateTrap()
    {
        if (rollingRock != null)
        {
            // 1. 돌 오브젝트에 이동 명령
            rollingRock.ActivateTrap();
            
            // 2. 트랩 발동 상태로 변경
            trapActivated = true; 
            Debug.Log("트랩 발동! 돌이 굴러갑니다.");
            
            // 3. [핵심 추가] 상호작용 후 해당 오브젝트(빛)를 파괴하여 사라지게 함
            Destroy(gameObject);
            Debug.Log("빛 오브젝트가 사라졌습니다.");
        }
    }
}