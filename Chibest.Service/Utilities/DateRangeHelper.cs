using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chibest.Service.Utilities
{
    public static class DateRangeHelper
    {
        public static (DateTime? StartDate, DateTime? EndDate) GetDateRange(
            string? datePreset,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            DateTime now = DateTime.Now;
            DateTime? startDate = fromDate;
            DateTime? endDate = toDate;

            switch (datePreset)
            {
                case "Hôm nay":
                    startDate = now.Date;
                    endDate = now.Date.AddDays(1);
                    break;

                case "Hôm qua":
                    startDate = now.Date.AddDays(-1);
                    endDate = now.Date;
                    break;

                case "Tháng này":
                    startDate = new DateTime(now.Year, now.Month, 1);
                    endDate = startDate.Value.AddMonths(1);
                    break;

                case "Tháng trước":
                    startDate = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
                    endDate = startDate.Value.AddMonths(1);
                    break;

                case "Năm này":
                    startDate = new DateTime(now.Year, 1, 1);
                    endDate = new DateTime(now.Year + 1, 1, 1);
                    break;

                case "Năm trước":
                    startDate = new DateTime(now.Year - 1, 1, 1);
                    endDate = new DateTime(now.Year, 1, 1);
                    break;

                case "Toàn thời gian":
                default:
                    // Nếu người dùng đã gửi fromDate/toDate thì giữ nguyên
                    // Nếu không có thì trả về null (không lọc)
                    break;
            }

            return (startDate, endDate);
        }
    }
}

