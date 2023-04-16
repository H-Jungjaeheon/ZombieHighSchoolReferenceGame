using UnityEngine;

public class DetectedWall : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Player ������Ʈ")]
    private Player player;

    [Tooltip("�� �±�")]
    private const string WALL = "Wall";

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag(WALL))
        {
            player.WallDetectionResults(true);
        }
        else
        {
            player.WallDetectionResults(false);
        }
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
        
    //}

    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    if (collision.CompareTag(WALL))
    //    {
    //        player.WallDetectionResults(false);
    //    }
    //}
}
