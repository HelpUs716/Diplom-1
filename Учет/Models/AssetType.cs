using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Учет.Models
{
    public class AssetType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DefaultWarrantyPeriod { get; set; }
        public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
    }
}