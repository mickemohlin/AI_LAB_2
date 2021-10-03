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
        string savedFileName;

        // Funktion för att bestämma vilken handling som ska genomföras.
        public abstract int SelectMove(GameBoard board); // change from SelectMove(Cell[,] grid) 

        // Funktion för att skriva till fil.
        public virtual void ToFile(string fileName)
        {
            Console.WriteLine("Saving data to file...");
            savedFileName = fileName;

            using (Stream stream = File.Open(fileName, FileMode.Create))
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                bformatter.Serialize(stream, this);
            }
        }

        // Funktion för att att läsa från fil.
        protected static AI FromFile(string fileName)
        {
            Console.WriteLine("Fetching agent from saved file...");

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

        public override int SelectMove(GameBoard board)
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


    public class Action
    {
        public int action;
        public double actionValue;

        public Action(int column)
        {
            action = column;
            actionValue = 0;
        }
    }
    

    [Serializable]
    public class QAgent : AI
    {
        [NonSerialized] Random generator;
        Dictionary<int, List<Action>> QTable;
        GameEngine game;
        int rewardAmount;
        int amountOfPlays;

        public QAgent(GameEngine gameEngine) 
        {
            QTable = new Dictionary<int, List<Action>>();
            game = gameEngine;
            generator = new Random();
            rewardAmount = 0;
            amountOfPlays = 0;
        }

        public QAgent(string fileName, GameEngine gameEngine)
        {
            QAgent tempAgent = (QAgent)(FromFile(fileName));

            // Copy values from saved file.
            game = gameEngine;
            generator = new Random();
            QTable = tempAgent.QTable;
            rewardAmount = tempAgent.rewardAmount;
            amountOfPlays = tempAgent.amountOfPlays;
        }

        public override int SelectMove(GameBoard board)
        {
            int stateOfBoard = board.GetHashCode();
            int move = generator.Next(7);

            amountOfPlays += 1;
            Console.WriteLine($"Amount of plays: {amountOfPlays}");
            
            
            if (QTable.ContainsKey(stateOfBoard))
            {
                // State exists in QTable.
                Console.WriteLine($"State: {stateOfBoard} exists in QTable!");
                move = CheckBestPossibleMove(stateOfBoard);
            } 
            else
            {
                // State do not exist in QTable --> Add new state.
                Console.WriteLine($"Adding new state: {stateOfBoard} into QTable");
                AddNewState(stateOfBoard);
            }

            double reward = GetReward(move);

            Console.WriteLine(reward);

            BellmanEquation(0.1, stateOfBoard, move);
            

            return move;
        }

        public double GetReward(int move)
        {

            if(!game.IsValid(move))
            {
                return -0.1;
            } 
           /* else if (game.IsWin(move, move)) // Wrong parameters
            {
                return 1;
            }
           */
            return -1;
        }

        public void BellmanEquation(double reward, int state, int action)
        {
            double discountFactor = 0.9;

            QTable[state][action].actionValue = reward + (discountFactor * QTable[state][action].actionValue);
        }

        /*
         * Returns the best possible move for a given state.
         */
        public int CheckBestPossibleMove(int state)
        {
            Action bestAction = new Action(int.MinValue);
            List<Action> currentStateActions = QTable[state];

            for (int i = 0; i <= currentStateActions.Count; i++)
            {
                Action action = currentStateActions[i];

                if (bestAction.actionValue <= action.actionValue)
                {
                    bestAction.action = action.action;
                    bestAction.actionValue = action.actionValue;
                }
            }

            return bestAction.action;
        }

        public void AddNewState(int state)
        {
            List<Action> listOfActions = new List<Action>();

            for (int i = 0; i <= 6; i++) // Initiate all valid actions within the game board.
            {
                listOfActions.Add(new Action(i));
            }

            QTable.Add(state, listOfActions);
        }

        public static void trainAgents()
        {
            double epsilon = 0.9;
            double discountFactor = 0.9;
            double learningRate = 0.9;

            Console.WriteLine("Training Agents...");
            Console.WriteLine($"{epsilon}, {discountFactor}, {learningRate}");
        }
    }
}
