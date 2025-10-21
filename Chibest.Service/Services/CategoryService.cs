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
public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IBusinessResult> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
        if (category == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var response = category.Adapt<CategoryResponse>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> GetPagedAsync(int pageNumber, int pageSize, string? search = null, string? type = null)
    {
        try
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            Expression<Func<Category, bool>> predicate = x => true;

            if (!string.IsNullOrWhiteSpace(search))
            {
                predicate = x => x.Name.Contains(search) ||
                                x.Description.Contains(search);
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                predicate = predicate.And(x => x.Type == type);
            }

            Func<IQueryable<Category>, IOrderedQueryable<Category>> orderBy =
                q => q.OrderBy(x => x.Name);

            var categories = await _unitOfWork.CategoryRepository.GetPagedAsync(
                pageNumber, pageSize, predicate, orderBy);

            var totalCount = await _unitOfWork.CategoryRepository.CountAsync();

            var response = categories.Adapt<List<CategoryResponse>>();
            var pagedResult = new PagedResult<CategoryResponse>
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

    public async Task<IBusinessResult> CreateAsync(CategoryRequest request)
    {
        try
        {
            if (request == null)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

            // Check if category name already exists
            var existingCategory = await _unitOfWork.CategoryRepository
                .GetByWhere(x => x.Name == request.Name && x.Type == request.Type)
                .FirstOrDefaultAsync();

            if (existingCategory != null)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Category name already exists for this type");

            var category = request.Adapt<Category>();
            category.Id = Guid.NewGuid();

            await _unitOfWork.CategoryRepository.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_CREATED, Const.SUCCESS_CREATE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.HTTP_STATUS_INTERNAL_ERROR, ex.Message);
        }
    }

    public async Task<IBusinessResult> UpdateAsync(Guid id, CategoryRequest request)
    {
        try
        {
            if (id == Guid.Empty || request == null)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
            if (category == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

            // Check for duplicate name
            if (!string.IsNullOrEmpty(request.Name) && request.Name != category.Name)
            {
                var duplicate = await _unitOfWork.CategoryRepository
                    .GetByWhere(x => x.Name == request.Name && x.Type == (request.Type ?? category.Type) && x.Id != id)
                    .FirstOrDefaultAsync();

                if (duplicate != null)
                    return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Category name already exists for this type");
            }

            request.Adapt(category);
            await _unitOfWork.CategoryRepository.UpdateAsync(category);
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

            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
            if (category == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

            // Check if category has products
            var hasProducts = await _unitOfWork.ProductRepository
                .GetByWhere(x => x.CategoryId == id)
                .AnyAsync();

            if (hasProducts)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Cannot delete category that has products");

            await _unitOfWork.CategoryRepository.DeleteAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_DELETE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.HTTP_STATUS_INTERNAL_ERROR, ex.Message);
        }
    }

    public async Task<IBusinessResult> GetCategoriesWithProductsAsync()
    {
        try
        {
            var categories = await _unitOfWork.CategoryRepository
                .GetAll()
                .Include(x => x.Products)
                .OrderBy(x => x.Name)
                .ToListAsync();

            var response = categories.Adapt<List<CategoryResponse>>();
            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.HTTP_STATUS_INTERNAL_ERROR, ex.Message);
        }
    }
}