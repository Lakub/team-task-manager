using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamTaskManager.Models.Entities
{
    public class Score
    {
        public int ScoreId { get; set; }
        public string UserName { get; set; }
        public int Points{ get; set; }
    }
}
