using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartZone : MonoBehaviour
{
    public GameManager manger;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
            manger.StageStart();
    }
}