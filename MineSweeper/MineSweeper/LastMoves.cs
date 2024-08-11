using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineSweeper
{
    public class LastMoves
    {
        public LastMoves()
        {

        }
        public LastMoves(string move, string user)
        {
            this.Move = move;
            this.User = user;
            DateTime = DateTime.Now;
        }
        public string Move { get; set; }
        public string User { get; set; }
        public DateTime DateTime { get; set; }
    }
}
