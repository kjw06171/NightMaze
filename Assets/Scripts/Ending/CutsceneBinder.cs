using UnityEngine;
using Unity.Cinemachine;

public class CutsceneBinder : MonoBehaviour
{
    [Header("Real Characters")]
    public GameObject player;
    public GameObject dad;

    [Header("Dummy Characters")]
    public GameObject playerDummy;
    public GameObject dadDummy;

    [Header("Cinemachine Cameras")]
    public CinemachineCamera vcamPlayer;
    public CinemachineCamera vcamCutscene;

    void Awake()
    {
        // ⭐️ 게임 시작 시 기본 상태 고정
        player.SetActive(true);
        dad.SetActive(true);

        playerDummy.SetActive(false);
        dadDummy.SetActive(false);

        // 카메라도 플레이어 기준
        vcamPlayer.Priority = 10;
        vcamCutscene.Priority = 0;
    }

    public void StartCutscene()
    {
        playerDummy.transform.SetPositionAndRotation(
            player.transform.position,
            player.transform.rotation
        );

        dadDummy.transform.SetPositionAndRotation(
            dad.transform.position,
            dad.transform.rotation
        );

        player.SetActive(false);
        dad.SetActive(false);

        playerDummy.SetActive(true);
        dadDummy.SetActive(true);

        vcamPlayer.Priority = 0;
        vcamCutscene.Priority = 10;
    }

    public void EndCutscene()
    {
        player.transform.SetPositionAndRotation(
            playerDummy.transform.position,
            playerDummy.transform.rotation
        );

        dad.transform.SetPositionAndRotation(
            dadDummy.transform.position,
            dadDummy.transform.rotation
        );

        playerDummy.SetActive(false);
        dadDummy.SetActive(false);

        player.SetActive(true);
        dad.SetActive(true);

        vcamCutscene.Priority = 0;
        vcamPlayer.Priority = 10;
    }
}
