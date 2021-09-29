using System;
using System.IO;
using BlazorConnect4.Model;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BlazorConnect4.AIModels
{
    [Serializable]
    public abstract class AI
    {
        // Funktion för att bestämma vilken handling som ska genomföras.
        public abstract int SelectMove(Cell[,] grid);

        // Funktion för att skriva till fil.
        public virtual void ToFile(string fileName)
        {
            using (Stream stream = File.Open(fileName, FileMode.Create))
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                bformatter.Serialize(stream, this);
            }
        }

        // Funktion för att att läsa från fil.
        protected static AI FromFile(string fileName)
        {
            AI returnAI;
            using (Stream stream = File.Open(fileName, FileMode.Open))
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                returnAI = (AI)bformatter.Deserialize(stream);
            }
            return returnAI;

        }

    }



    [Serializable]
    public class RandomAI : AI
    {
        [NonSerialized] Random generator;

        public RandomAI()
        {
            generator = new Random();
        }

        public override int SelectMove(Cell[,] grid)
        {
            return generator.Next(7);
        }

        public static RandomAI ConstructFromFile(string fileName)
        {
            RandomAI temp = (RandomAI)(AI.FromFile(fileName));
            // Eftersom generatorn inte var serialiserad.
            temp.generator = new Random();
            return temp;
        }
    }



    [Serializable]
    public class QAgent : AI
    {
        double rewardAmount;
        public QAgent() { }

        public QAgent(string fileName)
        {
            // TODO: Create constructor that create agent that reads from file.
            Console.WriteLine(fileName);
        }

        //TODO: Initialize Q-Table.
       public static void defineEnvironmentStates()
        {
            var possibleStates = (1.6 * (10 ^ 13)); // Different states for the connect4 gameBoard.
        }
        
        //TODO: Define actions.

        //TODO: Define rewards.

        public override int SelectMove(Cell[,] grid)
        {
            return 5;
        }

        public static QAgent ConstructFromFile(string fileName)
        {
            QAgent tempAgent = (QAgent)(FromFile(fileName));
            return tempAgent;
        }

        public void trainAgents()
        {
            var epsilon = 0.9;
            var discountFactor = 0.9;
            var learningRate = 0.9;
        }
    }
}
