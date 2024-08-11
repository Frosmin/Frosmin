using System;
using System.Collections.Generic;
using System.Linq;

namespace MineSweeperEngine
{
    public class GameBoard : IEquatable<GameBoard>
    {
        public GameStatus Status { get; set; }

        public List<Cell> Cells { get; set; }

        public GameBoard()
        {
            Status = GameStatus.InProgress;
            Cells = new List<Cell>();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as GameBoard);
        }

        public bool Equals(GameBoard other)
        {
            return other != null &&
                   Status == other.Status &&
                   Cells.SequenceEqual(other.Cells);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Status, Cells);
        }
    }
}
