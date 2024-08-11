using Xunit;
using Moq;
using SerializerLib;
using System.Linq;
using MineSweeperEngine;
using System.IO.Abstractions;
using System;

namespace MineSweeper.Tests
{
    public class GameEngineTests
    {
        private readonly Mock<ISerializer> _mockSerializer;
        private readonly Mock<IFileSystem> _mockFileSystem;

        public GameEngineTests()
        {
            _mockSerializer = new Mock<ISerializer>();
            _mockFileSystem = new Mock<IFileSystem>();
        }

        [Fact]
        public void NewGameShouldCreateCorrectGameBoard()
        {
            // Arrange

            // Act
            GameEngine game = new GameEngine(_mockSerializer.Object, 
                                             _mockFileSystem.Object);
            game.NewGame();

            // Assert
            Assert.Equal(GameStatus.InProgress, game.GameBoard.Status);
            Assert.Equal(GameEngine.NUM_COLUMNS * GameEngine.NUM_ROWS, game.GameBoard.Cells.Count);
            Assert.Equal(GameEngine.NUM_MINES, game.GameBoard.Cells.Count(c => c.IsMine));
            Assert.Equal(0, game.GameBoard.Cells.Count(c => c.IsRevealed));
            Assert.Equal(0, game.GameBoard.Cells.Count(c => c.IsFlagged));
            var mines = game.GameBoard.Cells.Where(m => m.IsMine);
            var cells = game.GameBoard.Cells.Where(c => !c.IsMine);
            foreach (Cell cell in cells)
            {
                if (!cell.IsMine)
                {
                    Assert.True(CheckAdjacentMines(game, cell));
                }
            }
        }

        [Theory]
        [InlineData(3, 5)]
        [InlineData(1, 8)]
        [InlineData(5, 2)]
        [InlineData(9, 4)]
        public void FlagCellShouldWorkCorrectly(int x, int y)
        {
            // Arrange

            // Act
            GameEngine game = new GameEngine(_mockSerializer.Object,
                                             _mockFileSystem.Object);
            game.NewGame();
            bool cellFlaggedBefore = game.GetCell(x, y).IsFlagged;
            game.FlagCell(x, y);
            bool cellFlagedAfter = game.GetCell(x, y).IsFlagged;

            // Assert
            Assert.False(cellFlaggedBefore);
            Assert.True(cellFlagedAfter);
        }

        [Theory]
        [InlineData(-3, 5)]
        [InlineData(11, 8)]
        [InlineData(5, -2)]
        [InlineData(9, 40)]
        public void FlagCellShouldThrowIndexOutOfRangeException(int x, int y)
        {
            // Arrange

            // Act
            GameEngine game = new GameEngine(_mockSerializer.Object,
                                             _mockFileSystem.Object);
            game.NewGame();

            // Assert
            Assert.Throws<IndexOutOfRangeException>(() => game.FlagCell(x, y));
        }

        [Theory]
        [InlineData("../../../Tests/game1.xml", 1, 3, "../../../Tests/game1-reveal-1-3-result.xml")]
        [InlineData("../../../Tests/game2.xml", 8, 8, "../../../Tests/game2-reveal-8-8-result.xml")]
        [InlineData("../../../Tests/game3.xml", 3, 0, "../../../Tests/game3-reveal-3-0-result.xml")]
        [InlineData("../../../Tests/game4.xml", 5, 2, "../../../Tests/game4-reveal-5-2-result.xml")]
        [InlineData("../../../Tests/game5.xml", 9, 3, "../../../Tests/game5-reveal-9-3-result.xml")]
        public void RevealCellShouldWorkCorrectly(string gameBoard, int x, int y, string gameBoardResult)
        {
            // Arrange

            // Act
            GameEngine game = new GameEngine(new XMLSerializer(),
                                             new FileSystem()); 
            game.LoadGame(gameBoard);
            game.RevealCell(x, y);
            GameBoard gameBoardAfter = game.GameBoard;
            GameEngine game2 = new GameEngine(new XMLSerializer(),
                                             new FileSystem());
            game2.LoadGame(gameBoardResult);

            // Assert
            Assert.Equal(game.GameBoard, game2.GameBoard);
        }

        [Theory]
        [InlineData(-3, 5)]
        [InlineData(11, 8)]
        [InlineData(5, -2)]
        [InlineData(9, 40)]
        public void RevealCellShouldThrowIndexOutOfRangeException(int x, int y)
        {
            // Arrange

            // Act
            GameEngine game = new GameEngine(_mockSerializer.Object,
                                             _mockFileSystem.Object);
            game.NewGame();

            // Assert
            Assert.Throws<IndexOutOfRangeException>(() => game.RevealCell(x, y));
        }

        private static bool CheckAdjacentMines(GameEngine game, Cell mine)
        {
            int count = 0;
            count += game.GameBoard.Cells.Count(c => c.PosX == mine.PosX - 1 
                                                     && c.PosY == mine.PosY - 1
                                                     && c.IsMine);
            count += game.GameBoard.Cells.Count(c => c.PosX == mine.PosX
                                                     && c.PosY == mine.PosY - 1
                                                     && c.IsMine);
            count += game.GameBoard.Cells.Count(c => c.PosX == mine.PosX + 1
                                                     && c.PosY == mine.PosY - 1
                                                     && c.IsMine);
            count += game.GameBoard.Cells.Count(c => c.PosX == mine.PosX - 1
                                                     && c.PosY == mine.PosY
                                                     && c.IsMine);
            count += game.GameBoard.Cells.Count(c => c.PosX == mine.PosX + 1
                                                     && c.PosY == mine.PosY
                                                     && c.IsMine);
            count += game.GameBoard.Cells.Count(c => c.PosX == mine.PosX - 1
                                                     && c.PosY == mine.PosY + 1
                                                     && c.IsMine);
            count += game.GameBoard.Cells.Count(c => c.PosX == mine.PosX
                                                     && c.PosY == mine.PosY + 1
                                                     && c.IsMine);
            count += game.GameBoard.Cells.Count(c => c.PosX == mine.PosX + 1
                                                     && c.PosY == mine.PosY + 1
                                                     && c.IsMine);

            return count == mine.AdjacentMines;
        }
    }
}
