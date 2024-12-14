using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    public PlayerScript player;

    private void OnTriggerStay(Collider other)
    {
       
            player.grounded = true;
            
        
    }

    private void OnTriggerExit(Collider other)
    {
      
        player.grounded = false;
       
    }
}
