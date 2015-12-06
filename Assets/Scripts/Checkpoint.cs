using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour {
    public bool last = false;

    void OnTriggerEnter(Collider other)
    {
        LearnDirector.Instance.CheckPoint(this, last);
        gameObject.SetActive(false);
    }
}
