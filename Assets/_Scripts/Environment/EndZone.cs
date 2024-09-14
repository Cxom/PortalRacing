using Units;
using UnityEngine;

public class EndZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        PlayerBasicRigidbodyMotor player = other.GetComponent<PlayerBasicRigidbodyMotor>();
        if (player)
        {
            // player.transform.position = 
        } 
    }
}
