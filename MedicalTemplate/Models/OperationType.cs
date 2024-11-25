using System;
using System.Collections.Generic;

namespace MedicalTemplate.Models;

public partial class OperationType
{
    public int OpId { get; set; }

    public string? OpName { get; set; }

    public double? OpDuration { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
