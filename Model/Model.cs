using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BlazorConnect4.AIModels;

namespace BlazorConnect4.Model
{
    public enum CellColor
    {
        Red,
        Yellow,
        Blank
    }


    public class Cell
    {
        public CellColor Color {get; set;}

        public Cell(CellColor color)
        {
            Color = color;
        }

    }

    public class GameBoard
    {
        public Cell[,] Grid { get; set; }

        public GameBoard()
        {
            Grid = new Cell[7, 6];

            //Populate the Board with blank pieces
            for (int i = 0; i <= 6; i++)
            {
                for (int j = 0; j <= 5; j++)
                {
                    Grid[i, j] = new Cell(CellColor.Blank);
                }
            }
        }

        public override int GetHashCode()
        {
            int hash = 0;

            for (int col=0; col<=6; col++)
            {
                for (int row=0; row<=5; row++)
                {
                    var cell = Grid[col, row];

                    if (cell.Color == CellColor.Blank)
                    {
                        hash += 1 * (row + (7 * col));
                    }
                    else if (cell.Color == CellColor.Red)
                    {
                        hash += 2 * (row + (7 * col));
                    }
                    else
                    {
                        hash += 3 * (row + (7 * col));
                    }
                }
            }

            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            GameBoard otherGameBoard = (GameBoard)obj;
            Cell[,] otherGrid = otherGameBoard.Grid;

            for (int col=0; col<=6; col++)
            {
                for (int row=0; row<=5; row++)
                {
                    if (Grid[col, row].Color != otherGrid[col, row].Color)
                    {
                        return false;
                    }
                }
            }
           
            return true;
        }
    }


    public class GameEngine
    {
        public GameBoard Board { get; set; }
        public CellColor Player { get; set;}
        public bool active;
        public String message;
        public AI ai;
        string fileName;


        public GameEngine()
        {
            Reset("Human");
        }

        public GameEngine ShallowCopy()
        {
            return (GameEngine)MemberwiseClone();

        }


        // Reset the game and creats the opponent.
        // TODO change the code so new RL agents are created.
        public void Reset(String playAgainst)
        {
            Board = new GameBoard();
            Player = CellColor.Red;
            active = true;
            message = "Starting new game";

            if (playAgainst == "Human")
            {
                ai = null;
            }
            else if (playAgainst == "Random")
            {
                if (File.Exists("Data/Random.bin"))
                {
                    ai = RandomAI.ConstructFromFile("Data/Random.bin");
                }
                else
                {
                    ai = new RandomAI();
                    ai.ToFile("Data/Random.bin");
                }
                
            }
            else if (playAgainst == "Q1") // Easy AI.
            {
                fileName = "Data/Q1.bin";

                if (File.Exists(fileName))
                {
                    
                    ai = new QAgent(fileName, this, CellColor.Yellow);
                }
                else
                {
                    ai = new QAgent(this, CellColor.Yellow);
                    ai.ToFile(fileName);
                }
                
            }
            else if (playAgainst == "Q2")
            {
                ai = new RandomAI(); //TODO: change to medium AI
            }
            else if (playAgainst == "Q3")
            {
                ai = new RandomAI(); //TODO: change to hard AI
            }
        }

        public void Train(string agent)
        {
            int trainingRounds = 100;

            if (agent == "Q1")
            {
                fileName = "Data/Q1.bin";

                if (File.Exists(fileName))
                {
                    QAgent.TrainAgents(trainingRounds, fileName, CellColor.Red);
                }
                else
                {
                    Console.WriteLine("File do not exist!");
                }
            }
            else if (agent == "Q2")
            {
                fileName = "Data/Q2.bin";

                if (File.Exists(fileName))
                {
                    QAgent.TrainAgents(trainingRounds, fileName, CellColor.Red);
                }
                else
                {
                    Console.WriteLine("File do not exist!");
                }
            }
            else if (agent == "Q3")
            {
                fileName = "Data/Q3.bin";

                if (File.Exists(fileName))
                {
                    QAgent.TrainAgents(trainingRounds, fileName, CellColor.Red);
                }
                else
                {
                    Console.WriteLine("File do not exist!");
                }
            }
        }


        public bool IsValid(int col)
        {
            return Board.Grid[col, 0].Color == CellColor.Blank;
        }


        public bool IsDraw()
        {
            for (int i = 0; i < 7; i++)
            {
                if (Board.Grid[i,0].Color == CellColor.Blank)
                {
                    return false;
                }
            }
            return true;
        }


        public bool IsWin(int col, int row)
        {
            bool win = false;
            int score = 0;
            
            // Check down
            if (row < 3)
            {
                for (int i = row; i <= row + 3; i++)
                {
                    if (Board.Grid[col,i].Color == Player)
                    {
                        score++;
                    }
                }
                win = score == 4;
                score = 0;
            }

            // Check horizontal

            int left = Math.Max(col - 3, 0);

            for (int i = left; i <= col; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (i+j <= 6 && Board.Grid[i+j,row].Color == Player)
                    {
                        score++;
                    }
                }
                win = win || score == 4;
                score = 0;
            }

            // Check left down diagonal

            int colpos;
            int rowpos;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    colpos = col - i + j;
                    rowpos = row - i + j;
                    if (0 <= colpos && colpos <= 6 &&
                        0 <= rowpos && rowpos < 6 &&
                        Board.Grid[colpos,rowpos].Color == Player)
                    {
                        score++;
                    }
                }

                win = win || score == 4;
                score = 0;
            }

            // Check left up diagonal

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    colpos = col + i - j;
                    rowpos = row - i + j;
                    if (0 <= colpos && colpos <= 6 &&
                        0 <= rowpos && rowpos < 6 &&
                        Board.Grid[colpos, rowpos].Color == Player)
                    {
                        score++;
                    }
                }
                
                win = win || score == 4;
                score = 0;
            }

            return win;
        }

        public int GetCellPlacement(int col)
        {
            for (int i=5; 0<=i; i--)
            {
                if(Board.Grid[col, i].Color == CellColor.Blank)
                {
                    return i;
                }
            }

            return 0;
        }

        
        public void RewardAI(QAgent agent, double reward)
        {
            agent.UpdateQValue(reward, agent.lastState, agent.lastAction);
        }


        public void SaveAiData(QAgent agent)
        {
            agent.gamesPlayed += 1;
            agent.ToFile(fileName);
        }


        public bool Play(int col)
        {
            if (IsValid(col) && active){

                for (int i = 5; i >= 0; i--)
                {
                    if (Board.Grid[col, i].Color == CellColor.Blank)
                    {
                        Board.Grid[col, i].Color = Player;

                        if (IsWin(col,i))
                        {
                            if (ai.GetType() == typeof(QAgent))
                            {
                                QAgent agent = ai as QAgent;

                                Console.WriteLine($"AI COLOR: {agent.cellColor}");

                                if (Player == agent.cellColor)
                                {
                                    RewardAI(agent, 1);
                                    SaveAiData(agent);
                                }
                                else
                                {      
                                    RewardAI(agent, -1);
                                    SaveAiData(agent);
                                }
                            }

                            message = Player.ToString() + " Wins";
                            active = false;
                            return true;
                        }

                        if (IsDraw())
                        {
                            if (ai.GetType() == typeof(QAgent))
                            {
                                QAgent agent = ai as QAgent;
                                RewardAI(agent, 0);
                                SaveAiData(agent);
                            }
                              
                            message = "Draw";
                            active = false;
                            return true;
                        }
                        break;
                    }
                }

                return PlayNext(); 
            }

            if (ai.GetType() == typeof(QAgent))
            {
                QAgent agent = ai as QAgent;
                RewardAI(agent, -0.1);
            }

            return false;
        }


        private bool PlayNext()
        {
            if (Player == CellColor.Red)
            {
                Player = CellColor.Yellow;
            }
            else
            {
                Player = CellColor.Red;
            }

            if (ai != null && Player == CellColor.Yellow)
            {
                int move = ai.SelectMove(Board);

                while (!IsValid(move))
                {
                        move = ai.SelectMove(Board);
                }

                return Play(move);
            }

            return false;
        }
    }


}
