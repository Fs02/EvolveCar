using UnityEngine;
using System.Collections;

namespace EvolveCar.Experiment1
{
    public class Checkpoint : MonoBehaviour
    {
        public bool last = false;

        void OnTriggerEnter(Collider other)
        {
            gameObject.SetActive(false);
            LearnDirector.Instance.ReportCheckPoint(this, last);
        }
    }
}