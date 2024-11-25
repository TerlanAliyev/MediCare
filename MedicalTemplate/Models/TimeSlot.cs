using System;
using System.Collections.Generic;
using System.Numerics;

namespace MedicalTemplate.Models;

public class TimeSlot
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int DoctorId { get; set; }  // DoctorId ile ilişkilendirme
    public virtual DoctorDatum Doctors { get; set; }  // Doktor nesnesi
}


