using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

[Serializable]
class net //neural network
{
    List<layer> layers;
    layer firstLayer;
    layer outputLayer;

    static double maxInitialWeight = 0.1;

    [Serializable]
    private class layer
    {
        private double alpha = 0.001; //learning rate
        private double lambda = 0; //weight decay rate, zero for no weight decay
        private Matrix<double> weights; //synapse weights for connections between input perceptrons to this layer
        private Matrix<double> weightUpdate; //unapplied change (for batch training)
        protected layer upstreamLayer; //where we pull input from, null for first layer
        protected layer downstreamLayer; //who pulls our values as input, null for output layer
        protected Vector<double> nodeVals; //values of this layer's perceptrons
        Vector<double> inputs; //inputs, pulled from upstreamLayer

        ///<summary>
        ///Neural network layer constructor - takes nodes in this layer and nodes in previous layer (not counting bias node)
        ///</summary>
        public layer(int inputSize, int size)
        {
            weights = Matrix<double>.Build.Random(size, inputSize + 1, new MathNet.Numerics.Distributions.ContinuousUniform(-maxInitialWeight, maxInitialWeight));
            weightUpdate = Matrix<double>.Build.Dense(size, inputSize + 1); //extra input for bias node
            nodeVals = Vector<double>.Build.Dense(size); 
        }

        public layer getUpstreamLayer()
        {
            return upstreamLayer;
        }

        public layer getDownstreamLayer()
        {
            return downstreamLayer;
        }

        public void setUpstreamLayer(layer newUpstreamLayer)
        {
            upstreamLayer = newUpstreamLayer;
        }

        public void setDownstreamLayer(layer newDownstreamLayer)
        {
            downstreamLayer = newDownstreamLayer;
        }

        ///<summary>
        ///Sigmoid activation function. Note, if f is the activation function, f' = f*(1-f)
        ///</summary>
        public double activationFunction(double input)
        {
            return 1.0 / (1 + Math.Exp(-input));
        }

        ///<summary>
        ///The sum over all layers of the weight's squared, for use in weight decay error function
        ///</summary>
        public double weightSqrd()
        {
            double weightSqrd = 0;
            for (int i = 0; i < weights.RowCount; i++)
            {
                for (int j = 0; j < weights.ColumnCount; j++)
                {
                    weightSqrd += weights[i, j] * weights[i, j];
                }
            }
            //if we're not the first layer
            if (upstreamLayer != null)
            {
                weightSqrd += upstreamLayer.weightSqrd();
            }
            return weightSqrd;
        }

        ///<summary>
        ///The error function, which we train to minimize
        ///</summary>
        public double error(Vector<double> expected)
        {
            //only bother calculating weight squared if we have weight decay enabled
            if ( lambda != 0 )
            {
                return 0.5 * (expected - nodeVals).DotProduct(expected - nodeVals) + 0.5 * lambda * weightSqrd();
            }
            else
            {
                return 0.5 * (expected - nodeVals).DotProduct(expected - nodeVals);
            }
        }

        ///<summary>
        ///Applies the activation function to all inputs
        ///</summary>
        public Vector<double> activation(Vector<double> input)
        {
            return input.Map<double>(activationFunction);
        }

        ///<summary>
        ///Takes a vector of doubles (the values of the input nodes), forward propagates them, and 
        ///recursively passes them to the downstream layer.
        ///Returns the output layer's node values after the propagation.
        ///</summary>
        public Vector<double> forward(Vector<double> input)
        {
            //The bias node is always one
            Vector<double> biasVector = Vector<double>.Build.Dense(1);
            biasVector[0] = 1;
            Vector<double> inputWithBias = Vector<double>.Build.Dense(weights.ColumnCount);
            //concatinate bias node with rest of inputs
            input.CopySubVectorTo(inputWithBias, 0, 0, input.Count);
            //Apply the weights to the input and apply the activation function
            Vector<double> prop = weights * inputWithBias;
            nodeVals = activation(prop);
            //remember the inputs for use in training
            inputs = inputWithBias;
            //if we're not the output layer, recurse
            if (downstreamLayer != null)
            {
                return downstreamLayer.forward(nodeVals);
            }
            //if we are the output layer, base case
            else
            {
                return nodeVals;
            }
        }

        ///<summary>
        ///(recursively) applies backpropagation algorithm, stores weight changes in weightUpdate
        ///Assumes we're the output layer
        ///</summary>
        public void train(Vector<double> trainingData)
        {
            //Find delta, the derivative of total error with respect to total input to each node (i.e. sum over j of w_ij input_j)
            Vector<double> delta = Vector<double>.Build.Dense(weights.RowCount);
            for (int j = 0; j < weights.RowCount; j++)
            {
                delta[j] = nodeVals[j] - trainingData[j];
                delta[j] = delta[j] * nodeVals[j] * (1 - nodeVals[j]);
            }
            //update w_ij by -alpha * dE/dw_ij = -alpha dE/d(input_i) d(input_i)/d_w_ij = -alpha delta_i input_j
            //with weight decay added if desired
            for (int i = 0; i < weights.RowCount; i++)
            {
                for (int j = 0; j < weights.ColumnCount; j++)
                {
                    if (lambda != 0)
                    {
                        weightUpdate[i, j] = -alpha * (inputs[j] * delta[i] + lambda * weights[i, j]);
                    }
                    else
                    {
                        weightUpdate[i, j] = -alpha * inputs[j] * delta[i];
                    }
                }
            }
            //if we're not the first layer, backpropagate to the previous layer
            if (upstreamLayer != null)
            {
                upstreamLayer.backprop(delta);
            }
        }

        ///<summary>
        ///(recursively) applies backpropagation algorithm, stores weight changes in weightUpdate
        ///Assumes we're NOT the output layer
        ///</summary>
        public void backprop(Vector<double> deltaDown)
        {
            //Find delta, the derivative of total error with respect to total input to each node (i.e. sum over j of w_ij input_j)
            Vector<double> delta = Vector<double>.Build.Dense(weights.RowCount);
            for (int j = 0; j < weights.RowCount; j++)
            {
                for (int l = 0; l < deltaDown.Count; l++)
                {
                    delta[j] += deltaDown[l] * downstreamLayer.weights[l, j];
                }
                delta[j] = delta[j] * nodeVals[j] * (1 - nodeVals[j]);
            }
            //update w_ij by -alpha * dE/dw_ij = -alpha dE/d(input_i) d(input_i)/d_w_ij = -alpha delta_i input_j
            //with weight decay added if desired
            for (int i = 0; i < weights.RowCount; i++)
            {
                for (int j = 0; j < weights.ColumnCount; j++)
                {
                    weightUpdate[i, j] = -alpha * (inputs[j] * delta[i] + lambda * weights[i, j]);
                }
            }
            //If we're not the first layer, continue backpropagating up
            if (upstreamLayer != null)
            {
                upstreamLayer.backprop(delta);
            }
        }


        ///<summary>
        ///Apply stored weight changes to this and all upstream layers
        ///</summary>
        public void updateWeights()
        {
            weights = weights + weightUpdate;
            if (upstreamLayer != null)
            {
                upstreamLayer.updateWeights();
            }
        }

        ///<summary>
        ///save this layer to a file
        ///</summary>
        public void saveToFile(string filename)
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream writer = new FileStream(filename, FileMode.Create);
            bf.Serialize(writer, this);
            writer.Close();
        }

        ///<summary>
        ///Returns the number of nodes in this layer
        ///</summary>
        public int size()
        {
            return weights.RowCount;
        }

        ///<summary>
        ///returns the number of inputs this layer accepts, not including bias node
        ///</summary>
        public int inputSize()
        {
            return weights.ColumnCount - 1; //don't include bias node
        }
    }

    ///<summary>
    ///Initialize a new neural network with random synapse weights. First layer has size inputSizexhiddenLayerSize
    ///Hidden layers have size hiddenLayerSizexhiddenLayerSize, and output layer has size hiddenLayerSizexoutputSize
    ///Generates the specified number of hidden layers (>=0)
    ///</summary>
    public net(int inputSize, int outputSize, int hiddenLayerNumbers, int hiddenLayerSize)
    {
        layers = new List<layer>();
        if ( hiddenLayerNumbers != 0 )
        {
            firstLayer = new layer(inputSize, hiddenLayerSize);
            layers.Add(firstLayer);
            for (int i = 0; i < hiddenLayerNumbers - 1; i++)
            {
                layers.Add(new layer(layers.Last().size(), hiddenLayerSize));
            }
            outputLayer = new layer(layers.Last().size(), outputSize);
            layers.Add(outputLayer);
            layers[0].setDownstreamLayer(layers[1]);
            layers[layers.Count - 1].setUpstreamLayer(layers[layers.Count - 2]);
            for (int i = 1; i < layers.Count - 1; i++)
            {
                layers[i].setUpstreamLayer(layers[i - 1]);
                layers[i].setDownstreamLayer(layers[i + 1]);
            }
        }
        //If there are no hidden layers, the first layer is also the output layer
        else
        {
            outputLayer = new layer(inputSize, outputSize);
            firstLayer = outputLayer;
            layers.Add(outputLayer);
        }
        
    }

    ///<summary>
    ///Initialize a new neural network from file
    ///from previously serialized neural net
    ///</summary>
    public net(string filename)
    {
        deserialize(filename);
    }

    ///<summary>
    ///Apply forward propagation, update all layers node values recursively
    ///</summary>
    public Vector<double> forward(Vector<double> input)
    {
        return firstLayer.forward(input);
    }

    ///<summary>
    ///Apply backpropagation training, takes a vector of inputs, what the outputs should be
    ///and how many times to train with this input
    ///</summary>
    public void train(Vector<double> inputs, Vector<double> desiredOutputs, int rounds)
    {
        for (int i = 0; i < rounds; i++)
        {
            firstLayer.forward(inputs);
            outputLayer.train(desiredOutputs);
            outputLayer.updateWeights();
        }
    }

    ///<summary>
    ///Apply backpropagation training to a batch of input, 
    ///takes a list of vectors of inputs, a list of what the outputs should be
    ///and how many times to train with this set of inputs
    ///</summary>
    public void train(List<Vector<double>> inputs, List<Vector<double>> desiredOutputs, int rounds)
    {
        for (int qq = 0; qq < inputs.Count; qq++)
        {
            for (int i = 0; i < rounds; i++)
            {
                firstLayer.forward(inputs[qq]);
                outputLayer.train(desiredOutputs[qq]);
            }
            outputLayer.updateWeights();
        }
    }


    ///<summary>
    ///Calculate the error function, useful for seeing how training is going
    ///</summary>
    public double error(Vector<double> inputs, Vector<double> desiredOutputs)
    {
        firstLayer.forward(inputs);
        return outputLayer.error(desiredOutputs);
    }

    ///<summary>
    ///Calculate the average of the error function for a batch of inputs,
    ///useful for seeing how training is going
    ///</summary>
    public double error(List<Vector<double>> inputs, List<Vector<double>> desiredOutputs)
    {
        double error = 0;
        for (int i = 0; i < inputs.Count; i++)
        {
            firstLayer.forward(inputs[i]);
            error += outputLayer.error(desiredOutputs[i]);
        }
        return error / inputs.Count;
    }

    ///<summary>
    ///Save net to file
    ///</summary>
    public void serialize(string filename)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream writer = new FileStream(filename, FileMode.Create);
        bf.Serialize(writer, layers);
        writer.Close();
    }

    ///<summary>
    ///Restore net state from file
    ///</summary>
    public void deserialize(string filename)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs = new FileStream(filename, FileMode.Open);

        //restore to array, build weights from array
        List<layer> loadedLayers;
        loadedLayers = (List<layer>)bf.Deserialize(fs);
        fs.Close();

        layers = loadedLayers;
        firstLayer = layers[0];
        outputLayer = layers.Last();
    }

    
}
