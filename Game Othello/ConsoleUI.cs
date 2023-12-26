using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Spectre.Console;

namespace Othello
{
    public class ConsoleUI
    {
        private Disc[,] _board = new Disc[8, 8];
        private Grid grid = new Grid();
        private Panel[,] panelBoard = new Panel[9, 9];

        public ConsoleUI(Disc[,] board)
        {
            for (int i = 0; i <= 8; i++)
            {
                string symbol = " ";

                for (int j = 0; j <= 8; j++)
                {
                    if (i > 0 && j > 0)
                    {
                        _board[i - 1, j - 1] = board[i - 1, j - 1];
                    }

                    if (i == 0 && j == 0)
                    {
                        grid.AddColumn();
                        panelBoard[i, j] = new Panel(" ").Border<Panel>(BoxBorder.None);
                    }
                    else if (i == 0)
                    {
                        grid.AddColumn();
                        panelBoard[i, j] = new Panel(new Text((j - 1).ToString())).PadLeft<Panel>(2).Border<Panel>(BoxBorder.None);
                    }
                    else if (j == 0)
                    {
                        panelBoard[i, j] = new Panel(new Text((i - 1).ToString())).PadTop<Panel>(1).Border<Panel>(BoxBorder.None);
                    }
                    else
                    {
                        if (_board[i - 1, j - 1] != Disc.None)
                        {
                            symbol = _board[i - 1, j - 1] == Disc.White ? "O" : "X";
                        }
                        else
                        {
                            symbol = " ";
                        }
                        panelBoard[i, j] = new Panel(new Text(symbol)).BorderStyle<Panel>(new Style(Color.SpringGreen2_1));
                    }
                }
            }

            Panel[] panelRow = new Panel[9];

            for (int i = 0; i <= 8; i++)
            {
                for (int j = 0; j <= 8; j++)
                {
                    panelRow[j] = panelBoard[i, j];
                }
                grid.AddRow(panelRow);
            }
        }

        public void UpdatePossibelMove(List<Position> legalMove)
        {
            grid = new Grid();

            foreach (var item in legalMove)
            {
                panelBoard[item.Row + 1, item.Col + 1] = new Panel(new Text(" ")).BorderStyle<Panel>(new Style(Color.SteelBlue1_1));
            }

            Panel[] panelRow = new Panel[9];

            for (int i = 0; i <= 8; i++)
            {
                for (int j = 0; j <= 8; j++)
                {
                    if (i == 0)
                    {
                        grid.AddColumn();
                    }
                    panelRow[j] = panelBoard[i, j];
                }
                grid.AddRow(panelRow);
            }
        }

        public void UpdateBoard(Disc disc, Position position, List<Position> legalMove, List<Position> outflanked)
        {
            grid = new Grid();

            foreach (var item in legalMove)
            {
                panelBoard[item.Row + 1, item.Col + 1] = new Panel(new Text(" ")).BorderStyle<Panel>(new Style(Color.SpringGreen2_1));
            }

            string symbol = disc == Disc.White ? "O" : "X";
            panelBoard[position.Row + 1, position.Col + 1] = new Panel(new Text(symbol)).BorderStyle<Panel>(new Style(Color.SpringGreen2_1));

            foreach (var item in outflanked)
            {
                panelBoard[item.Row + 1, item.Col + 1] = new Panel(new Text(symbol)).BorderStyle<Panel>(new Style(Color.SpringGreen2_1));
            }

            Panel[] panelRow = new Panel[9];

            for (int i = 0; i <= 8; i++)
            {
                for (int j = 0; j <= 8; j++)
                {
                    if (i == 0)
                    {
                        grid.AddColumn();
                    }
                    panelRow[j] = panelBoard[i, j];
                }
                grid.AddRow(panelRow);
            }
        }



        public void ShowGame()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(grid);
            //grid = null;
        }

    }
}
