using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Repository;
using Chibest.Repository.Models;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Chibest.Service.ModelDTOs.Response;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var warehouse = await _unitOfWork.WarehouseRepository.GetByIdAsync(id);
            if (warehouse == null)
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);
            }
            var response = warehouse.Adapt<WarehouseResponse>();
            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
        }
        public async Task<IBusinessResult> CreateWarehouse(WarehouseRequest request)
        {
            if (request == null)
            {
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Invalid request");
            }

            var warehouseEntity = request.Adapt<Warehouse>();
            warehouseEntity.CreatedAt = DateTime.Now;
            warehouseEntity.UpdatedAt = DateTime.Now;
            await _unitOfWork.WarehouseRepository.AddAsync(warehouseEntity);
            await _unitOfWork.SaveChangesAsync();
            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_CREATE_MSG);
        }
        public async Task<IBusinessResult> GetWarehouseList(int pageIndex, int pageSize, string allergyName)
        {
            string searchTerm = allergyName?.ToLower() ?? string.Empty;

            var warehouses = await _unitOfWork.WarehouseRepository.GetPagedAsync(
                pageIndex,
                pageSize,
                x => string.IsNullOrEmpty(searchTerm) || x.Name.ToLower().Contains(searchTerm)
            );
            warehouses = warehouses.Distinct().ToList();
            if (warehouses == null || !warehouses.Any())
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);
            }

            var response = warehouses.Adapt<List<WarehouseResponse>>();
            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
        }
        public async Task<IBusinessResult> UpdateWarehouse(Guid id, WarehouseRequest request)
        {
            var warehouse = await _unitOfWork.WarehouseRepository.GetByIdAsync(id);
            if (warehouse == null)
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Warehouse not found");
            }
            warehouse.Name = request.Name;
            warehouse.Address = request.Address;
            warehouse.FaxNumber = request.FaxNumber;
            warehouse.UpdatedAt = DateTime.Now;
            await _unitOfWork.WarehouseRepository.UpdateAsync(warehouse);
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
