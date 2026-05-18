using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Учет.Models
{
    public partial class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? ParentDepartmentId { get; set; }

        public virtual Department? ParentDepartment { get; set; }
        public virtual ICollection<Department> InverseParentDepartment { get; set; } = new List<Department>();
        public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();

        public ObservableCollection<Department> Children { get; set; } = new ObservableCollection<Department>();
        public override string ToString()
        {
            return Name;
        }


    }
}