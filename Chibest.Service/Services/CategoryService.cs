using Azure.Core;
using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Repository;
using Chibest.Repository.Models;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Chibest.Service.ModelDTOs.Request.Query;
using Chibest.Service.ModelDTOs.Response;
using Chibest.Service.Utilities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Text.Json;

namespace Chibest.Service.Services;
public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IBusinessResult> GetListAsync(CategoryQuery query)
    {
        Expression<Func<Category, bool>> predicate = c => true;

        if (!string.IsNullOrEmpty(query.Name))
        {
            predicate = predicate.And(c => c.Name.Contains(query.Name));
        }

        Func<IQueryable<Category>, IOrderedQueryable<Category>>? orderBy = null;
        if (!string.IsNullOrEmpty(query.SortBy))
        {
            orderBy = query.SortBy.ToLower() switch
            {
                "name" => q => query.SortDescending ? q.OrderByDescending(c => c.Name) : q.OrderBy(c => c.Name),
                _ => q => query.SortDescending ? q.OrderByDescending(c => c.Name) : q.OrderBy(c => c.Name)
            };
        }

        var categories = await _unitOfWork.CategoryRepository.GetPagedAsync(
            query.PageNumber,
            query.PageSize,
            predicate,
            orderBy
        );

        var totalCount = await _unitOfWork.CategoryRepository.GetByWhere(predicate).CountAsync();

        // Get product counts for each category
        var categoryResponses = new List<CategoryResponse>();
        foreach (var category in categories)
        {
            var response = category.Adapt<CategoryResponse>();
            categoryResponses.Add(response);
        }

        var pagedResult = new PagedResult<CategoryResponse>
        {
            DataList = categoryResponses,
            TotalCount = totalCount,
            PageIndex = query.PageNumber,
            PageSize = query.PageSize
        };

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, pagedResult);
    }

    public async Task<IBusinessResult> GetByIdAsync(Guid id)
    {
        var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
        if (category == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var response = category.Adapt<CategoryResponse>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> CreateAsync(CategoryRequest request)
    {
        if (request == null)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        // Check if category with same name and type already exists
        var existingCategory = await _unitOfWork.CategoryRepository.GetByWhere(c =>
            c.Name == request.Name )
            .FirstOrDefaultAsync();

        if (existingCategory != null)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Đã tồn tại danh mục với tên và loại này");

        var category = request.Adapt<Category>();
        category.Id = Guid.NewGuid();

        await _unitOfWork.CategoryRepository.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        var response = category.Adapt<CategoryResponse>();
        return new BusinessResult(Const.HTTP_STATUS_CREATED, Const.SUCCESS_CREATE_MSG, response);
    }

    public async Task<IBusinessResult> UpdateAsync(CategoryRequest request)
    {
        if (request.Id.HasValue == false || request.Id == Guid.Empty)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        var existing = await _unitOfWork.CategoryRepository.GetByIdAsync(request.Id);
        if (existing == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        var oldValue = JsonSerializer.Serialize(existing);
        var oldName = existing.Name;

        // Check for duplicate name and type (excluding current category)
        var duplicateCategory = await _unitOfWork.CategoryRepository.GetByWhere(c =>
            c.Id != request.Id &&
            c.Name == request.Name)
            .FirstOrDefaultAsync();

        if (duplicateCategory != null)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Đã tồn tại danh mục với tên và loại này");

        request.Adapt(existing);

        _unitOfWork.CategoryRepository.Update(existing);
        await _unitOfWork.SaveChangesAsync();


        var response = existing.Adapt<CategoryResponse>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_UPDATE_MSG, response);
    }

    public async Task<IBusinessResult> DeleteAsync(Guid id)
    {
        var existing = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
        if (existing == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);


        var oldValue = JsonSerializer.Serialize(existing);

        _unitOfWork.CategoryRepository.Delete(existing);
        await _unitOfWork.SaveChangesAsync();

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_DELETE_MSG);
    }

   
}