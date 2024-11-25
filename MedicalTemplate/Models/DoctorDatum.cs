using System;
using System.Collections.Generic;

namespace MedicalTemplate.Models;

public partial class DoctorDatum
{
    public int DrId { get; set; }

    public string? DrName { get; set; }

    public string? DrLastName { get; set; }

    public string? DrTitle { get; set; }

    public string? DrPhoneNumber { get; set; }

    public string? DrImage { get; set; }

    public string? DrSubTitle { get; set; }

    public string? DrContext { get; set; }

    public string? DrEmail { get; set; }

    public string? DrPassword { get; set; }

    public string? DrCity { get; set; }

    public string? DrAdress { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<DoctorTimeSchedule> DoctorTimeSchedules { get; set; } = new List<DoctorTimeSchedule>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
