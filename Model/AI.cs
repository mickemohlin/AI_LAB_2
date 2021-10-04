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
            
            //Console.WriteLine("Saving data to file...");

            using (Stream stream = File.Open(fileName, FileMode.Create))
            {
                var bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, this);
            }
        }

        // Funktion för att att läsa från fil.
        protected static AI FromFile(string fileName)
        {
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
        [NonSerialized] public Random generator;
        [NonSerialized] GameEngine game;
        Dictionary<int, List<Action>> QTable;
        public CellColor cellColor;

        public string savedFileName;
        public int rewardAmount;
        public int gamesPlayed;
        public int totalWins;
        public int lastState;
        public int lastAction;

        // Constructor #1
        public QAgent(GameEngine gameEngine, CellColor color)
        {
            QTable = new Dictionary<int, List<Action>>();
            generator = new Random();
            rewardAmount = 0;
            game = gameEngine;
            gamesPlayed = 0;
            cellColor = color;
            totalWins = 0;
        }

        // Constructor #2
        public QAgent(string fileName, GameEngine gameEngine, CellColor color)
        {
            QAgent tempAgent = (QAgent)(FromFile(fileName));

            // Copy values from saved file.
            generator = new Random();
            QTable = tempAgent.QTable;
            rewardAmount = tempAgent.rewardAmount;
            game = gameEngine;
            gamesPlayed = tempAgent.gamesPlayed;
            savedFileName = fileName;
            cellColor = color;
            totalWins = tempAgent.totalWins;

            if (QTable == null)
            {
                Console.WriteLine("QTable was null!");
                QTable = new Dictionary<int, List<Action>>();
            }

            //Console.WriteLine($"Games Played: {gamesPlayed}");
        }

        public override int SelectMove(GameBoard board)
        {
            int stateOfBoard = board.GetHashCode();
            int move = generator.Next(7);

            // Exploit
            if (QTable.ContainsKey(stateOfBoard))
            {
                // State exists in QTable --> Choose action with best QValue.
                Action bestPossibleAction = CheckBestPossibleAction(stateOfBoard);
                move = bestPossibleAction.action;
            }
            else
            {
                // State do not exist in QTable --> Add new state.
                AddNewState(stateOfBoard);
            }

            // Remember last state and action.
            lastState = stateOfBoard;
            lastAction = move;

            return move;
        }

        public void UpdateQValue(double reward, int state, int action)
        {
            double learningRate = 0.20;
            double discountFactor = 0.1;

            double maxFutureQValue = CheckBestPossibleAction(state).actionValue;

            double previousQValue = QTable[state][action].actionValue;

            QTable[state][action].actionValue = previousQValue + learningRate * (reward + (discountFactor * maxFutureQValue) - QTable[state][action].actionValue);
        }

     
        public Action CheckBestPossibleAction(int state)
        {
            Action bestAction = new Action(generator.Next(7))
            {
                actionValue = int.MinValue
            };

            if (QTable.ContainsKey(state))
            {
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
            else
            {
                return bestAction;
            }  
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


        public static void TrainAgents(int iterations, string opponentFile, string trainingAgentFile)
        {
            Console.WriteLine("Training Agents...");

            GameEngine game;
            AI opponent;

            for (int i = 1; i <= iterations; i++)
            {
                Console.WriteLine($"Round {i}");

                game = new GameEngine();

                if (opponentFile == "")
                    opponent = new RandomAI();
                else
                    opponent = new QAgent(opponentFile, game, CellColor.Red); 

                game.ai = new QAgent(trainingAgentFile, game, CellColor.Yellow);
                game.fileName = trainingAgentFile;

                while (game.active)
                {
                    int move = opponent.SelectMove(game.Board);

                    while (!game.IsValid(move))
                    {
                        if (opponent.GetType() == typeof(QAgent))
                        {
                            // If next move is invalid --> generate new random move.
                            QAgent agent = opponent as QAgent;
                            move = agent.generator.Next(7);
                        }
                        else
                        {
                            move = opponent.SelectMove(game.Board);
                        }

                    }

                    game.Play(move);
                }
            }

            Console.WriteLine("Training finished!");
        }
    }

}
