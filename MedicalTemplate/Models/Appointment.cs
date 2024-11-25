using System;
using System.Collections.Generic;

namespace MedicalTemplate.Models;

public partial class Appointment
{
    public int AppointmentId { get; set; }

    public int? AppointmentUserId { get; set; }

    public int? AppointmentDrId { get; set; }

    public int? OperationTypeId { get; set; }

    public DateTime? AppointmentStartTime { get; set; }

    public DateTime? AppointmentEndTime { get; set; }

    public virtual DoctorDatum? AppointmentDr { get; set; }

    public virtual User? AppointmentUser { get; set; }

    public virtual OperationType? OperationType { get; set; }
}
