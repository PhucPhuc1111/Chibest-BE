using Chibest.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chibest.Service.ModelDTOs.Response
{
    public class BranchDebtResponse
    {
        public Guid Id { get; set; }

        public string BranchName { get; set; } = string.Empty;
        public string? Email { get; set; }

        public decimal TotalDebt { get; set; }

        public decimal PaidAmount { get; set; }
        public decimal ReturnAmount { get; set; }

        public decimal? RemainingDebt { get; set; }

        public DateTime? LastTransactionDate { get; set; }

        public DateTime LastUpdated { get; set; }

        public virtual ICollection<BranchDebtHistoryResponse> BranchDebtHistories { get; set; } = new List<BranchDebtHistoryResponse>();
    }

    public class BranchDebtHistoryResponse
    {
        public Guid Id { get; set; }
        public string TransactionType { get; set; } = null!;

        public DateTime TransactionDate { get; set; }

        public decimal Amount { get; set; }

        public decimal BalanceBefore { get; set; }

        public decimal BalanceAfter { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }

    }
}
