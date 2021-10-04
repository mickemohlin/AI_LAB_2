using System;
using System.IO;
using BlazorConnect4.Model;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;

namespace BlazorConnect4.AIModels
{
    [Serializable]
    public abstract class AI
    {
        // Funktion för att bestämma vilken handling som ska genomföras.
        public abstract int SelectMove(GameBoard board); // change from SelectMove(Cell[,] grid) 

        // Funktion för att skriva till fil.
        public virtual void ToFile(string fileName)
        {
            Console.WriteLine("Saving data to file...");

            using (Stream stream = File.Open(fileName, FileMode.Create))
            {
                var bformatter = new BinaryFormatter();
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
                var bformatter = new BinaryFormatter();
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


    [Serializable]
    public class Action
    {
        public int action { get; set; }
        public double actionValue { get; set; }

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
        [NonSerialized] GameEngine game;
        Dictionary<int, List<Action>> QTable; // TODO: Make dictionary serializable.

        public string savedFileName;
        public int rewardAmount;
        public int gamesPlayed;

        // Constructor #1
        public QAgent(GameEngine gameEngine)
        {
            QTable = new Dictionary<int, List<Action>>();
            generator = new Random();
            rewardAmount = 0;
            game = gameEngine;
            gamesPlayed = 0;
        }

        // Constructor #2
        public QAgent(string fileName, GameEngine gameEngine)
        {
            QAgent tempAgent = (QAgent)(FromFile(fileName));

            // Copy values from saved file.
            generator = new Random();
            QTable = tempAgent.QTable;
            rewardAmount = tempAgent.rewardAmount;
            game = gameEngine;
            gamesPlayed = tempAgent.gamesPlayed;
            savedFileName = fileName;

            if (QTable == null)
            {
                Console.WriteLine("QTable was null!");
                QTable = new Dictionary<int, List<Action>>();
            }

            Console.WriteLine($"Games Played: {gamesPlayed}");
        }


        public override int SelectMove(GameBoard board)
        {
            int stateOfBoard = board.GetHashCode();
            int move = generator.Next(7); // Default value of move set to a random int between 1 and 7.

            Console.WriteLine($"Move: {move}");

            if (QTable.ContainsKey(stateOfBoard))
            {
                // State exists in QTable --> Choose action with best QValue.
                Console.WriteLine($"State: {stateOfBoard} exists in QTable!");
                Action bestPossibleAction = CheckBestPossibleAction(stateOfBoard);
                move = bestPossibleAction.action;
            }
            else
            {
                // State do not exist in QTable --> Add new state.
                Console.WriteLine($"Adding new state: {stateOfBoard} into QTable");
                AddNewState(stateOfBoard);
            }

            // 1. Choose action
            // 2. Perform action
            // 3. Measure Reward
            // 4. Evalute new QValue

            
            double reward = GetReward(move);
            Console.WriteLine($"Reward: {reward}");
            UpdateQValue(reward, stateOfBoard, move);

            return move;
        }

        public void SimulateOutcome()
        {
            // TODO
        }

        public double GetReward(int move)
        {
            if (!game.IsValid(move))
                return -0.1;
            else
            {
                int rowPlacement = game.GetCellPlacement(move);

                if (game.IsWin(move, rowPlacement))
                    return 1.0;
                else
                    return 0; // TODO: Check if loss?
            }

        }

        public void UpdateQValue(double reward, int state, int action)
        {
            double learningRate = 0.9;
            double discountFactor = 0.9;
            double maxFutureQValue = CheckBestPossibleAction(state).actionValue;

            double previousQValue = QTable[state][action].actionValue;

            QTable[state][action].actionValue = previousQValue + learningRate * (reward + (discountFactor * maxFutureQValue) - QTable[state][action].actionValue);
        }

        /*
         * Returns the best possible move for a given state.
         */
        public Action CheckBestPossibleAction(int state)
        {
            Action bestAction = new Action(generator.Next(7))
            {
                actionValue = int.MinValue
            };

            List<Action> currentStateActions = QTable[state];

            for (int i = 0; i < currentStateActions.Count; i++)
            {
                Action action = currentStateActions[i];

                if (bestAction.actionValue <= action.actionValue)
                {
                    bestAction.action = action.action;
                    bestAction.actionValue = action.actionValue;
                }
            }

            return bestAction;
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


        public static void TrainAgents(int iterations, string file)
        {
            Console.WriteLine("Training Agents...");

            //double epsilon = 0.9;

            GameEngine game;
            QAgent agent;

            for (int i = 1; i <= iterations; i++)
            {
                game = new GameEngine()
                {
                    ai = new RandomAI()
                };

                agent = new QAgent(file, game);

                while (game.active)
                {
                    int move = agent.SelectMove(game.Board); // Select best move.

                    while (!game.IsValid(move))
                    {
                        // TODO: Lower the QValue from the current action/move.
                        move = agent.generator.Next(7);
                    }

                    game.Play(move);
                }

                agent.gamesPlayed += 1;
                agent.ToFile("Data/Q1.bin");
            }

            Console.WriteLine("Training Finished!");
        }
    }

}
