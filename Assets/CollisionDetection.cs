using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    public PlayerScript player;

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("stay");
            player.grounded = true;
            
        
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Exit");
        player.grounded = false;
       
    }
}
