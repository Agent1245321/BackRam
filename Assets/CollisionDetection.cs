using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    public PlayerScript player;

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("Colliding");
        Debug.Log(player.s == PlayerScript.States.falling);
        if (player.s == PlayerScript.States.falling)
        {
            player.grounded = true;
            Debug.Log("Grounded");
        }
    }
}
