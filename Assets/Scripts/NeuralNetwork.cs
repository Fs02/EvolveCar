using UnityEngine;
using Random = UnityEngine.Random;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Artificial
{
    public class Neuron
    {
        public int m_inputsCount;
        public List<float> m_weights;

        public Neuron(int count)
        {
            m_inputsCount = count + 1;
            m_weights = new List<float>();
            for (int i = 0; i < m_inputsCount; ++i)
            {
                m_weights.Add(Random.Range(-1f, 1f));
            }
        }
    }

    public class NeuronLayer
    {
        public int m_neuronsCount;
        public List<Neuron> m_neurons;

        public NeuronLayer(int neuronsCount, int inputPerNeuronsCount)
        {
            m_neuronsCount = neuronsCount;
            m_neurons = new List<Neuron>();
            for (int i = 0; i < m_neuronsCount; ++i )
            {
                m_neurons.Add(new Neuron(inputPerNeuronsCount));
            }
        }
    }

    public class NeuralNetwork : MonoBehaviour
    {
        public int m_inputsCount;
        public int m_outputsCount;
        public int m_hiddenLayersCount;
        public int m_neuronsPerHiddenLayer;
        public int m_biass = -1;
        public int m_activationResponse = 1;

        List<NeuronLayer> m_layers;
        
        void Awake()
        {
            CreateNet();
        }

        public void CreateNet()
        {
            m_layers = new List<NeuronLayer>();
            if (m_hiddenLayersCount > 0)
            {
                m_layers.Add(new NeuronLayer(m_neuronsPerHiddenLayer, m_inputsCount));

                for (int i = 0; i < m_hiddenLayersCount-1; ++i)
                {
                    m_layers.Add(new NeuronLayer(m_neuronsPerHiddenLayer, m_neuronsPerHiddenLayer));
                }

                m_layers.Add(new NeuronLayer(m_outputsCount, m_neuronsPerHiddenLayer));
            }
            else
            {
                m_layers.Add(new NeuronLayer(m_outputsCount, m_inputsCount));
            }
        }

        public List<float> GetWeights()
        {
            var weights = new List<float>();

            for (int i = 0; i < m_hiddenLayersCount + 1; ++i)
            {
                for (int j = 0; j < m_layers[i].m_neuronsCount; ++j)
                {
                    for (int k = 0; k < m_layers[i].m_neurons[j].m_inputsCount; ++k)
                    {
                        weights.Add(m_layers[i].m_neurons[j].m_weights[k]);
                    }
                }
            }
            return weights;
        }

        public void PutWeights(List<float> weights)
        {
            int counter = 0;

            for (int i = 0; i < m_hiddenLayersCount + 1; ++i)
            {
                for (int j = 0; j < m_layers[i].m_neuronsCount; ++j)
                {
                    for (int k = 0; k < m_layers[i].m_neurons[j].m_inputsCount; ++k)
                    {
                        m_layers[i].m_neurons[j].m_weights[k] = weights[counter];
                        counter++;
                    }
                }
            }
        }

        public int GetWeightsCount()
        {
            int counter = 0;

            for (int i = 0; i < m_hiddenLayersCount + 1; ++i)
            {
                for (int j = 0; j < m_layers[i].m_neuronsCount; ++j)
                {
                    for (int k = 0; k < m_layers[i].m_neurons[j].m_inputsCount; ++k)
                    {
                        counter++;
                    }
                }
            }
            return counter;
        }

        public List<float> Process(List<float> inputs)
        {
            List<float> outputs = new List<float>();

            if (inputs.Count != m_inputsCount)
                return outputs;

            for (int i = 0; i < m_hiddenLayersCount + 1; ++i)
            {
                if (i > 0)
                    inputs = new List<float>(outputs);
                outputs.Clear();

                for (int j = 0; j < m_layers[i].m_neuronsCount; ++j)
                {
                    int counter = 0;
                    float netinput = 0;
                    int inputsCount = m_layers[i].m_neurons[j].m_inputsCount;

                    for (int k = 0; k < inputsCount - 1; ++k)
                    {
                        netinput += m_layers[i].m_neurons[j].m_weights[k] * inputs[counter];
                        counter++;
                    }

                    // bias
                    netinput += m_layers[i].m_neurons[j].m_weights[inputsCount - 1] * m_biass;

                    outputs.Add((float)Math.Tanh(netinput));
                }
            }
            return outputs;
        }
    }
}
