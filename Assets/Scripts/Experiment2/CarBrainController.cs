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
        public bool isReset = true;

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

            List<float> inputs = new List<float>();

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