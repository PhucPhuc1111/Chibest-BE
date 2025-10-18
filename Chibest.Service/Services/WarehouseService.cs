using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Repository;
using Chibest.Repository.Models;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Chibest.Service.ModelDTOs.Response;
using Microsoft.EntityFrameworkCore;

namespace Chibest.Service.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WarehouseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IBusinessResult> GetWarehouseById(Guid id)
        {
            var warehouse = await _unitOfWork.WarehouseRepository
                .GetByWhere(x => x.Id == id)
                .Include(x => x.Branch)
                .FirstOrDefaultAsync();

            if (warehouse == null)
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);
            }

            var response = new WarehouseResponse
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Address = warehouse.Address,
                BranchName = warehouse.Branch?.Name,
                CreatedAt = warehouse.CreatedAt,
                UpdatedAt = warehouse.UpdatedAt
            };

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
        }

        public async Task<IBusinessResult> CreateWarehouse(WarehouseRequest request)
        {
            if (request == null)
            {
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Invalid request");
            }

            var warehouseEntity = new Warehouse
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Address = request.Address,
                BranchId = request.BranchId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _unitOfWork.WarehouseRepository.AddAsync(warehouseEntity);
            await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_CREATE_MSG);
        }

        public async Task<IBusinessResult> GetWarehouseList(int pageIndex, int pageSize, string search)
        {
            string searchTerm = search?.Trim().ToLower() ?? string.Empty;
            var query = _unitOfWork.WarehouseRepository
                .GetByWhere(x => string.IsNullOrEmpty(searchTerm) || x.Name.ToLower().Contains(searchTerm))
                .Include(x => x.Branch);
            var warehouses = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (warehouses == null || !warehouses.Any())
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

            var response = warehouses.Select(x => new WarehouseResponse
            {
                Id = x.Id,
                Name = x.Name,
                Address = x.Address,
                BranchName = x.Branch?.Name,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            }).ToList();

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
        }


        public async Task<IBusinessResult> UpdateWarehouse(Guid id, WarehouseRequest request)
        {
            var warehouseEntity = await _unitOfWork.WarehouseRepository.GetByIdAsync(id);
            if (warehouseEntity == null)
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Warehouse not found");
            }

            warehouseEntity.Name = request.Name;
            warehouseEntity.Address = request.Address;
            warehouseEntity.BranchId = request.BranchId;
            warehouseEntity.Status = string.IsNullOrEmpty(request.Status) ? warehouseEntity.Status : request.Status;
            warehouseEntity.UpdatedAt = DateTime.Now;

            await _unitOfWork.WarehouseRepository.UpdateAsync(warehouseEntity);
            await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_UPDATE_MSG);
        }

        public async Task<IBusinessResult> DeleteWarehouse(Guid id)
        {
            var warehouse = await _unitOfWork.WarehouseRepository.GetByIdAsync(id);
            if (warehouse == null)
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Warehouse not found");
            }

            await _unitOfWork.WarehouseRepository.DeleteAsync(warehouse);
            await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_DELETE_MSG);
        }
    }
}
