using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour {
    public bool last = false;

    void OnTriggerEnter(Collider other)
    {
        gameObject.SetActive(false);
        LearnDirector.Instance.ReportCheckPoint(this, last);
    }
}
