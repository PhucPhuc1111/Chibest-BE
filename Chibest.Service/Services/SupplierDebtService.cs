using Chibest.Common;
using Chibest.Common.BusinessResult;
using Chibest.Repository;
using Chibest.Repository.Models;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.Request;
using Chibest.Service.ModelDTOs.Response;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Triangulate.Tri;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chibest.Service.Services
{
    public class SupplierDebtService : ISupplierDebtService
    {
        private readonly IUnitOfWork _unitOfWork;
        public SupplierDebtService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IBusinessResult> AddSupplierTransactionAsync(
    Guid supplierId,
    List<SupplierDebtHistoryRequest> transactions)
        {
            if (transactions == null || !transactions.Any())
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "No transaction data provided");

            await _unitOfWork.BeginTransaction();

            try
            {
                // 1️⃣ Tìm công nợ của NCC
                var supplierDebt = await _unitOfWork.SupplierDebtRepository
                    .GetByWhere(x => x.SupplierId == supplierId)
                    .FirstOrDefaultAsync();

                if (supplierDebt == null)
                {
                    supplierDebt = new SupplierDebt
                    {
                        Id = Guid.NewGuid(),
                        SupplierId = supplierId,
                        TotalDebt = 0,
                        PaidAmount = 0,
                        LastTransactionDate = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow
                    };

                    await _unitOfWork.SupplierDebtRepository.AddAsync(supplierDebt);
                    await _unitOfWork.SaveChangesAsync();
                }

                // 2️⃣ Chuẩn bị danh sách lịch sử giao dịch
                decimal currentBalance = supplierDebt.RemainingDebt ?? 0;
                var historyEntities = new List<SupplierDebtHistory>();

                foreach (var t in transactions)
                {
                    decimal balanceBefore = currentBalance;
                    decimal balanceAfter = balanceBefore;

                    switch (t.TransactionType)
                    {
                        case "Purchase":
                            supplierDebt.TotalDebt += t.Amount;
                            balanceAfter = balanceBefore + t.Amount;
                            break;

                        case "Payment":
                            supplierDebt.PaidAmount += t.Amount;
                            balanceAfter = balanceBefore - t.Amount;
                            break;

                        case "Return":
                            balanceAfter = balanceBefore - t.Amount;
                            break;
                        case "Custom":
                            balanceAfter = t.Amount;
                            supplierDebt.RemainingDebt = t.Amount;

                            supplierDebt.TotalDebt = Math.Max(t.Amount, 0);
                            supplierDebt.PaidAmount = supplierDebt.TotalDebt - t.Amount;
                            break;

                        default:
                            throw new Exception($"Invalid TransactionType: {t.TransactionType}");
                    }

                    supplierDebt.RemainingDebt = balanceAfter;
                    currentBalance = balanceAfter;

                    historyEntities.Add(new SupplierDebtHistory
                    {
                        Id = Guid.NewGuid(),
                        SupplierDebtId = supplierDebt.Id,
                        TransactionType = t.TransactionType,
                        TransactionDate = t.TransactionDate,
                        Amount = t.Amount,
                        BalanceBefore = balanceBefore,
                        BalanceAfter = balanceAfter,
                        Note = t.Note,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                // 3️⃣ Cập nhật SupplierDebt tổng
                supplierDebt.LastTransactionDate = DateTime.UtcNow;
                supplierDebt.LastUpdated = DateTime.UtcNow;
                _unitOfWork.SupplierDebtRepository.Update(supplierDebt);
                await _unitOfWork.SaveChangesAsync();

                // 4️⃣ Bulk insert tất cả lịch sử
                await _unitOfWork.BulkInsertAsync(historyEntities);

                // 5️⃣ Commit transaction
                await _unitOfWork.CommitTransaction();

                return new BusinessResult(Const.HTTP_STATUS_OK, "Supplier transactions created successfully", new
                {
                    SupplierId = supplierId,
                    supplierDebt.TotalDebt,
                    supplierDebt.PaidAmount,
                    supplierDebt.RemainingDebt,
                });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransaction();
                return new BusinessResult(Const.ERROR_EXCEPTION, "Error creating supplier transactions", ex.Message);
            }
        }


        public async Task<IBusinessResult> GetSupplierDebtAsync(Guid id)
        {
            try
            {
                var supplierDebt = await _unitOfWork.SupplierDebtRepository
                    .GetByWhere(x => x.Id == id)
                    .Include(x => x.Supplier)
                    .Include(x => x.SupplierDebtHistories)
                    .FirstOrDefaultAsync();

                if (supplierDebt == null)
                {
                    return new BusinessResult(Const.HTTP_STATUS_OK, "No debt record found");
                }

                var response = new SupplierDebtResponse
                {
                    Id = supplierDebt.Id,
                    SupplierName = supplierDebt.Supplier?.Name ?? "Unknown",
                    TotalDebt = supplierDebt.TotalDebt,
                    PaidAmount = supplierDebt.PaidAmount,
                    RemainingDebt = supplierDebt.RemainingDebt,
                    LastTransactionDate = supplierDebt.LastTransactionDate,
                    LastUpdated = supplierDebt.LastUpdated,
                    SupplierDebtHistories = supplierDebt.SupplierDebtHistories
                        .OrderByDescending(h => h.TransactionDate)
                        .Select(h => new SupplierDebtHistoryResponse
                        {
                            Id = h.Id,
                            TransactionType = h.TransactionType,
                            TransactionDate = h.TransactionDate,
                            Amount = h.Amount,
                            BalanceBefore = h.BalanceBefore,
                            BalanceAfter = h.BalanceAfter,
                            Note = h.Note,
                            CreatedAt = h.CreatedAt
                        })
                        .ToList()
                };

                return new BusinessResult(Const.HTTP_STATUS_OK, "Supplier debt retrieved successfully", response);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "Error retrieving supplier debt", ex.Message);
            }
        }

        public async Task<IBusinessResult> GetSupplierDebtList(int pageIndex, int pageSize, string search)
        {
            string searchTerm = search?.ToLower() ?? string.Empty;

            var supplierDebts = await _unitOfWork.SupplierDebtRepository.GetPagedAsync(
                pageIndex,
                pageSize,
                x => string.IsNullOrEmpty(searchTerm)
                    || x.Supplier.Name.ToLower().Contains(searchTerm)
                    || x.Supplier.PhoneNumber.ToLower().Contains(searchTerm)
                    || x.Supplier.Email.ToLower().Contains(searchTerm),
                include: q => q.Include(x => x.Supplier)
            );

            if (supplierDebts == null || !supplierDebts.Any())
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, Const.FAIL_READ_MSG);
            }

            var responseList = supplierDebts.Select(x => new SupplierDebtResponse
            {
                Id = x.Id,
                SupplierName = x.Supplier.Name,
                SupplierPhone = x.Supplier.PhoneNumber,
                TotalDebt = x.TotalDebt,
                PaidAmount = x.PaidAmount,
                RemainingDebt = x.RemainingDebt,
                LastTransactionDate = x.LastTransactionDate,
            }).ToList();

            return new BusinessResult(Const.HTTP_STATUS_OK, Const.SUCCESS_READ_MSG, responseList);
        }


    }

}
