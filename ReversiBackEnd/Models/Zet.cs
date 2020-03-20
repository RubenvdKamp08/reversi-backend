using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReversiBackEnd.Models
{
    public class Zet
    {
        public string SpelToken { get; set; }
        public string SpelerToken { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Pas { get; set; }
    }
}
