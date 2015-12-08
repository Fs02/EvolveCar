﻿using UnityEngine;
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

    List<string[]> StatisticsSummary = new List<string[]>();
    int respwan = 0;

    void Awake()
    {
        instance = this;
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

        string[] label = new string[4];
        label[0] = "Best fitness";
        label[1] = "Average fitness";
        label[2] = "Worst fitness";
        label[3] = "Respawn";
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
        if (timeleft > 0)
            return;

        if (currentIndividu < populationSize)
        {
            NextIndividu();
        }
        else
        {
            NextGeneration();
        }
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
        SaveStatistics("Reports/" + currentGeneration.ToString() + ".csv");
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
        population[currentIndividu - 1].m_fitness += timeleft + maxTrialTime;
        timeleft = maxTrialTime;
    }

    public void ReportCrash()
    {
        if (currentIndividu < populationSize)
        {
            NextIndividu();
        }
        else
        {
            NextGeneration();
        }
    }

    void SaveStatistics(string path)
    {
        List<string[]> row = new List<string[]>();
        string[] data = new string[2];
        data[0] = "Population Size";
        data[1] = genetic.m_populationSize.ToString();
        row.Add(data);

        data = new string[2];
        data[0] = "Mutation Rate";
        data[1] = genetic.m_mutationRate.ToString();
        row.Add(data);

        data = new string[2];
        data[0] = "Crossover Rate";
        data[1] = genetic.m_crossoverRate.ToString();
        row.Add(data);

        data = new string[2];
        data[0] = "Pertubation";
        data[1] = genetic.m_maxPertubation.ToString();
        row.Add(data);

        data = new string[2];
        data[0] = "Chromosome Length";
        data[1] = genetic.m_chromosomeLength.ToString();
        row.Add(data);

        data = new string[2];
        data[0] = "Elite";
        data[1] = genetic.m_elite.ToString();
        row.Add(data);

        data = new string[2];
        data[0] = "Elite Copies";
        data[1] = genetic.m_eliteCopies.ToString();
        row.Add(data);
        row.Add(new string[0]);

        data = new string[2];
        data[0] = "Bias";
        data[1] = carBrain.m_biass.ToString();
        row.Add(data);

        data = new string[2];
        data[0] = "Inputs";
        data[1] = carBrain.m_inputsCount.ToString();
        row.Add(data);

        data = new string[2];
        data[0] = "Hidden Layer";
        data[1] = carBrain.m_hiddenLayersCount.ToString();
        row.Add(data);

        data = new string[2];
        data[0] = "Neuron Hidden Layer";
        data[1] = carBrain.m_neuronsPerHiddenLayer.ToString();
        row.Add(data);

        data = new string[2];
        data[0] = "Output";
        data[1] = carBrain.m_outputsCount.ToString();
        row.Add(data);

        data = new string[3];
        row.Add(new string[0]);
        data[0] = "No";
        data[1] = "Fitness";
        data[2] = "Chromosome";
        row.Add(data);

        int count = 0;
        foreach (Artificial.Genome i in genetic.m_population)
        {
            data = new string[3];
            data[0] = (++count).ToString();
            data[1] = i.m_fitness.ToString();
            data[2] = "";
            foreach (float c in genetic.m_population[count - 1].m_weights)
                data[2] += c + ", ";

            row.Add(data);
        }

        row.Add(new string[0]);
        data = new string[2];
        data[0] = "Total Fitness";
        data[1] = genetic.m_totalFitness.ToString();
        row.Add(data);

        data = new string[2];
        data[0] = "Best Fitness";
        data[1] = genetic.m_bestFitness.ToString();
        row.Add(data);

        data = new string[2];
        data[0] = "Average Fitness";
        data[1] = genetic.m_averageFitness.ToString();
        row.Add(data);

        data = new string[2];
        data[0] = "Worst Fitness";
        data[1] = genetic.m_worstFitness.ToString();
        row.Add(data);

        string[][] output = new string[row.Count][];

        for (int i = 0; i < output.Length; i++)
        {
            output[i] = row[i];
        }

        int length = output.GetLength(0);
        string delimiter = ",";

        StringBuilder sb = new StringBuilder();

        for (int index = 0; index < length; index++)
            sb.AppendLine(string.Join(delimiter, output[index]));

        StreamWriter outStream = System.IO.File.CreateText(path);
        outStream.WriteLine(sb);
        outStream.Close();

        // Write summary report
        var stats = new string[4];
        stats[0] = genetic.m_bestFitness.ToString();
        stats[1] = genetic.m_averageFitness.ToString();
        stats[2] = genetic.m_worstFitness.ToString();
        stats[3] = respwan.ToString();
        StatisticsSummary.Add(stats);

        output = new string[StatisticsSummary.Count][];

        for (int i = 0; i < output.Length; i++)
        {
            output[i] = StatisticsSummary[i];
        }

        length = output.GetLength(0);

        sb = new StringBuilder();

        for (int index = 0; index < length; index++)
            sb.AppendLine(string.Join(delimiter, output[index]));

        outStream = System.IO.File.CreateText("Reports/summary.csv");
        outStream.WriteLine(sb);
        outStream.Close();
    }
}
