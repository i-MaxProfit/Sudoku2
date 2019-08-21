using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sudoku.Models
{
    public class PlayerModel
    {
        public string ConnectionId { get; set; }
        public string Name { get; set; }
        public int WinsCount { get; set; }
    }
}