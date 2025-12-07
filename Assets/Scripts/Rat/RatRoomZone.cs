using UnityEngine;

public class RatRoomZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 방 안에 들어온 모든 쥐에게 전달
            RatAI[] rats = FindObjectsOfType<RatAI>();
            foreach (RatAI r in rats)
                r.playerInsideRoom = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            RatAI[] rats = FindObjectsOfType<RatAI>();
            foreach (RatAI r in rats)
                r.playerInsideRoom = false;
        }
    }
}
