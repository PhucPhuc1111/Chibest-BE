using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Repository;
using Chibest.Repository.Models;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Chibest.Service.ModelDTOs.Request.Query;
using Chibest.Service.ModelDTOs.Response;
using Chibest.Service.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chibest.Service.Services;

public class ProductPlanService : IProductPlanService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductPlanService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IBusinessResult> GetListAsync(ProductPlanQuery query)
    {
        Expression<Func<ProductPlan, bool>> predicate = p => true;

        if (query.ProductId.HasValue)
            predicate = predicate.And(p => p.ProductId == query.ProductId.Value);

        if (query.SupplierId.HasValue)
            predicate = predicate.And(p => p.SupplierId == query.SupplierId.Value);

        if (!string.IsNullOrWhiteSpace(query.Status))
            predicate = predicate.And(p => p.Status == query.Status);

        if (query.FromSendDate.HasValue)
            predicate = predicate.And(p => p.SendDate >= query.FromSendDate.Value);

        if (query.ToSendDate.HasValue)
        {
            var endDate = query.ToSendDate.Value.Date.AddDays(1).AddSeconds(-1);
            predicate = predicate.And(p => p.SendDate <= endDate);
        }

        Func<IQueryable<ProductPlan>, IOrderedQueryable<ProductPlan>>? orderBy = null;
        if (!string.IsNullOrEmpty(query.SortBy))
        {
            orderBy = query.SortBy.ToLower() switch
            {
                "senddate" => q => query.SortDescending ? q.OrderByDescending(p => p.SendDate) : q.OrderBy(p => p.SendDate),
                "status" => q => query.SortDescending ? q.OrderByDescending(p => p.Status) : q.OrderBy(p => p.Status),
                _ => q => query.SortDescending ? q.OrderByDescending(p => p.CreatedAt) : q.OrderBy(p => p.CreatedAt)
            };
        }
        else
        {
            orderBy = q => query.SortDescending ? q.OrderByDescending(p => p.SendDate) : q.OrderBy(p => p.SendDate);
        }

        var plans = await _unitOfWork.ProductPlanRepository.GetPagedAsync(
            query.PageNumber,
            query.PageSize,
            predicate,
            orderBy,
            include: q => q.Include(p => p.Product)
                           .Include(p => p.Supplier));

        var totalCount = await _unitOfWork.ProductPlanRepository
            .GetByWhere(predicate)
            .CountAsync();

        var responses = plans.Select(MapToResponse).ToList();


        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, responses);
    }

    public async Task<IBusinessResult> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var plan = await _unitOfWork.ProductPlanRepository
            .GetByWhere(p => p.Id == id)
            .Include(p => p.Product)
            .Include(p => p.Supplier)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (plan == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, MapToResponse(plan));
    }

    public async Task<IBusinessResult> CreateAsync(ProductPlanRequest request, Guid accountId)
    {
        if (request == null)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        if (request.ProductId == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "ProductId là bắt buộc.");

        var product = await _unitOfWork.ProductRepository.GetByIdAsync(request.ProductId);
        if (product == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy sản phẩm.");

        if (request.SupplierId.HasValue)
        {
            var supplier = await _unitOfWork.AccountRepository.GetByIdAsync(request.SupplierId.Value);
            if (supplier == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy nhà cung cấp.");
        }

        var now = DateTime.Now;
        var entity = new ProductPlan
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            SupplierId = request.SupplierId,
            Type = request.Type,
            SendDate = request.SendDate ?? now,
            DetailAmount = request.DetailAmount,
            Amount = request.Amount,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Queue" : request.Status,
            Note = request.Note,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _unitOfWork.ProductPlanRepository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return new BusinessResult(Const.HTTP_STATUS_CREATED, Const.SUCCESS_CREATE_MSG);
    }

    public async Task<IBusinessResult> UpdateAsync(ProductPlanRequest request, Guid accountId)
    {
        if (request == null || !request.Id.HasValue || request.Id == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var plan = await _unitOfWork.ProductPlanRepository.GetByIdAsync(request.Id.Value);
        if (plan == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        if (request.ProductId != Guid.Empty && request.ProductId != plan.ProductId)
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(request.ProductId);
            if (product == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy sản phẩm.");
            plan.ProductId = request.ProductId;
        }

        if (request.SupplierId.HasValue && request.SupplierId != plan.SupplierId)
        {
            var supplier = await _unitOfWork.AccountRepository.GetByIdAsync(request.SupplierId.Value);
            if (supplier == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Không tìm thấy nhà cung cấp.");
            plan.SupplierId = request.SupplierId;
        }

        plan.Type = request.Type ?? plan.Type;
        plan.SendDate = request.SendDate ?? plan.SendDate;
        plan.DetailAmount = request.DetailAmount ?? plan.DetailAmount;
        plan.Amount = request.Amount ?? plan.Amount;
        if (!string.IsNullOrWhiteSpace(request.Status))
            plan.Status = request.Status;
        plan.Note = request.Note ?? plan.Note;
        plan.UpdatedAt = DateTime.Now;

        _unitOfWork.ProductPlanRepository.Update(plan);
        await _unitOfWork.SaveChangesAsync();

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_UPDATE_MSG);
    }

    public async Task<IBusinessResult> DeleteAsync(Guid id, Guid accountId)
    {
        if (id == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var plan = await _unitOfWork.ProductPlanRepository.GetByIdAsync(id);
        if (plan == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        _unitOfWork.ProductPlanRepository.Delete(plan);
        await _unitOfWork.SaveChangesAsync();

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_DELETE_MSG);
    }

    private static ProductPlanResponse MapToResponse(ProductPlan plan)
    {
        return new ProductPlanResponse
        {
            Id = plan.Id,
            ProductId = plan.ProductId,
            SupplierId = plan.SupplierId,
            Type = plan.Type,
            SendDate = plan.SendDate,
            DetailAmount = plan.DetailAmount,
            Amount = plan.Amount,
            Status = plan.Status,
            Note = plan.Note,
            CreatedAt = plan.CreatedAt,
            UpdatedAt = plan.UpdatedAt,
            ProductName = plan.Product?.Name,
            SupplierName = plan.Supplier?.Name,
            AvatarUrl = plan.Product?.AvatarUrl,
            VideoUrl = plan.Product?.VideoUrl,
        };
    }
}

