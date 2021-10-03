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
        public abstract int SelectMove(GameBoard board); // change from SelectMove(Cell[,] grid) 

        // Funktion för att skriva till fil.
        public virtual void ToFile(string fileName)
        {
            Console.WriteLine("Saving data to file...");

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
        public int actionValue;

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
        int rewardAmount;
        public int gamesPlayed;

        public QAgent() 
        {
            QTable = new Dictionary<int, List<Action>>();
            generator = new Random();
            rewardAmount = 0;
            gamesPlayed = 0;
        }

        public QAgent(string fileName)
        {
            QAgent tempAgent = (QAgent)(FromFile(fileName));

            // Copy values from saved file.
            generator = new Random();
            QTable = new Dictionary<int, List<Action>>();
            rewardAmount = tempAgent.rewardAmount;
            gamesPlayed = tempAgent.gamesPlayed;

            //Console.WriteLine($"Games Played: {gamesPlayed}");
        }


        public override int SelectMove(GameBoard board)
        {
            int stateOfBoard = board.GetHashCode();
            int move = generator.Next(7);
            
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

            return move;
        }

        /*
         * Returns the best possible move for a given state.
         */
        public int CheckBestPossibleMove(int state)
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

        
        public static void TrainAgents(int iterations, string file)
        {
            //double epsilon = 0.9;
            //double discountFactor = 0.9;
            //double learningRate = 0.9;

            QAgent agent = new QAgent(file);
            RandomAI opponent = new RandomAI();
            GameEngine game;

            for(int i=1; i<=iterations; i++)
            {
                Console.WriteLine($"Game: {i}");

                game = new GameEngine
                {
                    ai = opponent
                };

                while (game.active)
                {
                    //System.Threading.Thread.Sleep(100);

                    int move = agent.SelectMove(game.Board); // Select best move.

                    while (!game.IsValid(move))
                    {
                        // TODO: Lower the QValue from the current action/move.
                        move = agent.generator.Next(7);
                        System.Threading.Thread.Sleep(100);
                        Console.WriteLine($"Invalid Move: {move}");
                    }

                    game.Play(move);
                }

                agent.gamesPlayed += 1;
            }

            Console.WriteLine($"Agent Games Played: {agent.gamesPlayed}");
            agent.ToFile("Data/Q1.bin");
        }
    }
}
