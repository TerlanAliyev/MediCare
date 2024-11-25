using System;
using System.Collections.Generic;

namespace MedicalTemplate.Models;

public partial class DoctorTimeSchedule
{
    public int DrTimeId { get; set; }

    public int? DrId { get; set; }

    public DateTime? DrStartTime { get; set; }

    public DateTime? DrEndTime { get; set; }

    public DateTime? BreakStart { get; set; }

    public DateTime? BreakEnd { get; set; }

    public int? DrDayId { get; set; }

    public virtual DoctorDatum? Dr { get; set; }

    public virtual DaysOfWeek? DrDay { get; set; }
}
