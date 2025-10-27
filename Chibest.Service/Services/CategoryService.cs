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
    private readonly ISystemLogService _systemLogService;

    public CategoryService(IUnitOfWork unitOfWork, ISystemLogService systemLogService)
    {
        _unitOfWork = unitOfWork;
        _systemLogService = systemLogService;
    }

    public async Task<IBusinessResult> GetListAsync(CategoryQuery query)
    {
        Expression<Func<Category, bool>> predicate = c => true;

        if (!string.IsNullOrEmpty(query.Type))
        {
            predicate = predicate.And(c => c.Type.Contains(query.Type));
        }

        if (!string.IsNullOrEmpty(query.Name))
        {
            predicate = predicate.And(c => c.Name.Contains(query.Name));
        }

        if (query.ParentId.HasValue)
        {
            predicate = predicate.And(c => c.ParentId == query.ParentId.Value);
        }
        else if (query.OnlyRoot.HasValue && query.OnlyRoot.Value)
        {
            predicate = predicate.And(c => c.ParentId == null);
        }

        Func<IQueryable<Category>, IOrderedQueryable<Category>> orderBy = null;
        if (!string.IsNullOrEmpty(query.SortBy))
        {
            orderBy = query.SortBy.ToLower() switch
            {
                "name" => q => query.SortDescending ? q.OrderByDescending(c => c.Name) : q.OrderBy(c => c.Name),
                "type" => q => query.SortDescending ? q.OrderByDescending(c => c.Type) : q.OrderBy(c => c.Type),
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
            response.ProductCount = await _unitOfWork.ProductRepository.GetByWhere(p => p.CategoryId == category.Id).CountAsync();
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

    public async Task<IBusinessResult> GetByTypeAsync(string type)
    {
        var categories = await _unitOfWork.CategoryRepository.GetByWhere(c => c.Type == type)
            .ToListAsync();

        var response = categories.Adapt<IEnumerable<CategoryResponse>>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> GetHierarchyAsync()
    {
        var allCategories = await _unitOfWork.CategoryRepository.GetAll().ToListAsync();
        var rootCategories = allCategories.Where(c => c.ParentId == null).ToList();

        var hierarchy = BuildCategoryHierarchy(rootCategories, allCategories);
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, hierarchy);
    }

    public async Task<IBusinessResult> GetChildrenAsync(Guid parentId)
    {
        var children = await _unitOfWork.CategoryRepository.GetByWhere(c => c.ParentId == parentId)
            .ToListAsync();

        var response = children.Adapt<IEnumerable<CategoryResponse>>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> CreateAsync(CategoryRequest request, Guid accountId)
    {
        if (request == null)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, Const.ERROR_EXCEPTION_MSG);

        // Check if category with same name and type already exists
        var existingCategory = await _unitOfWork.CategoryRepository.GetByWhere(c =>
            c.Name == request.Name && c.Type == request.Type)
            .FirstOrDefaultAsync();

        if (existingCategory != null)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Đã tồn tại danh mục với tên và loại này");

        var category = request.Adapt<Category>();
        category.Id = Guid.NewGuid();

        await _unitOfWork.CategoryRepository.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        await LogSystemAction("Create", "Category", category.Id, accountId,
                            null, JsonSerializer.Serialize(category),
                            $"Tạo mới danh mục: {category.Name} (Type: {category.Type})");

        var response = category.Adapt<CategoryResponse>();
        return new BusinessResult(Const.HTTP_STATUS_CREATED, Const.SUCCESS_CREATE_MSG, response);
    }

    public async Task<IBusinessResult> UpdateAsync(CategoryRequest request, Guid accountId)
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
            c.Name == request.Name &&
            c.Type == request.Type)
            .FirstOrDefaultAsync();

        if (duplicateCategory != null)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Đã tồn tại danh mục với tên và loại này");

        request.Adapt(existing);

        _unitOfWork.CategoryRepository.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        await LogSystemAction("Update", "Category", request.Id.Value, accountId,
                            oldValue, JsonSerializer.Serialize(existing),
                            $"Cập nhật danh mục: {oldName} → {existing.Name}");

        var response = existing.Adapt<CategoryResponse>();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_UPDATE_MSG, response);
    }

    public async Task<IBusinessResult> DeleteAsync(Guid id, Guid accountId)
    {
        var existing = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
        if (existing == null)
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);

        // Check if category has children
        var hasChildren = await _unitOfWork.CategoryRepository.GetByWhere(c => c.ParentId == id)
            .AnyAsync();
        if (hasChildren)
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Không thể xóa danh mục có danh mục con");

        var oldValue = JsonSerializer.Serialize(existing);

        _unitOfWork.CategoryRepository.Delete(existing);
        await _unitOfWork.SaveChangesAsync();

        await LogSystemAction("Delete", "Category", id, accountId,
                            oldValue, null,
                            $"Xóa danh mục: {existing.Name}");

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_DELETE_MSG);
    }

    private List<CategoryResponse> BuildCategoryHierarchy(List<Category> parentCategories, List<Category> allCategories)
    {
        var result = new List<CategoryResponse>();

        foreach (var parent in parentCategories)
        {
            var parentResponse = parent.Adapt<CategoryResponse>();
            var children = allCategories.Where(c => c.ParentId == parent.Id).ToList();

            if (children.Any())
            {
                parentResponse.Children = BuildCategoryHierarchy(children, allCategories);
            }

            result.Add(parentResponse);
        }

        return result;
    }

    private async Task LogSystemAction(string action, string entityType, Guid entityId, Guid accountId,
                                     string? oldValue, string? newValue, string description)
    {
        var account = await _unitOfWork.AccountRepository
            .GetByWhere(acc => acc.Id == accountId)
            .AsNoTracking().FirstOrDefaultAsync();
        var logRequest = new SystemLogRequest
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValue = oldValue,
            NewValue = newValue,
            Description = description,
            AccountId = accountId,
            AccountName = account != null ? account.Name : null,
            Module = "Category",
            LogLevel = "INFO"
        };

        await _systemLogService.CreateAsync(logRequest);
    }
}