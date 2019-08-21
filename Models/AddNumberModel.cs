using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sudoku.Models
{
    public class AddNumberModel
    {
        public bool IsNumberCorrect { get; set; }
        public bool IsNumberAdded { get; set; }
        public bool IsGameOver { get; set; }
    }
}