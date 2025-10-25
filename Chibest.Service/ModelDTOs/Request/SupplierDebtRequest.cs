using Chibest.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chibest.Service.ModelDTOs.Request
{
    public class SupplierDebtRequest
    {
        public Guid Id { get; set; }

        public Guid SupplierId { get; set; }

        public decimal TotalDebt { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal? RemainingDebt { get; set; }

        public DateTime? LastTransactionDate { get; set; }

        public DateTime LastUpdated { get; set; }

        public virtual Account Supplier { get; set; } = null!;

        public virtual ICollection<SupplierDebtHistoryRequest> SupplierDebtHistories { get; set; } = new List<SupplierDebtHistoryRequest>();

    }
    public class SupplierDebtHistoryRequest
    {

        public string TransactionType { get; set; } = null!;

        public DateTime TransactionDate { get; set; }

        public decimal Amount { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }

    }
}

