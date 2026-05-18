using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Учет.Data.Interfaces;
using Учет.Models;

namespace Учет.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IUnitOfWork _uow;

        public DepartmentService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<bool> IsNameUniqueAsync(string name, int? parentId, int? excludeId = null)
        {
            var departments = await _uow.Departments.GetAllAsync();

            return !departments.Any(d =>
                d.Name.Trim().Equals(name.Trim(), System.StringComparison.OrdinalIgnoreCase) &&
                d.ParentDepartmentId == parentId &&
                d.Id != excludeId);
        }

        public async Task<bool> HasDependenciesAsync(int departmentId)
        {
            var departments = await _uow.Departments.GetAllAsync();
            var assets = await _uow.Assets.GetAllAsync();

            bool hasChildren = departments.Any(d => d.ParentDepartmentId == departmentId);

            bool hasAssets = assets.Any(a => a.DepartmentId == departmentId);

            return hasChildren || hasAssets;
        }

        public List<Department> BuildHierarchy(IEnumerable<Department> flatList)
        {
            if (flatList == null || !flatList.Any())
                return new List<Department>();

            var dict = flatList.ToDictionary(d => d.Id);
            var roots = new List<Department>();

            foreach (var dept in flatList)
            {
                if (dept.ParentDepartmentId.HasValue && dict.TryGetValue(dept.ParentDepartmentId.Value, out var parent))
                {
                    parent.Children.Add(dept);
                }
                else
                {
                    roots.Add(dept);
                }
            }

            return roots;
        }

        public async Task<string> GetDepartmentPathAsync(int departmentId)
        {
            var departments = await _uow.Departments.GetAllAsync();
            var path = new List<string>();
            var currentId = departmentId;

            while (true)
            {
                var dept = departments.FirstOrDefault(d => d.Id == currentId);
                if (dept == null) break;

                path.Insert(0, dept.Name);

                if (!dept.ParentDepartmentId.HasValue)
                    break;

                currentId = dept.ParentDepartmentId.Value;
            }

            return string.Join(" / ", path);
        }

        public List<Department> BuildHierarchy(List<Department> flatList)
        {
            var lookup = flatList.ToLookup(d => d.ParentDepartmentId);
            foreach (var dept in flatList)
            {
                dept.Children = new ObservableCollection<Department>(lookup[dept.Id]);
            }
            return lookup[null].ToList();
        }

        public List<int> GetAllChildDepartmentIds(int parentDepartmentId, IEnumerable<Department> allDepartments)
        {
            var result = new List<int> { parentDepartmentId };
            var children = allDepartments.Where(d => d.ParentDepartmentId == parentDepartmentId);

            foreach (var child in children)
            {
                result.AddRange(GetAllChildDepartmentIds(child.Id, allDepartments));
            }

            return result;
        }
    }
}