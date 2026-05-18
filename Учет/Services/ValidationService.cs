using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Учет.Data.Interfaces;
using Учет.Models;

namespace Учет.Services
{
    public class ValidationService : IValidationService
    {
        private readonly IUnitOfWork _uow;

        public ValidationService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public Dictionary<string, string> ValidateLocal(Asset asset)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(asset.ShortName))
                errors[nameof(asset.ShortName)] = "Краткое название обязательно для заполнения";

            if (string.IsNullOrWhiteSpace(asset.InventoryNumber))
                errors[nameof(asset.InventoryNumber)] = "Инвентарный номер обязателен для заполнения";

            if (asset.CommissioningDate == default)
                errors[nameof(asset.CommissioningDate)] = "Дата ввода в эксплуатацию обязательна";
            else if (asset.CommissioningDate > DateTime.Today)
                errors[nameof(asset.CommissioningDate)] = "Дата ввода не может быть в будущем";

            if (asset.DepartmentId == 0)
                errors[nameof(asset.DepartmentId)] = "Необходимо выбрать отдел";

            if (asset.AssetTypeId == 0)
                errors[nameof(asset.AssetTypeId)] = "Необходимо выбрать тип оборудования";

            return errors;
        }

        public async Task<bool> IsInventoryUniqueAsync(string inventoryNumber, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(inventoryNumber))
                return false;

            var assets = await _uow.Assets.GetAllAsync();
            return !assets.Any(a =>
                a.InventoryNumber.Equals(inventoryNumber.Trim(), StringComparison.OrdinalIgnoreCase) &&
                a.Id != excludeId);
        }

        public async Task<bool> DepartmentExistsAsync(int departmentId)
        {
            if (departmentId == 0) return false;
            var departments = await _uow.Departments.GetAllAsync();
            return departments.Any(d => d.Id == departmentId);
        }

        public async Task<bool> AssetTypeExistsAsync(int assetTypeId)
        {
            if (assetTypeId == 0) return false;

            if (_uow.AssetTypes != null)
            {
                var assetTypes = await _uow.AssetTypes.GetAllAsync();
                return assetTypes.Any(t => t.Id == assetTypeId);
            }

            return true;
        }
    }
}