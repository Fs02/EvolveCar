using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour {
    void OnTriggerEnter(Collider other)
    {
        LearnDirector.Instance.CheckPoint(this);
        gameObject.SetActive(false);
    }
}
