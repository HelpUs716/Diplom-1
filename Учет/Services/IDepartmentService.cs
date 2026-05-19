using System.Collections.Generic;
using System.Threading.Tasks;
using Учет.Models;

namespace Учет.Services
{
    public interface IDepartmentService    
        Task<bool> IsNameUniqueAsync(string name, int? parentId, int? excludeId = null);
        Task<bool> HasDependenciesAsync(int departmentId);
        List<Department> BuildHierarchy(IEnumerable<Department> flatList);
    }
}
