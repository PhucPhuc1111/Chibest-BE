using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Repository;
using Chibest.Repository.Models;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Chibest.Service.ModelDTOs.Response;
using Chibest.Service.Utilities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chibest.Service.Services;

public class BranchStockService : IBranchStockService
{
    private readonly IUnitOfWork _unitOfWork;

    public BranchStockService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IBusinessResult> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var branchStock = await _unitOfWork.BranchStockRepository.GetByIdAsync(id);
        if (branchStock == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var response = branchStock.Adapt<BranchStockResponse>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> GetPagedAsync(int pageNumber, int pageSize, Guid? productId = null, Guid? branchId = null)
    {
        try
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            Expression<Func<BranchStock, bool>> predicate = x => true;

            if (productId.HasValue && productId != Guid.Empty)
            {
                predicate = predicate.And(x => x.ProductId == productId.Value);
            }

            if (branchId.HasValue && branchId != Guid.Empty)
            {
                predicate = predicate.And(x => x.BranchId == branchId.Value);
            }

            Func<IQueryable<BranchStock>, IOrderedQueryable<BranchStock>> orderBy =
                q => q.OrderBy(x => x.Product.Name);

            var branchStocks = await _unitOfWork.BranchStockRepository.GetPagedAsync(
                pageNumber, pageSize, predicate, orderBy,
                include: q => q.Include(bs => bs.Product).Include(bs => bs.Branch));

            var totalCount = await _unitOfWork.BranchStockRepository.CountAsync();

            var response = branchStocks.Adapt<List<BranchStockResponse>>();
            var pagedResult = new PagedResult<BranchStockResponse>
            {
                DataList = response,
                TotalCount = totalCount,
                PageIndex = pageNumber,
                PageSize = pageSize
            };

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, pagedResult);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.HTTP_STATUS_INTERNAL_ERROR, ex.Message);
        }
    }

    public async Task<IBusinessResult> CreateAsync(BranchStockRequest request)
    {
        try
        {
            if (request == null)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

            // Check if product exists
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(request.ProductId);
            if (product == null)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Product not found");

            // Check if stock record already exists for this product and branch
            var existingStock = await _unitOfWork.BranchStockRepository
                .GetByWhere(x => x.ProductId == request.ProductId && x.BranchId == request.BranchId)
                .FirstOrDefaultAsync();

            if (existingStock != null)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Stock record already exists for this product and branch");

            var branchStock = request.Adapt<BranchStock>();
            branchStock.Id = Guid.NewGuid();
            branchStock.LastUpdated = DateTime.UtcNow;

            await _unitOfWork.BranchStockRepository.AddAsync(branchStock);
            await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_CREATED, Const.SUCCESS_CREATE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.HTTP_STATUS_INTERNAL_ERROR, ex.Message);
        }
    }

    public async Task<IBusinessResult> UpdateAsync(Guid id, BranchStockRequest request)
    {
        try
        {
            if (id == Guid.Empty || request == null)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

            var branchStock = await _unitOfWork.BranchStockRepository.GetByIdAsync(id);
            if (branchStock == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

            request.Adapt(branchStock);
            branchStock.LastUpdated = DateTime.UtcNow;

            _unitOfWork.BranchStockRepository.Update(branchStock);
            await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_UPDATE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.HTTP_STATUS_INTERNAL_ERROR, ex.Message);
        }
    }

    public async Task<IBusinessResult> DeleteAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

            var branchStock = await _unitOfWork.BranchStockRepository.GetByIdAsync(id);
            if (branchStock == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

            _unitOfWork.BranchStockRepository.Delete(branchStock);
            await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_DELETE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.HTTP_STATUS_INTERNAL_ERROR, ex.Message);
        }
    }

    public async Task<IBusinessResult> UpdateStockAsync(Guid id, int availableQty, int reservedQty)
    {
        try
        {
            if (id == Guid.Empty)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

            var branchStock = await _unitOfWork.BranchStockRepository.GetByIdAsync(id);
            if (branchStock == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

            branchStock.AvailableQty = availableQty;
            branchStock.ReservedQty = reservedQty;
            branchStock.LastUpdated = DateTime.UtcNow;

            _unitOfWork.BranchStockRepository.Update(branchStock);
            await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_UPDATE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.HTTP_STATUS_INTERNAL_ERROR, ex.Message);
        }
    }

    public async Task<IBusinessResult> GetByProductAndBranchAsync(Guid productId, Guid branchId)
    {
        try
        {
            var branchStock = await _unitOfWork.BranchStockRepository
                .GetByWhere(x => x.ProductId == productId && x.BranchId == branchId)
                .Include(x => x.Product)
                .Include(x => x.Branch)
                .FirstOrDefaultAsync();

            if (branchStock == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Stock record not found");

            var response = branchStock.Adapt<BranchStockResponse>();
            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.HTTP_STATUS_INTERNAL_ERROR, ex.Message);
        }
    }

    public async Task<IBusinessResult> GetLowStockItemsAsync(Guid branchId)
    {
        try
        {
            var lowStockItems = await _unitOfWork.BranchStockRepository
                .GetByWhere(x => x.BranchId == branchId && x.AvailableQty <= x.MinimumStock)
                .Include(x => x.Product)
                .OrderBy(x => x.AvailableQty)
                .ToListAsync();

            var response = lowStockItems.Adapt<List<BranchStockResponse>>();
            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.HTTP_STATUS_INTERNAL_ERROR, ex.Message);
        }
    }
}