using System;
using System.Text.RegularExpressions;

namespace Battleships
{
    internal class Program
    {
        // The player's grid of what they know of the opponent
        // 0 = unkown, 1 = missed, 2 = hit
        private static int[,] _playerGrid = new int[8, 8];

        // The fleet of both the player and the computer
        // 0 = empty, 1 = ship, 2 = destroyed ship, 3 = missed square for all grids
        private static int[,] _playerFleet = new int[8, 8];
        private static int[,] _computerFleet = new int[8, 8];

        // Indexes that are used by the player to select a particular square on the board
        private static readonly string[] ColIndexes = {"A", "B", "C", "D", "E", "F", "G", "H"};
        private static readonly string[] RowIndexes = {"1", "2", "3", "4", "5", "6", "7", "8"};

        // The number of ships left
        private static int _computerShips = 5;
        private static int _playerShips = 5;

        private static void Main(string[] args)
        {
            var option = Menu();

            switch (option)
            {
                case "1":
                    NewGame();
                    break;
                case "2":
                    Console.WriteLine("Resume a Game");
                    break;
                case "3":
                    Console.WriteLine("Read Instructions");
                    break;
                default:
                    Console.WriteLine("Not a valid option, idiot!");
                    break;
            }
        }

        // Handles the logic if the player selects 'Start a new game'
        private static void NewGame()
        {
            Console.Clear();

            ClearGrid(ref _playerGrid);
            ClearGrid(ref _playerFleet);
            ClearGrid(ref _computerFleet);

            RandomizeComputer();

            for (var i = 1; i <= 5; i++)
            {
                ShowGrid(_playerFleet);

                Console.Write("\nSquare number " + i);
                var choice = SelectSquare();
                var loc = GetLoc(choice);

                if (_playerFleet[loc[0], loc[1]] == 0)
                {
                    _playerFleet[loc[0], loc[1]] = 1;
                    Console.Clear();
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("\nSquare already taken!");
                    i--;
                }
            }

            while (true)
            {
                // Player turn
                Console.Clear();
                PlayerTurn();
                CheckWin();
                Console.WriteLine("Press enter to continue...");
                Console.ReadLine();
                Console.Clear();
                ComputerTurn();
                CheckWin();
                Console.WriteLine("Press enter to continue...");
                Console.ReadLine();
            }
        }

        private static void CheckWin()
        {
            if (_playerShips <= 0)
            {
                Console.WriteLine("Computer wins!");
                Environment.Exit(0);
            }
            else if (_computerShips <= 0)
            {
                Console.WriteLine("You win!");
                Environment.Exit(0);
            }
        }

        // The players turn logic
        private static void PlayerTurn()
        {
            Console.WriteLine("It's your turn!");
            Console.Write("\nYour fleet:");
            ShowGrid(_playerFleet);
            Console.Write("\nTarget tracker:");
            ShowTargetTracker();

            var selection = SelectSquare();
            var coords = GetLoc(selection);

            var row = coords[0];
            var col = coords[1];

            if (_playerGrid[row, col] != 0)
            {
                Console.WriteLine("Invalid selection!");
                PlayerTurn();
            }
            else
            {
                var hit = _computerFleet[row, col] == 1;

                if (hit)
                {
                    Console.WriteLine("\n--HIT!--\n");
                    _computerFleet[row, col] = 2;
                    _playerGrid[row, col] = 2;
                    _computerShips -= 1;
                }
                else
                {
                    Console.WriteLine("\n--MISS!--\n");
                    _playerGrid[row, col] = 1;
                    _computerFleet[row, col] = 3;
                }
            }
        }

        // The computers turn logic
        private static void ComputerTurn()
        {
            Console.WriteLine("It is the computers turn!");

            var selection = RandomSquare();

            var row = selection[0];
            var col = selection[1];

            // Check to make sure that we haven't attacked this square already. If it has, pick another square
            if (_playerFleet[row, col] >= 2)
            {
                ComputerTurn();
                return;
            }

            if (_playerFleet[row, col] == 1)
            {
                Console.WriteLine("\n--THEY HIT!--");
                _playerFleet[row, col] = 2;
                _playerShips -= 1;
            }
            else if (_playerFleet[row, col] == 0)
            {
                Console.WriteLine("\n--THEY MISSED!--");
                _playerFleet[row, col] = 3;
            }

            ShowGrid(_playerFleet);
        }

        // Randomly place 5 battleships onto a grid
        private static void RandomizeComputer()
        {
            for (var i = 0; i < 5; i++)
            {
                var square = RandomSquare();

                if (_computerFleet[square[0], square[1]] == 0)
                    _computerFleet[square[0], square[1]] = 1;
                else
                    i--;
            }
        }

        // Pick a random square on the grid as an int[] {row, col}
        private static int[] RandomSquare()
        {
            var rnd = new Random();

            var rowIndex = rnd.Next(0, 7);
            var colIndex = rnd.Next(0, 7);

            return new[] {rowIndex, colIndex};
        }

        // Clears a particular grid with 0s
        private static void ClearGrid(ref int[,] grid)
        {
            for (var row = 0; row < grid.GetLength(0); row++)
            for (var col = 0; col < grid.GetLength(1); col++)
                grid[row, col] = 0;
        }

        // Get the column and row of a particular square on a grid, given a particular coordinate in form int[] {row, col}
        private static int[] GetLoc(string coord)
        {
            var split = coord.ToCharArray();
            var col = Array.IndexOf(ColIndexes, split[0].ToString());
            var row = Array.IndexOf(RowIndexes, split[1].ToString());

            return new[] {row, col};
        }

        // Gets a valid square from the player between A1 to H8
        private static string SelectSquare()
        {
            Console.Write("\nSelect a square (format column then row, e.g G3)\n\n>>> ");

            var input = Console.ReadLine();

            // Regex pattern that matches any valid coordinate for a square
            var regex = new Regex(@"([A-H]|[a-h])([1-8])");

            if (input == null || (input.Length == 2 && regex.IsMatch(input))) return input;
            Console.WriteLine("\nNot a valid input, idiot!");
            // Uses recursion to repeat until a vald square has been selected
            input = SelectSquare();

            return input;
        }

        // Display the menu to the player and accept an option
        private static string Menu()
        {
            Console.Write("Select an option:\n\t1. New Game\n\t2. Resume Game\n\t3. Read Instructions\n\n>>> ");

            return Console.ReadLine();
        }

        // Show a given grid to the player in the console, using ' ' for empty/miss, 'D' for destroyed, 'S' for ship
        private static void ShowGrid(int[,] grid)
        {
            Console.Write("\n  ");
            // Write the letters at the top of the board
            foreach (var colIndex in ColIndexes) Console.Write(" " + colIndex);
            Console.WriteLine();
            for (var rowIndex = 0; rowIndex < grid.GetLength(0); rowIndex++)
            {
                // Start each row with it's corrosponding number
                Console.Write(RowIndexes[rowIndex] + " |");
                // For each column, add the value in the square that is also in the current row
                for (var colIndex = 0; colIndex < grid.GetLength(1); colIndex++)
                    switch (grid[rowIndex, colIndex])
                    {
                        case 0:
                            Console.Write(" |");
                            break;
                        case 1:
                            Console.Write("S|");
                            break;
                        case 2:
                            Console.Write("D|");
                            break;
                        case 3:
                            Console.Write("M|");
                            break;
                    }
                Console.WriteLine();
            }
        }

        // Shows a target tracker, that uses empty squares for unknown, 'H' for hit, 'M' for miss
        private static void ShowTargetTracker()
        {
            Console.Write("\n  ");
            // Write the letters at the top of the board
            foreach (var colIndex in ColIndexes) Console.Write(" " + colIndex);
            Console.WriteLine();
            for (var rowIndex = 0; rowIndex < _playerGrid.GetLength(0); rowIndex++)
            {
                // Start each row with it's corrosponding number
                Console.Write(RowIndexes[rowIndex] + " |");
                // For each column, add the value in the square that is also in the current row
                for (var colIndex = 0; colIndex < _playerGrid.GetLength(1); colIndex++)
                    switch (_playerGrid[rowIndex, colIndex])
                    {
                        case 0:
                            Console.Write(" |");
                            break;
                        case 1:
                            Console.Write("M|");
                            break;
                        case 2:
                            Console.Write("H|");
                            break;
                    }
                Console.WriteLine();
            }
        }
    }
}