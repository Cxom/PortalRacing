using Units;
using UnityEngine;

public class EndZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        PlayerMotor player = other.GetComponent<PlayerMotor>();
        if (player)
        {
            // player.transform.position = 
        } 
    }
}
