public class Door : MonoBehaviour
{
    [SerializeField] int leadsToRoom = 2;
    [SerializeField] string sceneName = "Room2";

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FindObjectOfType<GameManager>().ProgressToRoom(leadsToRoom, sceneName);
        }
    }
}