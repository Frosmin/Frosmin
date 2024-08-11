using System;

namespace MineSweeperEngine
{
    public class Cell : IEquatable<Cell>
    {
        public int PosX { get; set; }
        public int PosY { get; set; }
        public int AdjacentMines { get; set; }
        public bool IsMine { get; set; }
        public bool IsRevealed { get; set; }
        public bool IsFlagged { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Cell);
        }

        public bool Equals(Cell other)
        {
            return other != null &&
                   PosX == other.PosX &&
                   PosY == other.PosY &&
                   AdjacentMines == other.AdjacentMines &&
                   IsMine == other.IsMine &&
                   IsRevealed == other.IsRevealed &&
                   IsFlagged == other.IsFlagged;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PosX, PosY, AdjacentMines, IsMine, IsRevealed, IsFlagged);
        }
    }
}
