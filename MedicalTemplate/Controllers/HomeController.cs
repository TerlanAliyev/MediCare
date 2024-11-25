using MedicalTemplate.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace MedicalTemplate.Controllers
{
    public class HomeController : Controller
    {
        private readonly PatientSystemContext _sql;

        public HomeController(PatientSystemContext sql)
        {
            _sql = sql;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult appointment(int id,int userId)
        {
            // Kullan�c� ID'sini ViewBag ile View'a g�nderiyoruz
            var user = _sql.Users.SingleOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound("Kullan�c� bulunamad�.");
            }

            // Kullan�c� ID'si ve di�er bilgileri ViewBag ile g�nderiyoruz
            ViewBag.UserId = user.UserId; // Kullan�c� ID'si
            ViewBag.Users = _sql.Users.ToList(); // T�m kullan�c�lar� View'a g�nderiyoruz

            // Doktorun randevu zamanlar�n� al�yoruz
            var doctorSchedules = _sql.DoctorTimeSchedules
                .Include(x => x.Dr)
                .Include(x => x.DrDay)
                .Where(x => x.Dr.DrId == id)
                .ToList();

            return View(doctorSchedules);
        }


        public IActionResult AppointmentUser()
        {
            ViewBag.Op = _sql.OperationTypes.ToList();
            ViewBag.Doctors = _sql.DoctorData.ToList();

            return View();
        }
        [HttpPost]
        public IActionResult SaveUser(User user)
        {
            user.UserPostDate = DateTime.Now;
            user.UserComplate = "UnComplate";
            user.UserOption = "No";
            _sql.Users.Add(user);
            _sql.SaveChanges();
            return RedirectToAction("appointment", "Home", new { id = user.UserChoosenDrId,userId=user.UserId });
        }

        public IActionResult team()
        {
            var a = _sql.DoctorData.ToList();
            return View(a);
        }




        public IActionResult ShowAvailableSlots(int doctorId, int dayId, int userId)
        {
            var user = _sql.Users.SingleOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                return NotFound("Kullan�c� bulunamad�.");
            }

            var operationType = _sql.OperationTypes.FirstOrDefault(o => o.OpId == user.UserOperationTypeId);
            if (operationType == null)
            {
                return NotFound("Operasyon t�r� bulunamad�.");
            }

            var operationDuration = operationType.OpDuration ?? 1;

            var doctorSchedule = _sql.DoctorTimeSchedules.FirstOrDefault(x => x.DrId == doctorId && x.DrDayId == dayId);
            if (doctorSchedule == null)
            {
                return NotFound("Doktorun �al��ma saatleri bulunamad�.");
            }

            var takenAppointments = _sql.Appointments
                .Where(a => a.AppointmentDrId == doctorId)
                .AsEnumerable()
                .Where(a => a.AppointmentStartTime.HasValue &&
                            a.AppointmentStartTime.Value.DayOfWeek == (DayOfWeek)dayId)
                .ToList();

            var workStartTime = doctorSchedule.DrStartTime.Value;
            var workEndTime = doctorSchedule.DrEndTime.Value;
            var breakStart = doctorSchedule.BreakStart.HasValue ? doctorSchedule.BreakStart.Value : (DateTime?)null;
            var breakEnd = doctorSchedule.BreakEnd.HasValue ? doctorSchedule.BreakEnd.Value : (DateTime?)null;


            var availableSlots = new List<TimeSlot>();
            AddAvailableSlots(workStartTime, workEndTime, operationDuration, availableSlots, takenAppointments, breakStart, breakEnd);

            if (!availableSlots.Any())
            {
                ViewBag.Message = "Bu g�n i�in bo� zaman dilimi bulunmamaktad�r.";
            }

            ViewData["DoctorId"] = doctorId;
            ViewBag.UserId = userId;
            ViewBag.OperationTypeId = user.UserOperationTypeId;
            ViewBag.OperationTypeName = operationType.OpName;

            return View(availableSlots);
        }





        private void AddAvailableSlots(
     DateTime startTime,
     DateTime endTime,
     double operationDuration,
     List<TimeSlot> availableSlots,
     List<Appointment> takenAppointments,
     DateTime? breakStart = null,
     DateTime? breakEnd = null)
        {
            var durationInMinutes = operationDuration * 60; // S�reyi dakikaya �eviriyoruz
            var timeSlotStart = startTime;

            while (timeSlotStart.AddMinutes(durationInMinutes) <= endTime)
            {
                var timeSlotEnd = timeSlotStart.AddMinutes(durationInMinutes);

                // Ara zamanlar�n� kontrol et
                if (breakStart.HasValue && breakEnd.HasValue &&
                    ((timeSlotStart >= breakStart && timeSlotStart < breakEnd) ||
                     (timeSlotEnd > breakStart && timeSlotEnd <= breakEnd)))
                {
                    timeSlotStart = timeSlotEnd; // Ara zaman�na denk gelen slotlar� atla
                    continue;
                }

                // �ak��ma kontrol�
                if (IsTimeSlotAvailable(timeSlotStart, timeSlotEnd, takenAppointments))
                {
                    availableSlots.Add(new TimeSlot
                    {
                        StartTime = timeSlotStart,
                        EndTime = timeSlotEnd
                    });
                }

                timeSlotStart = timeSlotEnd;
            }
        }


        private bool IsTimeSlotAvailable(DateTime startTime, DateTime endTime, List<Appointment> takenAppointments)
        {
            foreach (var appointment in takenAppointments)
            {
                var takenStart = appointment.AppointmentStartTime.Value;
                var takenEnd = appointment.AppointmentEndTime.Value;

                // �ak��ma kontrol�
                if (startTime < takenEnd && endTime > takenStart)
                {
                    return false; // �ak��ma var
                }
            }
            return true; // �ak��ma yok
        }



        // Burada hasta randevusu kaydedecek
        [HttpPost]
        public IActionResult BookAppointment(int AppointmentDrId, int AppointmentUserId, int OperationTypeId, DateTime AppointmentStartTime, DateTime AppointmentEndTime)
        {
            var appointment = new Appointment
            {
                AppointmentDrId = AppointmentDrId,
                AppointmentUserId = AppointmentUserId,
                OperationTypeId = OperationTypeId,
                AppointmentStartTime = AppointmentStartTime,
                AppointmentEndTime = AppointmentEndTime
            };

            _sql.Appointments.Add(appointment);
            _sql.SaveChanges();

            return RedirectToAction("AppointmentSuccess", "Home" ,new { id = AppointmentDrId, userId = AppointmentUserId });
        }


        public IActionResult AppointmentSuccess(int id,int userId)
        {
            ViewBag.User=_sql.Users.SingleOrDefault(x=>x.UserId == userId); 
            ViewBag.Doctor=_sql.DoctorData.SingleOrDefault(x=>x.DrId==id);
            return View();
        }






































        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
