using MineSweeperEngine;
using SerializerLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;

namespace MineSweeper
{
    class Program
    {
        private const string NEW_GAME = "newgame";
        private const string FLAG_CELL = "flagcell";
        private const string REVEAL_CELL = "revealcell";
        public static readonly string CURRENT_GAME_FILE = "../../../../Game/game.xml";
        public static readonly string README_TEMPLATE = "../../../../../README.md.template";
        public static readonly string README = "../../../../../README.md";
        public static readonly string LAST_MOVES_FILE = "../../../../Game/last-moves.xml";
        public static readonly string TOP_MOVES_FILE = "../../../../Game/top-moves.xml";

        static void Main(string[] args)
        {
            if (args.Length == 2 && args[1] == NEW_GAME)
            {
                NewGame(args[0]);
            }
            else if (args.Length == 4 && args[1] == FLAG_CELL
                && int.TryParse(args[2], out int x)
                && int.TryParse(args[3], out int y))
            {
                FlagCell(args[0], x, y);
            }
            else if (args.Length == 4 && args[1] == REVEAL_CELL
                && int.TryParse(args[2], out x)
                && int.TryParse(args[3], out y))
            {
                RevealCell(args[0], x, y);
            }
        }

        private static List<LastMoves> LastMoves(string user, string command, int x, int y)
        {
            List<LastMoves> lastMoves = new List<LastMoves>(10);
            ISerializer serializer = new XMLSerializer();
            try
            {
                if (File.Exists(LAST_MOVES_FILE))
                    lastMoves = serializer.DeserializeFile<List<LastMoves>>(LAST_MOVES_FILE);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading Last Moves File: {ex.Message}");
            }
            if (command == NEW_GAME)
                lastMoves.Insert(0, new LastMoves($"{command}", user));
            else
                lastMoves.Insert(0, new LastMoves($"{command}({x}, {y})", user));
            lastMoves = lastMoves.OrderByDescending(m => m.DateTime).Take(10).ToList();
            try
            {
                File.WriteAllText(LAST_MOVES_FILE, serializer.Serialize<List<LastMoves>>(lastMoves));
                Console.WriteLine($"File {Path.GetFullPath(LAST_MOVES_FILE)} saved");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing Last Moves File: {ex.Message}");
            }
            return lastMoves;
        }

        private static List<TopMoves> TopMoves(string user)
        {
            List<TopMoves> topMoves = new List<TopMoves>();
            ISerializer serializer = new XMLSerializer();
            try
            {
                if (File.Exists(TOP_MOVES_FILE))
                    topMoves = serializer.DeserializeFile<List<TopMoves>>(TOP_MOVES_FILE);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading Top Moves File: {ex.Message}");
            }
            var movesUser = topMoves.FirstOrDefault(m => m.User == user);
            if (movesUser != null)
            {
                movesUser.AddMove();
            }
            else
            {
                topMoves.Add(new TopMoves(user));
            }
            topMoves = topMoves.OrderByDescending(m => m.TotalMoves).ThenByDescending(m => m.DateTime).ToList();
            try
            {
                File.WriteAllText(TOP_MOVES_FILE, serializer.Serialize<List<TopMoves>>(topMoves));
                Console.WriteLine($"File {Path.GetFullPath(TOP_MOVES_FILE)} saved");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing Last Moves File: {ex.Message}");
            }
            return topMoves;
        }

        private static void NewGame(string user)
        {
            try
            {
                GameEngine game = new GameEngine(new XMLSerializer(), new FileSystem());
                game.LoadGame(CURRENT_GAME_FILE);
                if (game.GameBoard.Status == GameStatus.Completed 
                    || game.GameBoard.Status == GameStatus.Failed)
                {
                    game.NewGame();
                    game.SaveGame(CURRENT_GAME_FILE);
                    List<LastMoves> lastMoves = LastMoves(user, NEW_GAME, 0, 0);
                    List<TopMoves> topMoves = TopMoves(user);
                    DrawBoardInTemplate(game.GameBoard, lastMoves, topMoves, README_TEMPLATE, README);
                }
                DrawBoardRevealed(game.GameBoard);
                Console.WriteLine("");
                DrawBoard(game.GameBoard);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

        private static void FlagCell(string user, int x, int y)
        {
            try
            {
                GameEngine game = new GameEngine(new XMLSerializer(), new FileSystem());
                game.LoadGame(CURRENT_GAME_FILE);
                if (game.GameBoard.Status == GameStatus.InProgress)
                {
                    game.FlagCell(x, y);
                    game.SaveGame(CURRENT_GAME_FILE);
                    DrawBoardRevealed(game.GameBoard);
                    Console.WriteLine("");
                    DrawBoard(game.GameBoard);
                    List<LastMoves> lastMoves = LastMoves(user, FLAG_CELL, x, y);
                    List<TopMoves> topMoves = TopMoves(user);
                    DrawBoardInTemplate(game.GameBoard, lastMoves, topMoves, README_TEMPLATE, README);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

        private static void RevealCell(string user, int x, int y)
        {
            try
            {
                GameEngine game = new GameEngine(new XMLSerializer(), new FileSystem());
                game.LoadGame(CURRENT_GAME_FILE);
                if (game.GameBoard.Status == GameStatus.InProgress)
                {
                    game.RevealCell(x, y);
                    game.SaveGame(CURRENT_GAME_FILE);
                    DrawBoardRevealed(game.GameBoard);
                    Console.WriteLine("");
                    DrawBoard(game.GameBoard);
                    List<LastMoves> lastMoves = LastMoves(user, REVEAL_CELL, x, y);
                    List<TopMoves> topMoves = TopMoves(user);
                    DrawBoardInTemplate(game.GameBoard, lastMoves, topMoves, README_TEMPLATE, README);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

        private static void DrawBoardRevealed(GameBoard gameBoard)
        {
            int i = 0;
            string row = "";
            foreach (var cell in gameBoard.Cells)
            {
                if (cell.IsMine)
                    row += "M ";
                else
                    row += cell.AdjacentMines + " ";
                i++;
                if (i == 10)
                {
                    i = 0;
                    Console.WriteLine(row);
                    row = "";
                }
            }
        }

        private static void DrawBoard(GameBoard gameBoard)
        {
            int i = 0;
            string row = "";
            foreach (var cell in gameBoard.Cells)
            {
                if (cell.IsRevealed && !cell.IsMine)
                    row += cell.AdjacentMines + " ";
                else if (cell.IsRevealed && cell.IsMine)
                    row += "M ";
                else if (cell.IsFlagged)
                    row += "F ";
                else
                    row += "X ";
                i++;
                if (i == 10)
                {
                    i = 0;
                    Console.WriteLine(row);
                    row = "";
                }
            }
        }

        private static void DrawBoardInTemplate(GameBoard gameBoard, 
            List<LastMoves> lastMoves, List<TopMoves> topMoves, 
            string readmeTemplateFile, string readmeFile)
        {
            try
            {
                string template = File.ReadAllText(readmeTemplateFile);
                StringBuilder board = new StringBuilder();
                if (gameBoard.Status == GameStatus.Completed)
                {
                    board.Append($"## YOU HAVE WON - [NEW GAME]({BuildIssueNewGameLink()})\n");
                }
                else if (gameBoard.Status == GameStatus.Failed)
                {
                    board.Append($"## YOU HAVE LOST - [NEW GAME]({BuildIssueNewGameLink()})\n");
                }
                board.Append("|   |   |   |   |   |   |   |   |   |   |\n");
                board.Append("| - | - | - | - | - | - | - | - | - | - |\n");
                int i = 0;
                StringBuilder row = new StringBuilder("|");
                foreach (var cell in gameBoard.Cells)
                {
                    if (cell.IsRevealed && !cell.IsMine)
                        row.Append($"![{cell.AdjacentMines}](MineSweeper/Resources/cell-{cell.AdjacentMines}.jpg \"{cell.AdjacentMines}\")|");
                    else if (cell.IsRevealed && cell.IsMine)
                        row.Append("![Mine](MineSweeper/Resources/cell-mine.jpg \"Mine\")|");
                    else if (cell.IsFlagged)
                        row.Append($"[![Flag](MineSweeper/Resources/cell-flag.jpg \"Flag\")]({BuildIssueLink(cell)})|");
                    else
                        row.Append($"[![Not revealed](MineSweeper/Resources/cell-not-revealed.jpg \"Not revealed\")]({BuildIssueLink(cell)})|");
                    i++;
                    if (i == 10)
                    {
                        i = 0;
                        board.Append(row);
                        row.Clear();
                        row.Append("\n|");
                    }
                }
                template = template.Replace("{GameBoard}", board.ToString());
                row.Clear();
                foreach (var lastMove in lastMoves)
                {
                    row.Append($"|[@{lastMove.User}](https://github.com/{lastMove.User})|{lastMove.Move}|\n");
                }
                template = template.Replace("{LastMoves}", row.ToString());
                row.Clear();
                topMoves = topMoves.OrderByDescending(m => m.TotalMoves).ThenByDescending(m => m.DateTime).Take(10).ToList();
                foreach (var topMove in topMoves)
                {
                    row.Append($"|[@{topMove.User}](https://github.com/{topMove.User})|{topMove.TotalMoves}|\n");
                }
                template = template.Replace("{TopMoves}", row.ToString());
                File.WriteAllText(readmeFile, template);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving readme.md: {ex.Message}");
            }
        }

        private static string BuildIssueLink(Cell cell)
        {
            return $"https://github.com/evaristocuesta/evaristocuesta/issues/new?title=revealcell%7C{cell.PosX}%7C{cell.PosY}" +
                $"&body=Just+push+%27Submit+new+issue%27+without+editing+the+title+if+you+want+to+reveal+the+cell.+If+you+want+to+flag+" +
                $"the+cell,+replace+%27revealcell%27+by+%27flagcell%27+in+the+title.+The+README.md+will+be+updated+after+some+minutes.";
        }

        private static string BuildIssueNewGameLink()
        {
            return $"https://github.com/evaristocuesta/evaristocuesta/issues/new?title=newgame" +
                $"&body=Just+push+%27Submit+new+issue%27+without+editing+the+title+if+you+want+to+play+a+new+game." +
                $"The+README.md+will+be+updated+after+some+minutes.";
        }
    }
}
