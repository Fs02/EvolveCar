using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LearnDirector : MonoBehaviour {
    public Transform start;
    public NeuralNet carBrain;

    public int populationSize = 6;

    private GenAlg genetic;
    private int totalWeightInNN;
    private List<Genome> population = new List<Genome>();
    private int generationCount = 0;
	
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
    }
}
