using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using UnityStandardAssets.Vehicles.Car;

namespace EvolveCar.Experiment2 {
    [RequireComponent(typeof(CarController))]
    [RequireComponent(typeof(Artificial.NeuralNetwork))]
    public class CarBrainController : MonoBehaviour
    {
        private CarController m_CarController;    // Reference to actual car controller we are controlling
        private Artificial.NeuralNetwork m_BlackBox;
        private Rigidbody m_Rigidbody;
        private bool isReset = true;

        public Utility.CatmullRomSpline track;
        public int nextPoint = 0;
        public float sight = 25f;
        public Transform sensor;
        public bool ignoreCollision = false;
        public Text indicator;

        //    [DBG_Track()]
        public float Steering = 0f;
        //   [DBG_Track()]
        public float Gass = 0f;
        //    [DBG_Track()]
        public float Speed = 0f;

        private void Awake()
        {
            m_CarController = GetComponent<CarController>();
            m_BlackBox = GetComponent<Artificial.NeuralNetwork>();
            m_Rigidbody = GetComponent<Rigidbody>();
        }


        private void FixedUpdate()
        {
            if (isReset)
            {
                // reset all forces
                m_Rigidbody = GetComponent<Rigidbody>();
                m_Rigidbody.isKinematic = true;

                var wheels = GetComponentsInChildren<WheelCollider>();
                foreach (WheelCollider wheel in wheels)
                {
                    wheel.brakeTorque = float.MaxValue;
                }
                isReset = false;
                return;
            }

            if (m_Rigidbody.isKinematic)
            {
                m_Rigidbody.isKinematic = false;
                var wheels = GetComponentsInChildren<WheelCollider>();
                foreach (WheelCollider wheel in wheels)
                {
                    wheel.brakeTorque = 0;
                }
            }

            var sqrDistanceToNextPoint = Vector3.SqrMagnitude(transform.position - track.points[nextPoint].position);
            if (sqrDistanceToNextPoint < 100f)
            {
                ++nextPoint;
                if (nextPoint == track.pointCount) nextPoint = 0;
            }

            List<float> inputs = new List<float>();

            var point = transform.InverseTransformPoint(track.points[nextPoint].position).normalized;
            inputs.Add(point.x);
            inputs.Add(point.y);
            inputs.Add(point.z);
            Debug.DrawLine(transform.position, track.points[nextPoint].position, Color.green);

            var afterNextPoint = nextPoint + 1 == track.pointCount ? 0 : nextPoint + 1;
            point = transform.InverseTransformPoint(track.points[afterNextPoint].position).normalized;
            inputs.Add(point.x);
            inputs.Add(point.y);
            inputs.Add(point.z);
            Debug.DrawLine(transform.position, track.points[afterNextPoint].position, Color.blue);

            // Forward Sensor
            RaycastHit hit = new RaycastHit();
            var direction = Quaternion.AngleAxis(-3f, sensor.up) * sensor.forward;
            hit.distance = sight;
            Physics.Raycast(sensor.position, direction, out hit, sight);
            inputs.Add(hit.distance / sight);
            if (hit.collider)
                Debug.DrawRay(sensor.position, direction * hit.distance, Color.red);


            direction = Quaternion.AngleAxis(3f, sensor.up) * sensor.forward;
            hit.distance = sight;
            Physics.Raycast(sensor.position, direction, out hit, sight);
            inputs.Add(hit.distance / sight);
            if (hit.collider)
                Debug.DrawRay(sensor.position, direction * hit.distance, Color.red);

            // Side forward Sensors
            var distance = sight / 3f;
            direction = Quaternion.AngleAxis(-10f, sensor.up) * sensor.forward;

            hit.distance = distance;
            Physics.Raycast(sensor.position, direction, out hit, distance);
            inputs.Add(hit.distance / distance);
            if (hit.collider)
                Debug.DrawRay(sensor.position, direction * hit.distance, Color.red);

            direction = Quaternion.AngleAxis(10f, sensor.up) * sensor.forward;

            hit.distance = distance;
            Physics.Raycast(sensor.position, direction, out hit, distance);
            inputs.Add(hit.distance / distance);
            if (hit.collider)
                Debug.DrawRay(sensor.position, direction * hit.distance, Color.red);

            distance = sight / 9f;
            direction = Quaternion.AngleAxis(-40f, sensor.up) * sensor.forward;

            hit.distance = distance;
            Physics.Raycast(sensor.position, direction, out hit, distance);
            inputs.Add(hit.distance / distance);
            if (hit.collider)
                Debug.DrawRay(sensor.position, direction * hit.distance, Color.red);

            direction = Quaternion.AngleAxis(40f, sensor.up) * sensor.forward;

            hit.distance = distance;
            Physics.Raycast(sensor.position, direction, out hit, distance);
            inputs.Add(hit.distance / distance);
            if (hit.collider)
                Debug.DrawRay(sensor.position, direction * hit.distance, Color.red);

            // Side sensors
            distance = sight / 15f;

            hit.distance = distance;
            Physics.Raycast(sensor.position, -sensor.right, out hit, distance);
            inputs.Add(hit.distance / distance);
            if (hit.collider)
                Debug.DrawRay(sensor.position, -sensor.right * hit.distance, Color.red);

            hit.distance = distance;
            Physics.Raycast(sensor.position, sensor.right, out hit, distance);
            inputs.Add(hit.distance / distance);
            if (hit.collider)
                Debug.DrawRay(sensor.position, sensor.right * hit.distance, Color.red);

            Speed = m_CarController.CurrentSpeed / m_CarController.MaxSpeed;
            inputs.Add(Speed);

            List<float> outputs = m_BlackBox.Process(inputs);

            Steering = outputs[0];
            Gass = outputs[1];
            m_CarController.Move((float)outputs[0], (float)outputs[1], (float)outputs[1], 0f);

            string text = "Input : ";
            foreach (float i in inputs)
                text += "\n" + i;
            text += "\nOutput : ";
            foreach (float o in outputs)
                text += "\n" + o;

            text += "\nFitness : " + LearnManager.Instance.CurrentFitness;
            indicator.text = text;
        }

        public void Reset()
        {
            isReset = true;
            nextPoint = 0;
            StartCoroutine(NeedReset());
        }

        IEnumerator NeedReset()
        {
            yield return new WaitForSeconds(0.5f);
            if (Speed < 0.001f && Gass > 0.5f)
            {
                gameObject.SetActive(false);
                gameObject.SetActive(true);
                LearnManager.Instance.Respawn();
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (!ignoreCollision)
                LearnManager.Instance.ReportCrash();
        }
    }
}