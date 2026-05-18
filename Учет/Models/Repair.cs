using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Учет.Models
{
    public class Repair
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Description { get; set; } = string.Empty;

        public virtual Asset? Asset { get; set; }
    }
}