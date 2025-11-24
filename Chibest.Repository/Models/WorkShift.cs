using System;
using System.Collections.Generic;

namespace Chibest.Repository.Models;

public class WorkShift
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public bool IsOvernight { get; set; }

    public decimal ShiftCoefficient { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}
