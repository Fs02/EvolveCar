using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityStandardAssets.Vehicles.Car;

[RequireComponent(typeof(CarController))]
[RequireComponent(typeof(NeuralNet))]
public class CarBlackBoxController : MonoBehaviour
{
    private CarController m_CarController;    // Reference to actual car controller we are controlling
    private NeuralNet m_BlackBox;
    private Rigidbody m_Rigidbody;

    public float forwardSight = 20f;
    public float sideForwardSight = 5f;
    public float sideSight = 2f;
    public Transform sensor;

    private void Awake()
    {
        m_CarController = GetComponent<CarController>();
        m_BlackBox = GetComponent<NeuralNet>();
        m_Rigidbody = GetComponent<Rigidbody>();
    }


    private void FixedUpdate()
    {
        List<double> inputs = new List<double>();

        // Forward Sensor
        RaycastHit hit;
        Physics.Raycast(sensor.position, sensor.forward, out hit, forwardSight);
        inputs.Add(Mathf.Max(hit.distance / forwardSight, 1f));
        if (hit.collider)
            Debug.DrawRay(sensor.position, sensor.forward * hit.distance, Color.red);

        // Side forward Sensors
        var sideForwardLeft = Quaternion.AngleAxis(-45f, sensor.up) * sensor.forward;
        Physics.Raycast(sensor.position, sideForwardLeft, out hit, sideForwardSight);
        inputs.Add(Mathf.Max(hit.distance / sideForwardSight, 1f));
        if (hit.collider)
            Debug.DrawRay(sensor.position, sideForwardLeft * hit.distance, Color.red);

        var sideForwardRight = Quaternion.AngleAxis(45f, sensor.up) * sensor.forward;
        Physics.Raycast(sensor.position, sideForwardRight, out hit, sideForwardSight);
        inputs.Add(Mathf.Max(hit.distance / sideForwardSight, 1f));
        if (hit.collider)
            Debug.DrawRay(sensor.position, sideForwardRight * hit.distance, Color.red);

        // Side sensors
        Physics.Raycast(sensor.position, -sensor.right, out hit, sideSight);
        inputs.Add(Mathf.Max(hit.distance / sideSight, 1f));
        if (hit.collider)
            Debug.DrawRay(sensor.position, -sensor.right * hit.distance, Color.red);

        Physics.Raycast(sensor.position, sensor.right, out hit, sideSight);
        inputs.Add(Mathf.Max(hit.distance / sideSight, 1f));
        if (hit.collider)
            Debug.DrawRay(sensor.position, sensor.right * hit.distance, Color.red);

        List<double> outputs = m_BlackBox.Process(inputs);

        m_CarController.Move((float)outputs[0], (float)outputs[1], (float)outputs[2], (float)outputs[3]);
    }
}