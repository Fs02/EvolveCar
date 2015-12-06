using UnityEngine;
using System.Collections.Generic;
using System;

public class Neuron
{
    public int InputsCount;
    public List<double> Weights;

    public Neuron(int inputsCount)
    {
        this.InputsCount = inputsCount+1;

        Weights = new List<double>();
        // Additional one is for the bias
        for (int i = 0; i < this.InputsCount; ++i)
        {
            Weights.Add((double)UnityEngine.Random.Range(-1f, 1f));
        }
    }
}

public class NeuronLayer
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

        for (int i = 0; i < HiddenLayersCount + 1; ++i )
        {
            for (int j = 0; j < Layers[i].NeuronsCount; ++j )
            {
                for (int k = 0; k < Layers[i].Neurons[j].InputsCount; ++k )
                {
                    weights.Add(Layers[i].Neurons[j].Weights[k]);
                }
            }
        }
        return weights;
    }

    public int GetCountWeights()
    {
        int count = 0;
        for (int i = 0; i < HiddenLayersCount + 1; ++i)
        {
            for (int j = 0; j < Layers[i].NeuronsCount; ++j)
            {
                for (int k = 0; k < Layers[i].Neurons[j].InputsCount; ++k)
                {
                    ++count;
                }
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
            for (int j = 0; j < Layers[i].NeuronsCount; ++j )
            {
                double netinput = 0;
                Debug.Log(Layers[i].Neurons[j].InputsCount + " : " + inputs.Count);

                // TODO:
                for (int k = 0; k < Layers[i].Neurons[j].InputsCount - 1; ++k)
                //for (int j = 0; j < neuron.InputsCount; ++j)
                {
                    netinput += Layers[i].Neurons[j].Weights[k] * inputs[cWeight];
                    cWeight++;
                }
                // add bias
                netinput += Layers[i].Neurons[j].Weights[Layers[i].Neurons[j].InputsCount - 1] * Biass;

                outputs.Add(Math.Tanh(netinput));

                cWeight = 0;
            }
        }
        return outputs;
    }

    public double Sigmoid(double netinput, double response)
    {
	    return 1 / ( 1 + Math.Exp(-netinput / response));
    }
}