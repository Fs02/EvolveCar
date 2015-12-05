using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LearnDirector : MonoBehaviour {
    static LearnDirector instance;
    static public LearnDirector Instance
    {
        get { return instance; }
    }

    public Transform start;
    public NeuralNet carBrain;

    public int populationSize = 6;
    public float maxTrialTime = 10f;

    private GenAlg genetic;
    private int totalWeightInNN;
    private List<Genome> population = new List<Genome>();

    private int currentIndividu = 0;
    private int currentGeneration = 0;
    private float timeleft = 10f;

    private List<Checkpoint> elapsedCheckpoint = new List<Checkpoint>();
	
    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        genetic = GetComponent<GenAlg>();
        totalWeightInNN = carBrain.GetComponent<NeuralNet>().GetCountWeights();
        
        genetic.ChromoLength = totalWeightInNN;
        genetic.PopulationSize = populationSize;
        genetic.Init();

        population = genetic.GetChromo();
        carBrain.transform.position = start.position;
        carBrain.transform.rotation = start.rotation;
        carBrain.PutWeights(population[0].weights);
        timeleft = maxTrialTime;
    }

    void Update()
    {
        timeleft -= Time.deltaTime;
    }

    public void CheckPoint(Checkpoint checkpoint)
    {
        population[currentIndividu].AddFitness((float)timeleft);
        elapsedCheckpoint.Add(checkpoint);
    }
}
