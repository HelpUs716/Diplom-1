using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Учет.Models
{
    public class Asset
    {
        public int Id { get; set; }
        public string ShortName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string InventoryNumber { get; set; } = string.Empty;
        public DateTime CommissioningDate { get; set; }
        public DateTime PlannedWriteOffDate { get; set; }
        public int Status { get; set; }
        public int RepairCount { get; set; }
        public int DepartmentId { get; set; }
        public int AssetTypeId { get; set; }
        public virtual Department Department { get; set; } = null!;
        public virtual AssetType AssetType { get; set; } = null!;
        public virtual ICollection<Repair> Repairs { get; set; } = new List<Repair>();
    }
}