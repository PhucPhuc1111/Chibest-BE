using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Repository;
using Chibest.Repository.Models;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Chibest.Service.ModelDTOs.Response;
using Microsoft.EntityFrameworkCore;

public class BranchService : IBranchService
{
    private readonly IUnitOfWork _unitOfWork;

    public BranchService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IBusinessResult> GetBranchById(Guid id)
    {
        var branch = await _unitOfWork.BranchRepository
            .GetByWhere(x => x.Id == id)
            .Include(x => x.AccountRoles)
            .FirstOrDefaultAsync();

        if (branch == null)
        {
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);
        }

        var response = new BranchResponse
        {
            Id = branch.Id,
            Name = branch.Name,
            Code = branch.Code,
            Address = branch.Address,
            PhoneNumber = branch.PhoneNumber,
            IsFranchise = branch.IsFranchise,
            Status = branch.Status,
            UserCount = branch.AccountRoles?.Count ?? 0,
        };

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, response);
    }

    public async Task<IBusinessResult> CreateBranch(BranchRequest request)
    {
        if (request == null)
        {
            return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Invalid request");
        }

        var now = DateTime.Now;

        var branchEntity = new Branch
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Code = request.Code,
            Address = request.Address,
            PhoneNumber = request.PhoneNumber,
            IsFranchise = request.IsFranchise,
            CreatedAt = now,
            UpdatedAt = now,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status!
        };

        await _unitOfWork.BranchRepository.AddAsync(branchEntity);
        await _unitOfWork.SaveChangesAsync();

        // Create default BranchDebt for new branch
        var existingBranchDebt = await _unitOfWork.BranchDebtRepository
            .GetByWhere(x => x.BranchId == branchEntity.Id)
            .FirstOrDefaultAsync();

        if (existingBranchDebt == null)
        {
            var branchDebt = new BranchDebt
            {
                Id = Guid.NewGuid(),
                BranchId = branchEntity.Id,
                TotalDebt = 0,
                PaidAmount = 0,
                ReturnAmount = 0,
                LastTransactionDate = now,
                LastUpdated = now
            };
            await _unitOfWork.BranchDebtRepository.AddAsync(branchDebt);
            await _unitOfWork.SaveChangesAsync();
        }


        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_CREATE_MSG);
    }

    public async Task<IBusinessResult> GetBranchList(int pageIndex, int pageSize, string search)
    {
        string searchTerm = search?.ToLower() ?? string.Empty;

        var branches = await _unitOfWork.BranchRepository.GetPagedAsync(
            pageIndex,
            pageSize,
            x => string.IsNullOrEmpty(searchTerm) || x.Name.ToLower().Contains(searchTerm),
            include: q => q.Include(b => b.AccountRoles)
        );

        if (branches == null || !branches.Any())
        {
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);
        }

        var responseList = branches.Select(branch => new BranchResponse
        {
            Id = branch.Id,
            Code = branch.Code,
            Name = branch.Name,
            Address = branch.Address,
            PhoneNumber = branch.PhoneNumber,
            IsFranchise = branch.IsFranchise,
            Status = branch.Status,
            UserCount = branch.AccountRoles?.Count ?? 0,
        }).ToList();

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, responseList);
    }

    public async Task<IBusinessResult> UpdateBranch(Guid id, BranchRequest request)
    {
        var branchEntity = await _unitOfWork.BranchRepository.GetByIdAsync(id);
        if (branchEntity == null)
        {
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Branch not found");
        }
        branchEntity.Name = request.Name;
        branchEntity.Code = request.Code;
        branchEntity.Address = request.Address;
        branchEntity.PhoneNumber = request.PhoneNumber;
        branchEntity.IsFranchise = request.IsFranchise;
        branchEntity.Status = string.IsNullOrEmpty(request.Status) ? branchEntity.Status : request.Status;
        branchEntity.UpdatedAt = DateTime.Now;
        _unitOfWork.BranchRepository.Update(branchEntity);
        await _unitOfWork.SaveChangesAsync();
        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_UPDATE_MSG);
    }

    public async Task<IBusinessResult> DeleteBranch(Guid id)
    {
        var branch = await _unitOfWork.BranchRepository.GetByIdAsync(id);
        if (branch == null)
        {
            return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Branch not found");
        }

        _unitOfWork.BranchRepository.Delete(branch);
        await _unitOfWork.SaveChangesAsync();

        return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_DELETE_MSG);
    }
}
