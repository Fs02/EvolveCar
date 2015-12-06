using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using UnityStandardAssets.Vehicles.Car;

[RequireComponent(typeof(CarController))]
[RequireComponent(typeof(Artificial.NeuralNetwork))]
public class CarBlackBoxController : MonoBehaviour
{
    private CarController m_CarController;    // Reference to actual car controller we are controlling
    private Artificial.NeuralNetwork m_BlackBox;
    private Rigidbody m_Rigidbody;

    public float sight = 25f;
    public Transform sensor;
    public bool ignoreCollision = false;
    public Text indicator;

    private void Awake()
    {
        m_CarController = GetComponent<CarController>();
        m_BlackBox = GetComponent<Artificial.NeuralNetwork>();
        m_Rigidbody = GetComponent<Rigidbody>();
    }


    private void FixedUpdate()
    {
        List<double> inputs = new List<double>();

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

        inputs.Add(m_CarController.CurrentSpeed / m_CarController.MaxSpeed);

        List<double> outputs = m_BlackBox.Process(inputs);

        m_CarController.Move((float)outputs[0], (float)outputs[1], (float)outputs[1], 0f);

        string text = "Input : ";
        foreach (double i in inputs)
            text += "\n" + i;
        text += "\nOutput : ";
        foreach (double o in outputs)
            text += "\n" + o;

        text += "\nFitness : " + LearnDirector.Instance.CurrentFitness;
        indicator.text = text;
    }

    public void Reset()
    {
        var r = GetComponent<Rigidbody>();
        r.Sleep();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!ignoreCollision)
            LearnDirector.Instance.ReportCrash();
    }
}