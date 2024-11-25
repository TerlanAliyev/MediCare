using System;
using System.Collections.Generic;

namespace MedicalTemplate.Models;

public partial class User
{
    public int UserId { get; set; }

    public int? UserChoosenDrId { get; set; }

    public string? UserFirstName { get; set; }

    public string? UserLastName { get; set; }

    public string? UserPhoneNumber { get; set; }

    public string? UserMessage { get; set; }

    public DateTime? UserPostDate { get; set; }

    public string? UserOption { get; set; }

    public string? UserComplate { get; set; }

    public int? UserOperationTypeId { get; set; }

    public string? UserEmail { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual DoctorDatum? UserChoosenDr { get; set; }

    public virtual OperationType? UserOperationType { get; set; }
}
