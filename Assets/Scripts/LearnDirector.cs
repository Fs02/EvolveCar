using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;

[RequireComponent(typeof(Artificial.GeneticAlgorithm))]
public class LearnDirector : MonoBehaviour {
    static LearnDirector instance;
    static public LearnDirector Instance
    {
        get { return instance; }
    }

    public Transform start;
    public Artificial.NeuralNetwork carBrain;

    public int populationSize = 6;
    public float maxTrialTime = 10f;

    private Artificial.GeneticAlgorithm genetic;
    private int totalWeightInNN;
    private List<Artificial.Genome> population = new List<Artificial.Genome>();
    float elapsedTime = 0f;

    public float CurrentFitness
    {
        get { return population[currentIndividu-1].m_fitness; }
    }

    [DBG_Track(0f, 0f, 1f)]
    public float BestFitness = 0;

    private int currentIndividu = 0;
    private int currentGeneration = 0;
    private float timeleft = 10f;

    private List<Checkpoint> elapsedCheckpoint = new List<Checkpoint>();

    public Text indicator;

    List<List<string>> StatisticsSummary = new List<List<string>>();
    int respwan = 0;

    void Awake()
    {
        instance = this;
        Load("Reports/0.csv");
    }

    void Start()
    {
        genetic = GetComponent<Artificial.GeneticAlgorithm>();
        totalWeightInNN = carBrain.GetComponent<Artificial.NeuralNetwork>().GetWeightsCount();
        
        genetic.m_chromosomeLength = totalWeightInNN;
        genetic.m_populationSize = populationSize;
        genetic.Init();

        population = genetic.GetChromo();
        NextIndividu();

        List<string> label = new List<string>();
        label.Add("Best fitness");
        label.Add("Average fitness");
        label.Add("Worst fitness");
        label.Add("Respawn");
        StatisticsSummary.Add(label);

        Application.runInBackground = true;
    }

    void Update()
    {
        genetic.CalculateBestWorstAvTot();
        indicator.text =
            "Generation : " + currentGeneration +
            "\nIndividu : " + currentIndividu +
            "\nTotal fit : " + genetic.m_totalFitness +
            "\nBest fit : " + genetic.m_bestFitness +
            "\nAverage Fit : " + genetic.m_averageFitness +
            "\nWorst Fit : " + genetic.m_worstFitness;

        timeleft -= Time.deltaTime;
        elapsedTime += Time.deltaTime;
        if (timeleft > 0)
            return;
        CalculateFitness();
        elapsedTime = 0f;

        if (currentIndividu < populationSize)
        {
            NextIndividu();
        }
        else
        {
            NextGeneration();
        }
    }

    void CalculateFitness()
    {
        var wPoint = 2f;
        var wTime = 2f;
        population[currentIndividu - 1].m_fitness = elapsedCheckpoint.Count * wPoint + wTime / elapsedTime;
        Debug.Log(population[currentIndividu - 1].m_fitness);
    }

    void NextIndividu()
    {
        carBrain.GetComponent<CarBlackBoxController>().Reset();
        carBrain.transform.position = start.position;
        carBrain.transform.rotation = start.rotation;
        carBrain.PutWeights(population[currentIndividu].m_weights);
        population[currentIndividu].m_fitness = 0;
        timeleft = maxTrialTime;
        ++currentIndividu;

        foreach (Checkpoint c in elapsedCheckpoint)
        {
            c.gameObject.SetActive(true);
        }
        elapsedCheckpoint.Clear();
    }

    void NextGeneration()
    {
        BestFitness = (float)genetic.m_bestFitness;
        Save("Reports/" + currentGeneration.ToString() + ".csv");
        currentIndividu = 0;
        respwan = 0;
        ++currentGeneration;
        population = genetic.Epoch(ref population);
        NextIndividu();
    }

    public void Respawn()
    {
        Debug.LogWarning("Respawn");
        respwan++;
        carBrain.GetComponent<CarBlackBoxController>().Reset();
        carBrain.transform.position = start.position;
        carBrain.transform.rotation = start.rotation;
        timeleft = maxTrialTime;
    }

    public void ReportCheckPoint(Checkpoint checkpoint, bool last=false)
    {
        elapsedCheckpoint.Add(checkpoint);
        if (last && elapsedCheckpoint.Count < 30)
        {
            if (currentIndividu < populationSize)
            {
                NextIndividu();
            }
            else
            {
                NextGeneration();
            }
            return;
        }
//        population[currentIndividu - 1].m_fitness += timeleft + maxTrialTime;
        timeleft = maxTrialTime;
    }

    public void ReportCrash()
    {
        CalculateFitness();
        elapsedTime = 0f;
        if (currentIndividu < populationSize)
        {
            NextIndividu();
        }
        else
        {
            NextGeneration();
        }
    }

    void Load(string path)
    {
        List<List<string>> dataGrid = Mono.Csv.CsvFileReader.ReadAll(path, Encoding.GetEncoding("gbk"));

        // TODO: deal with data grid
        foreach (var row in dataGrid)
        {
            var v = "";
            foreach (var cell in row)
            {
                v += "| " + cell;
            }
            Debug.Log(v);
        }
    }

    void Save(string path)
    {
        List<List<string>> row = new List<List<string>>();
        List<string> data = new List<string>();
        data.Add("Population Size");
        data.Add(genetic.m_populationSize.ToString());
        row.Add(data);

        data = new List<string>();
        data.Add("Mutation Rate");
        data.Add(genetic.m_mutationRate.ToString());
        row.Add(data);

        data = new List<string>();
        data.Add("Crossover Rate");
        data.Add(genetic.m_crossoverRate.ToString());
        row.Add(data);

        data = new List<string>();
        data.Add("Pertubation");
        data.Add(genetic.m_maxPertubation.ToString());
        row.Add(data);

        data = new List<string>();
        data.Add("Chromosome Length");
        data.Add(genetic.m_chromosomeLength.ToString());
        row.Add(data);

        data = new List<string>();
        data.Add("Elite");
        data.Add(genetic.m_elite.ToString());
        row.Add(data);

        data = new List<string>();
        data.Add("Elite Copies");
        data.Add(genetic.m_eliteCopies.ToString());
        row.Add(data);
        row.Add(new List<string>());

        data = new List<string>();
        data.Add("Bias");
        data.Add(carBrain.m_biass.ToString());
        row.Add(data);

        data = new List<string>();
        data.Add("Inputs");
        data.Add(carBrain.m_inputsCount.ToString());
        row.Add(data);

        data = new List<string>();
        data.Add("Hidden Layer");
        data.Add(carBrain.m_hiddenLayersCount.ToString());
        row.Add(data);

        data = new List<string>();
        data.Add("Neuron Hidden Layer");
        data.Add(carBrain.m_neuronsPerHiddenLayer.ToString());
        row.Add(data);

        data = new List<string>();
        data.Add("Output");
        data.Add(carBrain.m_outputsCount.ToString());
        row.Add(data);

        row.Add(new List<string>());
        
        data = new List<string>();
        data.Add("No");
        data.Add("Fitness");
        data.Add("Chromosome");
        row.Add(data);

        int count = 0;
        foreach (Artificial.Genome i in genetic.m_population)
        {
            data = new List<string>();
            data.Add((++count).ToString());
            data.Add(i.m_fitness.ToString());
            foreach (float c in genetic.m_population[count - 1].m_weights)
                data.Add(c.ToString());

            row.Add(data);
        }

        row.Add(new List<string>());


        data = new List<string>();
        data.Add("Total Fitness");
        data.Add(genetic.m_totalFitness.ToString());
        row.Add(data);
        
        data = new List<string>();
        data.Add("Best Fitness");
        data.Add(genetic.m_bestFitness.ToString());
        row.Add(data);

        data = new List<string>();
        data.Add("Average Fitness");
        data.Add(genetic.m_averageFitness.ToString());
        row.Add(data);

        data = new List<string>();
        data.Add("Worst Fitness");
        data.Add(genetic.m_worstFitness.ToString());
        row.Add(data);

        Mono.Csv.CsvFileWriter.WriteAll(row, path, Encoding.GetEncoding("gbk"));

        // Write summary report
        var stats = new  List<string>();
        stats.Add(genetic.m_bestFitness.ToString());
        stats.Add(genetic.m_averageFitness.ToString());
        stats.Add(genetic.m_worstFitness.ToString());
        stats.Add(respwan.ToString());
        StatisticsSummary.Add(stats);

        Mono.Csv.CsvFileWriter.WriteAll(StatisticsSummary, "Reports/Summary.csv", Encoding.GetEncoding("gbk"));
    }
}
