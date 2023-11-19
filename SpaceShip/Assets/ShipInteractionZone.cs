using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipInteractionZone : MonoBehaviour
{
    [SerializeField]
    private Spaceship spaceship;

    private ZeroGMovement player;

    private void OnTriggerEnter(Collider other){
        if(other.gameObject.CompareTag("Player")){
            player=other.gameObject.GetComponentInParent<ZeroGMovement>();
            if(player != null){
                player.AssignShip(spaceship);
            }
            print("player inraction zone");
        }
    }
    private void OnTriggerExit(Collider other){
        if(other.gameObject.CompareTag("Player")){
            if(player != null){
                player.RemoveShip();
            }
            print("player left zone");
        }
    }
}
