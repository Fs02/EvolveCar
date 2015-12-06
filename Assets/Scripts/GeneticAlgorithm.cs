using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Artificial
{
    public class Genome
    {
        public List<double> m_weights;
        public double m_fitness;

        public Genome()
        {
            m_weights = new List<double>();
            m_fitness = 0;
        }

        public Genome(Genome copy)
        {
            m_weights = new List<double>(copy.m_weights);
            m_fitness = copy.m_fitness;
        }

        public Genome(List<double> weights, double fitness)
        {
            m_weights = weights;
            m_fitness = fitness;
        }
    }

    public class GeneticAlgorithm : MonoBehaviour
    {
        public List<Genome> m_population;
        public int m_populationSize;
        public int m_chromosomeLength;
        public double m_totalFitness;
        public double m_bestFitness;
        public double m_averageFitness;
        public double m_worstFitness;

        public int m_fittestGenomeId;

        public double m_mutationRate = 0.1;
        public double m_crossoverRate = 0.7;
        public double m_generationCount;
        public double m_maxPertubation = 0.3;
        public int m_elite = 2;
        public int m_eliteCopies = 1;

        public void Init()
        {
            m_totalFitness = 0;
            m_bestFitness = 0;
            m_averageFitness = 0;
            m_worstFitness = double.MaxValue;

            m_population = new List<Genome>();
            for (int i = 0; i < m_populationSize; ++i)
            {
                m_population.Add(new Genome());
                for (int j = 0; j < m_chromosomeLength; ++j)
                {
                    m_population[i].m_weights.Add(Random.Range(-1f, 1f));
                }
            }
        }

        void Mutate(ref Genome genome)
        {
            for (int i = 0; i < genome.m_weights.Count; ++i)
            {
                if (Random.Range(0f, 1f) < m_mutationRate)
                    genome.m_weights[i] += Random.Range(-1f, 1f) * m_maxPertubation;
            }
        }

        Genome GetChromoRoulette()
        {
            double slice = (double)(Random.Range(0f, 0.999f) * m_totalFitness);
            double fitnessSoFar = 0;

            for (int i = 0; i < m_populationSize; ++i)
            {
                fitnessSoFar += m_population[i].m_fitness;

                if (fitnessSoFar >= slice)
                {
                    return new Genome(m_population[i]);
                }
            }
            return new Genome(m_population[m_populationSize-1]);
        }

        void Crossover(Genome mum, Genome dad, ref Genome baby1, ref Genome baby2)
        {
            if (Random.Range(0f, 1f) > m_crossoverRate || mum == dad)
            {
                baby1.m_weights = new List<double>(mum.m_weights);
                baby2.m_weights = new List<double>(dad.m_weights);
                return;
            }

            int cp = Random.Range(0, m_chromosomeLength - 1);

            for (int i = 0; i < cp; ++i)
            {
                baby1.m_weights.Add(mum.m_weights[i]);
                baby2.m_weights.Add(dad.m_weights[i]);
            }

            for (int i = cp; i < mum.m_weights.Count; ++i)
            {
                baby1.m_weights.Add(dad.m_weights[i]);
                baby2.m_weights.Add(mum.m_weights[i]);
            }

            return;
        }

        public List<Genome> Epoch(ref List<Genome> oldPopulation)
        {
            m_population = oldPopulation;
            Reset();
            m_population.Sort((g1, g2) => g1.m_fitness.CompareTo(g2.m_fitness));
            CalculateBestWorstAvTot();

            List<Genome> newPopulation = new List<Genome>();

            if (m_elite * m_eliteCopies % 2 == 0)
            {
                GrabNBest(m_elite, m_eliteCopies, ref newPopulation);
                if (newPopulation[0].m_fitness != m_population[m_populationSize - 2].m_fitness)
                    Debug.LogError("Error 1");
                if (newPopulation[1].m_fitness != m_population[m_populationSize - 1].m_fitness)
                    Debug.LogError("Error 2");
            }

            while (newPopulation.Count < m_populationSize)
            {
                var mum = GetChromoRoulette();
                var dad = GetChromoRoulette();

                var baby1 = new Genome();
                var baby2 = new Genome();

                Crossover(mum, dad, ref baby1, ref baby2);

                Mutate(ref baby1);
                Mutate(ref baby2);

                newPopulation.Add(baby1);
                newPopulation.Add(baby2);
            }
            m_population = newPopulation;
            return m_population;
        }

        void GrabNBest(int bestCount, int bestCopies, ref List<Genome> population)
        {
            while (bestCount-- > 0)
            {
                for (int i = 0; i < bestCopies; ++i)
                {
                    Debug.Log("best id : " + (m_populationSize - 1 - bestCount) + " | "  + m_population[m_populationSize - 1 - bestCount].m_fitness);
                    population.Add(new Genome(m_population[m_populationSize - 1 - bestCount]));
                }
            }
        }

        public void CalculateBestWorstAvTot()
        {
            m_totalFitness = 0;
            double highestSoFar = 0;
            double lowestSoFar = double.MaxValue;

            for (int i = 0; i <m_populationSize; ++i)
            {
                if (m_population[i].m_fitness > highestSoFar)
                {
                    highestSoFar = m_population[i].m_fitness;
                    m_fittestGenomeId = i;
                    m_bestFitness = highestSoFar;
                }

                if (m_population[i].m_fitness < lowestSoFar)
                {
                    lowestSoFar = m_population[i].m_fitness;
                    m_worstFitness = lowestSoFar;
                }

                m_totalFitness += m_population[i].m_fitness;
            }

            m_averageFitness = m_totalFitness / m_populationSize;
        }

        void Reset()
        {
            m_totalFitness = 0;
            m_bestFitness = 0;
            m_worstFitness = double.MaxValue;
            m_averageFitness = 0;
        }

        public List<Genome> GetChromo()
        {
            return m_population;
        }

        public double GetAverageFitness()
        {
            return m_totalFitness / m_populationSize;
        }

        double GetBestFitness()
        {
            return m_bestFitness;
        }
    }
}