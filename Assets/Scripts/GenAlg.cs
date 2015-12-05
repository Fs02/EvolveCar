using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct Genome
{
    public List<double> weights;
    public double fitness;

    public Genome(List<double> Weights, double Fitness)
    {
        this.weights = Weights;
        this.fitness = Fitness;
    }

    public static int Compare(Genome left, Genome right)
    {
        if (left.fitness < right.fitness)
        {
            return -1;
        }
        if (left.fitness > right.fitness)
        {
            return 1;
        }
        return 0;
    }
}

public class GenAlg : MonoBehaviour 
{
    public double MutationRate = 0.1;
    public double CrossoverRate = 0.7;
    public double MaxPerturbation = 0.3;
    public int ChromoLength = 10;
    public int Elite = 2;
    public int EliteCopies = 1;

    List<Genome> Population = new List<Genome>();
    public int PopulationSize;
    public double TotalFitness = 0;
    public double BestFitness = 0;
    public double AverageFitness = 0;
    public double WorstFitness = float.MaxValue;
    int FittestGenome = 0;

    int cGeneration;

    public void Init()
    {
        Init(PopulationSize, ChromoLength);
    }

    public void Init(int PopulationCount, int WeightCount)
    {
        Debug.Log(PopulationSize);
        Debug.Log(WeightCount);
        Population.Clear();
        for (int i = 0; i <PopulationCount; ++i)
        {
            Population.Add(new Genome(new List<double>(), 0));

            for (int j = 0; j < ChromoLength; ++j)
            {
                Population[i].weights.Add(UnityEngine.Random.Range(-1f, 1f));
            }
        }
    }

    List<Genome> Epoch(List<Genome> old)
    {
        Population = old;
        Reset();

        Population.Sort();

        CalculateBestWorstToAvTot();

        List<Genome> newPopulation = new List<Genome>();

        GrabNBest(Elite, EliteCopies, ref newPopulation);

        while (newPopulation.Count < PopulationSize)
        {
            var mum = ChromoRoulette();
            var dad = ChromoRoulette();

            List<double> baby1, baby2;

            Crossover(mum.weights, dad.weights, out baby1, out baby2);

            Mutate(ref baby1);
            Mutate(ref baby2);

            newPopulation.Add(new Genome(baby1, 0));
            newPopulation.Add(new Genome(baby2, 0));
        }

        Population = newPopulation;
        return Population;
    }

    void Mutate(ref List<double> chromo)
    {
        for (int i = 0; i < chromo.Count; ++i)
        {
            if (UnityEngine.Random.Range(0f, 1f) < MutationRate)
            {
                chromo[i] += UnityEngine.Random.Range(-1f, 1f) * MaxPerturbation;
            }
        }
    }

    void Crossover(List<double> mum, List<double> dad, out List<double> baby1, out List<double> baby2)
    {
        //return parents as offspring dependent on the rate or if parents are the same
        if (UnityEngine.Random.Range(0f, 1f) > CrossoverRate || mum == dad)
        {
            baby1 = new List<double>(mum);
            baby2 = new List<double>(dad);
            return;
        }

        // determine the crossover point
        int cp = UnityEngine.Random.Range(0, ChromoLength - 1);

        // crossover
        baby1 = new List<double>();
        baby2 = new List<double>();
        for (int i = 0; i < cp; ++i)
        {
            baby1.Add(mum[i]);
            baby2.Add(dad[i]);
        }

        for (int i = cp; i < ChromoLength; ++i)
        {
            baby1.Add(dad[i]);
            baby2.Add(mum[i]);
        }
    }

    Genome ChromoRoulette()
    {
        double slice = UnityEngine.Random.Range(0f, 1f) * TotalFitness;

        double FitnessSoFar = 0;
        for (int i=0; i<PopulationSize; ++i)
        {
            FitnessSoFar += Population[i].fitness;

            if (FitnessSoFar >= slice)
            {
                return Population[i];
            }
        }
        return Population[PopulationSize - 1];
    }

    void GrabNBest(int NBest, int Copies, ref List<Genome> Pop)
    {
        while (NBest > 0)
        {
            for (int i = 0; i < Copies; ++i)
            {
                Pop.Add(Population[Population.Count - 1 - NBest]);
            }
            --NBest;
        }
    }

    void CalculateBestWorstToAvTot()
    {
        TotalFitness = 0;
        double HighestSoFar = 0f;
        double LowestSoFar = double.MaxValue;

        int i = 0;
        foreach (Genome individu in Population)
        {
            if (individu.fitness > HighestSoFar)
            {
                HighestSoFar = individu.fitness;
                FittestGenome = i;
                BestFitness = HighestSoFar;
            }

            if (individu.fitness < LowestSoFar)
            {
                LowestSoFar = individu.fitness;
                WorstFitness = LowestSoFar;
            }

            TotalFitness += individu.fitness;
        }

        AverageFitness = TotalFitness / Population.Count;
    }

    void Reset()
    {
        AverageFitness = BestFitness = WorstFitness = TotalFitness = 0;
    }

    public List<Genome> GetChromo()
    {
        return Population;
    }

    public double GetAverageFitness()
    {
        return TotalFitness / PopulationSize;
    }

    double GetBestFitness()
    {
        return BestFitness;
    }
}
