using System.Collections.Generic;
using System.Threading.Tasks;
using Учет.Models;

namespace Учет.Services
{
    public interface IValidationService
    {
        Dictionary<string, string> ValidateLocal(Asset asset);
        Task<bool> IsInventoryUniqueAsync(string inventoryNumber, int? excludeId = null);
        Task<bool> DepartmentExistsAsync(int departmentId);
        Task<bool> AssetTypeExistsAsync(int assetTypeId);
    }
}