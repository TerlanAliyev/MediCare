using System;
using System.Collections.Generic;

namespace MedicalTemplate.Models;

public partial class DaysOfWeek
{
    public int DaysId { get; set; }

    public string? DaysName { get; set; }

    public virtual ICollection<DoctorTimeSchedule> DoctorTimeSchedules { get; set; } = new List<DoctorTimeSchedule>();
}
