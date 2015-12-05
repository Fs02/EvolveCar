using UnityEngine;
using System.Collections.Generic;
using System;

public struct Neuron
{
    public int InputsCount;
    public List<double> Weights;

    public Neuron(int InputsCount)
    {
        this.InputsCount = InputsCount;

        Weights = new List<double>();
        // Additional one is for the bias
        for (int i = 0; i < InputsCount+1; ++i )
        {
            Weights.Add((double)UnityEngine.Random.Range(-1f, 1f));
        }
    }
}

public struct NeuronLayer
{
    public int NeuronsCount;
    public List<Neuron> Neurons;

    public NeuronLayer(int NeuronsCount, int InputsPerNeuron)
    {
        this.NeuronsCount = NeuronsCount;

        Neurons = new List<Neuron>();
        for (int i = 0; i < NeuronsCount; ++i)
        {
            Neurons.Add(new Neuron(InputsPerNeuron));
        }
    }
}

public class NeuralNet : MonoBehaviour {
    public int InputsCount = 4;
    public int OutputsCount = 4;
    public int HiddenLayersCount = 1;
    public int NeuronsPerHiddenLayer = 8;

    public double Biass = -1;
    public double ActivationResponse = 1;

    List<NeuronLayer> Layers = new List<NeuronLayer>();

    void Awake()
    {
        CreateNet();
    }

    public void CreateNet()
    {
        if (HiddenLayersCount > 0)
        {
            // first hidden layer
            Layers.Add(new NeuronLayer(NeuronsPerHiddenLayer, InputsCount));

            for (int i = 0; i < HiddenLayersCount-1; ++i)
            {
                Layers.Add(new NeuronLayer(NeuronsPerHiddenLayer, NeuronsPerHiddenLayer));
            }

            // output layer
            Layers.Add(new NeuronLayer(OutputsCount, NeuronsPerHiddenLayer));
        }
        else
        {
            Layers.Add(new NeuronLayer(OutputsCount, InputsCount));
        }
    }

    public List<double> GetWeights()
    {
        List<double> weights = new List<double>();

        foreach (NeuronLayer layer in this.Layers)
        {
            foreach (Neuron neuron in layer.Neurons)
            {
                for (int i = 0; i < neuron.InputsCount; ++i)
                {
                    weights.Add(neuron.Weights[i]);
                }
            }
        }
        return weights;
    }

    public int GetCountWeights()
    {
        int count = 0;
        foreach (NeuronLayer layer in this.Layers)
        {
            foreach (Neuron neuron in layer.Neurons)
            {
                count += neuron.InputsCount;
            }
        }
        return count;
    }

    public void PutWeights(List<double> weights)
    {
        int counter = 0;
        for (int i = 0; i < HiddenLayersCount + 1; ++i)
        {
            for (int j = 0; j < Layers[i].NeuronsCount; ++j)
            {
                for (int k = 0; k < Layers[i].Neurons[j].InputsCount; ++k)
                {
                    Layers[i].Neurons[j].Weights[k] = weights[counter++];
                }
            }
        }
    }

    public List<double> Process(List<double> inputs)
    {
        List<double> outputs = new List<double>();
        int cWeight = 0;

        // Make sure the size of inputs is correct
        if (inputs.Count != InputsCount)
            return outputs;

        for (int i = 0; i < HiddenLayersCount + 1; ++i)
        {
            if (i > 0)
                inputs = outputs;

            outputs.Clear();
            cWeight = 0;

            // sum the total of inputs*weights
            foreach (Neuron neuron in Layers[i].Neurons)
            {
                double netinput = 0;
                // TODO:
                // for (int j = 0; i < neuron.InputsCount - 1; ++i)
                for (int j = 0; i < neuron.InputsCount; ++i)
                {
                    netinput += neuron.Weights[j] * inputs[cWeight++];
                }
                // add bias
                netinput += neuron.Weights[neuron.InputsCount] * Biass;

                outputs.Add(Sigmoid(netinput, ActivationResponse));

                cWeight = 0;
            }
        }
        return outputs;
    }

    public double Sigmoid(double netinput, double response)
    {
	    return ( 2 / ( 1 + Math.Exp(2 * -netinput / response)) - 1);
    }
}