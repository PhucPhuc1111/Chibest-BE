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

public class ProductDetailService : IProductDetailService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductDetailService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IBusinessResult> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var productDetail = await _unitOfWork.ProductDetailRepository.GetByIdAsync(id);
        if (productDetail == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var response = productDetail.Adapt<ProductDetailResponse>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? productId = null,
        Guid? branchId = null,
        string? status = null)
    {
        try
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            Expression<Func<ProductDetail, bool>> predicate = x => true;

            if (productId.HasValue && productId != Guid.Empty)
            {
                predicate = predicate.And(x => x.ProductId == productId.Value);
            }

            if (branchId.HasValue && branchId != Guid.Empty)
            {
                predicate = predicate.And(x => x.BranchId == branchId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                predicate = predicate.And(x => x.Status == status);
            }

            Func<IQueryable<ProductDetail>, IOrderedQueryable<ProductDetail>> orderBy =
                q => q.OrderByDescending(x => x.ImportDate);

            var productDetails = await _unitOfWork.ProductDetailRepository.GetPagedAsync(
                pageNumber, pageSize, predicate, orderBy,
                include: q => q.Include(pd => pd.Product));

            var totalCount = await _unitOfWork.ProductDetailRepository.CountAsync();

            var response = productDetails.Adapt<List<ProductDetailResponse>>();
            var pagedResult = new PagedResult<ProductDetailResponse>
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

    public async Task<IBusinessResult> CreateAsync(ProductDetailRequest request)
    {
        try
        {
            if (request == null)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

            // Check if product exists
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(request.ProductId);
            if (product == null)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Product not found");

            // Check if chip code already exists
            if (!string.IsNullOrEmpty(request.ChipCode))
            {
                var existingDetail = await _unitOfWork.ProductDetailRepository
                    .GetByWhere(x => x.ChipCode == request.ChipCode)
                    .FirstOrDefaultAsync();

                if (existingDetail != null)
                    return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Chip code already exists");
            }

            var productDetail = request.Adapt<ProductDetail>();
            productDetail.Id = Guid.NewGuid();
            productDetail.LastTransactionDate = null;

            await _unitOfWork.ProductDetailRepository.AddAsync(productDetail);
            await _unitOfWork.SaveChangesAsync();

            var response = productDetail.Adapt<ProductDetailResponse>();
            return new BusinessResult(Const.HTTP_STATUS_CREATED, Const.SUCCESS_CREATE_MSG, response);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.HTTP_STATUS_INTERNAL_ERROR, ex.Message);
        }
    }

    public async Task<IBusinessResult> UpdateAsync(Guid id, ProductDetailRequest request)
    {
        try
        {
            if (id == Guid.Empty || request == null)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

            var productDetail = await _unitOfWork.ProductDetailRepository.GetByIdAsync(id);
            if (productDetail == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

            // Check for duplicate chip code
            if (!string.IsNullOrEmpty(request.ChipCode) && request.ChipCode != productDetail.ChipCode)
            {
                var duplicate = await _unitOfWork.ProductDetailRepository
                    .GetByWhere(x => x.ChipCode == request.ChipCode && x.Id != id)
                    .FirstOrDefaultAsync();

                if (duplicate != null)
                    return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Chip code already exists");
            }

            request.Adapt(productDetail);
            _unitOfWork.ProductDetailRepository.Update(productDetail);
            await _unitOfWork.SaveChangesAsync();

            var response = productDetail.Adapt<ProductDetailResponse>();
            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_UPDATE_MSG, response);
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

            var productDetail = await _unitOfWork.ProductDetailRepository.GetByIdAsync(id);
            if (productDetail == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

            _unitOfWork.ProductDetailRepository.Delete(productDetail);
            await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_DELETE_MSG);
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
            var productDetails = await _unitOfWork.ProductDetailRepository
                .GetByWhere(x => x.ProductId == productId && x.BranchId == branchId)
                .Include(x => x.Product)
                .ToListAsync();

            var response = productDetails.Adapt<List<ProductDetailResponse>>();
            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.HTTP_STATUS_INTERNAL_ERROR, ex.Message);
        }
    }
}
