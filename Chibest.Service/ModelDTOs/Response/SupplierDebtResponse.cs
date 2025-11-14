using Chibest.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chibest.Service.ModelDTOs.Response
{
    public class SupplierDebtResponse
    {
        public Guid Id { get; set; }

        public string SupplierName { get; set; } = string.Empty;
        public string? SupplierPhone { get; set; }
        public Guid? supplierId { get; set; }
        public decimal TotalDebt { get; set; }

        public decimal PaidAmount { get; set; }
        public decimal ReturnAmount { get; set; }

        public decimal? RemainingDebt { get; set; }

        public virtual ICollection<SupplierDebtHistoryResponse> SupplierDebtHistories { get; set; } = new List<SupplierDebtHistoryResponse>();
    }
    public class SupplierDebtHistoryResponse
    {
        public Guid Id { get; set; }
        public string TransactionType { get; set; } = null!;

        public DateTime TransactionDate { get; set; }

        public decimal Amount { get; set; }


        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }

    }


}
