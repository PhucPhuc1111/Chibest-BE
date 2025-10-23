using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public partial class Attendance
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }

    public Guid BranchId { get; set; }

    public Guid? WorkShiftId { get; set; }

    public DateOnly WorkDate { get; set; }

    public DateTime? CheckInTime { get; set; }

    public DateTime? CheckOutTime { get; set; }

    public decimal WorkHours { get; set; }

    public decimal OvertimeHours { get; set; }

    public string DayType { get; set; } = null!;

    public string AttendanceStatus { get; set; } = null!;

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual Account Employee { get; set; } = null!;

    public virtual WorkShift? WorkShift { get; set; }
}
