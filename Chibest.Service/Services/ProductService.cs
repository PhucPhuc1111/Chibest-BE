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

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IBusinessResult> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var product = await _unitOfWork.ProductRepository.GetByIdAsync(id);
        if (product == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var response = product.Adapt<ProductResponse>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        Guid? categoryId = null,
        string? status = null)
    {
        try
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            Expression<Func<Product, bool>> predicate = x => true;

            if (!string.IsNullOrWhiteSpace(search))
            {
                predicate = x => x.Sku.Contains(search) ||
                                x.Name.Contains(search) ||
                                x.Description.Contains(search) ||
                                x.Brand.Contains(search);
            }

            if (categoryId.HasValue && categoryId != Guid.Empty)
            {
                predicate = predicate.And(x => x.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                predicate = predicate.And(x => x.Status == status);
            }

            Func<IQueryable<Product>, IOrderedQueryable<Product>> orderBy =
                q => q.OrderBy(x => x.Name);

            var products = await _unitOfWork.ProductRepository.GetPagedAsync(
                pageNumber, pageSize, predicate, orderBy,
                include: q => q.Include(p => p.Category));

            var totalCount = await _unitOfWork.ProductRepository.CountAsync();

            var response = products.Adapt<List<ProductResponse>>();
            var pagedResult = new PagedResult<ProductResponse>
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

    public async Task<IBusinessResult> CreateAsync(ProductRequest request)
    {
        try
        {
            if (request == null)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

            // Check if SKU already exists
            var existingProduct = await _unitOfWork.ProductRepository
                .GetByWhere(x => x.Sku == request.Sku)
                .FirstOrDefaultAsync();

            if (existingProduct != null)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Product SKU already exists");

            // Check if category exists
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(request.CategoryId);
            if (category == null)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Category not found");

            var product = request.Adapt<Product>();
            product.Id = Guid.NewGuid();

            await _unitOfWork.ProductRepository.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_CREATED, Const.SUCCESS_CREATE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.HTTP_STATUS_INTERNAL_ERROR, ex.Message);
        }
    }

    public async Task<IBusinessResult> UpdateAsync(Guid id, ProductRequest request)
    {
        try
        {
            if (id == Guid.Empty || request == null)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

            var product = await _unitOfWork.ProductRepository.GetByIdAsync(id);
            if (product == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

            // Check for duplicate SKU
            if (!string.IsNullOrEmpty(request.Sku) && request.Sku != product.Sku)
            {
                var duplicate = await _unitOfWork.ProductRepository
                    .GetByWhere(x => x.Sku == request.Sku && x.Id != id)
                    .FirstOrDefaultAsync();

                if (duplicate != null)
                    return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Product SKU already exists");
            }

            // Check if category exists if updating
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(request.CategoryId);
            if (category == null)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Category not found");

            request.Adapt(product);
            _unitOfWork.ProductRepository.Update(product);
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

            var product = await _unitOfWork.ProductRepository.GetByIdAsync(id);
            if (product == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

            // Check if product has details
            var hasDetails = await _unitOfWork.ProductDetailRepository
                .GetByWhere(x => x.ProductId == id)
                .AnyAsync();

            if (hasDetails)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Cannot delete product that has details");

            _unitOfWork.ProductRepository.Delete(product);
            await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_DELETE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.HTTP_STATUS_INTERNAL_ERROR, ex.Message);
        }
    }

    public async Task<IBusinessResult> UpdateStatusAsync(Guid id, string status)
    {
        try
        {
            if (id == Guid.Empty || string.IsNullOrEmpty(status))
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

            var product = await _unitOfWork.ProductRepository.GetByIdAsync(id);
            if (product == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

            product.Status = status;
            _unitOfWork.ProductRepository.Update(product);
            await _unitOfWork.SaveChangesAsync();

            var response = product.Adapt<ProductResponse>();
            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_UPDATE_MSG, response);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.HTTP_STATUS_INTERNAL_ERROR, ex.Message);
        }
    }
}