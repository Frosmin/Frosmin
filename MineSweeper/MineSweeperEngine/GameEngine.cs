using SerializerLib;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace MineSweeperEngine
{
    public class GameEngine
    {
        public const int NUM_COLUMNS = 10;
        public const int NUM_ROWS = 10;
        public const int NUM_MINES = 10;

        private readonly ISerializer _serializer;
        private readonly IFileSystem _fileSystem;

        public GameBoard GameBoard { get; private set; }

        public GameEngine(ISerializer serializer,
                          IFileSystem fileSystem)
        {
            _serializer = serializer;
            _fileSystem = fileSystem;
        }

        public void NewGame()
        {
            GameBoard = new GameBoard();
            AddCells();
            AddMines();
            AddAdjacents();
        }

        public void LoadGame(string fileGame)
        {
            try
            {
                GameBoard = _serializer.DeserializeFile<GameBoard>(fileGame);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading the game: {ex.Message}");
            }
        }

        public void SaveGame(string fileGame)
        {
            try
            {
                _fileSystem.File.WriteAllText(fileGame, _serializer.Serialize<GameBoard>(GameBoard));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving the game: {ex.Message}");
            }
        }

        public void FlagCell(int x, int y)
        {
            Cell cell = GetCell(x, y);
            if (!cell.IsFlagged && !cell.IsRevealed)
                cell.IsFlagged = true;
        }

        public void RevealCell(int x, int y)
        {
            Cell cell = GetCell(x, y);
            if (!cell.IsRevealed)
            {
                cell.IsRevealed = true;
                if (cell.IsFlagged)
                    cell.IsFlagged = false;
                if (cell.IsMine)
                {
                    GameBoard.Status = GameStatus.Failed;
                    RevealAllCells();
                }
                else if (cell.AdjacentMines == 0)
                {
                    RevealAdjacents(cell);
                }
                if (IsCompleted())
                {
                    GameBoard.Status = GameStatus.Completed;
                    RevealAllCells();
                }
            }
        }

        private void RevealAllCells()
        {
            GameBoard.Cells.ForEach(cell =>  
            {
                cell.IsRevealed = true;
            });
        }

        private bool IsCompleted()
        {
            return GameBoard.Cells.Count(c => c.IsRevealed) == ((NUM_COLUMNS * NUM_ROWS) - NUM_MINES);
        }

        private void RevealAdjacents(Cell cell)
        {
            List<Cell> adjacents = GetAdjacents(cell);
            foreach (Cell adjacent in adjacents)
            {
                if (!adjacent.IsMine && !adjacent.IsRevealed)
                {
                    adjacent.IsRevealed = true;
                    if (cell.IsFlagged)
                        cell.IsFlagged = false;
                    if (adjacent.AdjacentMines == 0)
                        RevealAdjacents(adjacent);
                }
            }
        }

        private void AddCells()
        {
            for (int y = 0; y < NUM_ROWS; y++)
            {
                for (int x = 0; x < NUM_COLUMNS; x++)
                {
                    GameBoard.Cells.Add(new Cell()
                    {
                        PosX = x,
                        PosY = y
                    });
                }
            }
        }

        private void AddMines()
        {
            Random random = new Random();
            for (int i = 0; i < NUM_MINES; i++)
            {
                bool mineCreated = false;
                while (!mineCreated)
                {
                    int x = random.Next(0, NUM_COLUMNS - 1);
                    int y = random.Next(0, NUM_ROWS - 1);
                    if (!MineExists(x, y))
                    {
                        GetCell(x, y).IsMine = true;
                        mineCreated = true;
                    }
                }
            }
        }

        private void AddAdjacents()
        {
            var mines = GameBoard.Cells.Where(c => c.IsMine);
            foreach (var mine in mines)
            {
                List<Cell> adjacents = GetAdjacents(mine);
                foreach (Cell adjacent in adjacents)
                {
                    if (!adjacent.IsMine)
                    {
                        adjacent.AdjacentMines++;
                    }
                }
            }
        }

        private bool MineExists(int x, int y)
        {
            if (!CellIsInGameBoard(x, y))
                throw new IndexOutOfRangeException($"The coordinate ({x}, {y}) is out of range");

            return GameBoard.Cells.Exists(c => c.PosX == x && c.PosY == y && c.IsMine);
        }

        public Cell GetCell(int x, int y)
        {
            if (!CellIsInGameBoard(x, y))
                throw new IndexOutOfRangeException($"The coordinate ({x}, {y}) is out of range");

            return GameBoard.Cells.First(c => c.PosX == x && c.PosY == y);
        }

        private List<Cell> GetAdjacents(Cell cell)
        {
            List<Cell> adjacents = new List<Cell>();

            if (CellIsInGameBoard(cell.PosX - 1, cell.PosY - 1))
                adjacents.Add(GetCell(cell.PosX - 1, cell.PosY - 1));
            if (CellIsInGameBoard(cell.PosX, cell.PosY - 1))
                adjacents.Add(GetCell(cell.PosX, cell.PosY - 1));
            if (CellIsInGameBoard(cell.PosX + 1, cell.PosY - 1))
                adjacents.Add(GetCell(cell.PosX + 1, cell.PosY - 1));
            if (CellIsInGameBoard(cell.PosX - 1, cell.PosY))
                adjacents.Add(GetCell(cell.PosX - 1, cell.PosY));
            if (CellIsInGameBoard(cell.PosX + 1, cell.PosY))
                adjacents.Add(GetCell(cell.PosX + 1, cell.PosY));
            if (CellIsInGameBoard(cell.PosX - 1, cell.PosY + 1))
                adjacents.Add(GetCell(cell.PosX - 1, cell.PosY + 1));
            if (CellIsInGameBoard(cell.PosX, cell.PosY + 1))
                adjacents.Add(GetCell(cell.PosX, cell.PosY + 1));
            if (CellIsInGameBoard(cell.PosX + 1, cell.PosY + 1))
                adjacents.Add(GetCell(cell.PosX + 1, cell.PosY + 1));

            return adjacents;
        }

        private static bool CellIsInGameBoard(int x, int y)
        {
            return x >= 0 && y >= 0 && x < NUM_COLUMNS && y < NUM_ROWS;
        }
    }
}
