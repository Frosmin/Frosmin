using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineSweeper
{
    public class TopMoves
    {
        public TopMoves()
        {

        }
        public TopMoves(string user)
        {
            TotalMoves = 1;
            User = user;
            DateTime = DateTime.Now;
        }
        public int TotalMoves { get; set; }
        public string User { get; set; }
        public DateTime DateTime { get; set; }
        public void AddMove()
        {
            TotalMoves++;
            DateTime = DateTime.Now;
        }
    }
}
