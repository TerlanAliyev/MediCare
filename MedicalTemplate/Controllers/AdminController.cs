using MailKit.Security;
using MedicalTemplate.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.Net.Mail;
using System.Security.Claims;
using MailKit.Net.Smtp;


namespace MedicalTemplate.Controllers
{
    public class AdminController : Controller
    {
        private readonly PatientSystemContext _sql;
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminController(PatientSystemContext sql)
        {
            _sql = sql;

        }

        public IActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("SignIn", "Admin");
            }

            int id = Convert.ToInt32(User.FindFirst(ClaimTypes.Sid).Value);
            var a = _sql.DoctorData.SingleOrDefault(x => x.DrId == id);

            ViewBag.Users = _sql.Users.ToList();
            ViewBag.Appointments = _sql.Appointments.Include(x => x.AppointmentDr).Include(x => x.AppointmentUser).ThenInclude(x => x.UserOperationType).Where(x => x.AppointmentDrId == id).ToList();

            var today = DateTime.Today;

            return View(a);
        }



        public IActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        public IActionResult SignUp(DoctorDatum doctorDatum, IFormFile DrImageFile)
        {
            if (DrImageFile == null || DrImageFile.Length == 0)
            {
                ModelState.AddModelError("DrImageFile", "Lütfen bir dosya seçin.");
                return View(doctorDatum);
            }

            string folderPath = Path.Combine("wwwroot", "adminWwwroot", "img", "drprofilephotos");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string newphoto = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(DrImageFile.FileName);
            string fullPath = Path.Combine(folderPath, newphoto);

            using (Stream stream = new FileStream(fullPath, FileMode.Create))
            {
                DrImageFile.CopyTo(stream);
            }

            doctorDatum.DrImage = newphoto;

            doctorDatum.DrContext = "DrContext";
            doctorDatum.DrTitle = "DrTitle";
            doctorDatum.DrSubTitle = "subtitle";

            _sql.DoctorData.Add(doctorDatum);
            _sql.SaveChanges();

            return RedirectToAction("SignIn", "Admin");
        }



        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignIn(DoctorDatum doctorDatum)
        {

            var daxilolan = _sql.DoctorData.SingleOrDefault(x => x.DrName == doctorDatum.DrName && x.DrPassword == doctorDatum.DrPassword);
            if (daxilolan != null)
            {
                List<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name,daxilolan.DrName),
                    new Claim(ClaimTypes.Surname,daxilolan.DrLastName),
                    new Claim(ClaimTypes.Sid,daxilolan.DrId.ToString()),


                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var prins = new ClaimsPrincipal(identity);
                var props = new AuthenticationProperties();
                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, prins, props).Wait();
                return RedirectToAction("Index", "Admin");

            }
            else
            {
                return RedirectToAction("404", "Admin");
            }
        }
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync().Wait();
            return RedirectToAction("SignIn", "Admin");
        }

        public IActionResult AdminProfile(int id)
        {
            id = Convert.ToInt32(User.FindFirst(ClaimTypes.Sid).Value);
            var a = _sql.DoctorData.SingleOrDefault(x => x.DrId == id);
            ViewBag.Users = _sql.Users.Include(x => x.UserChoosenDr).Where(x => x.UserChoosenDrId == id).ToList();

            return View(a);
        }
        [HttpPost]
        public IActionResult AdminProfileEdit(int id, DoctorDatum doctorDatum)
        {
            id = Convert.ToInt32(User.FindFirst(ClaimTypes.Sid).Value);
            var a = _sql.DoctorData.SingleOrDefault(x => x.DrId == id);

            if (a.DrPassword == doctorDatum.DrPassword)
            {

                a.DrName = doctorDatum.DrName;
                a.DrLastName = doctorDatum.DrLastName;
                a.DrPhoneNumber = doctorDatum.DrPhoneNumber;
                a.DrContext = doctorDatum.DrContext;
                a.DrCity = doctorDatum.DrCity;
                a.DrEmail = doctorDatum.DrEmail;
                a.DrAdress = doctorDatum.DrAdress;
                a.DrTitle = doctorDatum.DrTitle;
                a.DrSubTitle = doctorDatum.DrSubTitle;

                _sql.SaveChanges();
                return RedirectToAction("AdminProfile", "Admin");
            }
            else
            {
                return RedirectToAction("404", "Admin");
            }
        }

        [HttpGet]
        public IActionResult UpdateUserOption(int id, string option)
        {
            
                    // Kullanıcıyı veritabanından al
                    var user = _sql.Users.FirstOrDefault(u => u.UserId == id);

                    if (user == null)
                    {
                        return NotFound("Kullanıcı bulunamadı.");
                    }

                    // UserOption değerini güncelle
                    user.UserOption = option;

                    if (option == "Yes")
                    {
                        // Onay e-postası gönder
                        SendConfirmationEmail(user.UserEmail, user.UserFirstName, user.UserLastName);
                    }
                    else if (option == "No")
                    {
                        // Kullanıcıyı sil
                        _sql.Users.Remove(user);
                    }

                    // Değişiklikleri kaydet
                    _sql.SaveChanges();


                    return RedirectToAction("Index", "Admin");
        }



        private void SendConfirmationEmail(string email, string firstName, string lastName)
        {
            try
            {
                // E-posta içeriğini oluştur
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Klinik", "eliyev91terlan99@gmail.com"));
                message.To.Add(new MailboxAddress(firstName, email));
                message.Subject = "Randevu Onayı";

                var builder = new BodyBuilder
                {
                    TextBody = $"Merhaba {firstName} {lastName},\n\nRandevunuz onaylanmıştır. Detaylar için kliniğimizi arayabilirsiniz."
                };

                message.Body = builder.ToMessageBody();

                // SMTP istemcisiyle gönderim
                using var smtp = new MailKit.Net.Smtp.SmtpClient(); // Doğru sınıf adı!
                smtp.Connect("smtp.gmail.com", 465, MailKit.Security.SecureSocketOptions.SslOnConnect);
                smtp.Authenticate("eliyev91terlan99@gmail.com", "terlantotu96");
                smtp.Send(message);
                smtp.Disconnect(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"E-posta gönderilirken bir hata oluştu: {ex.Message}");
            }
        }



        [HttpGet]
        public IActionResult UpdateUserComplate(int id, string option)
        {

            var user = _sql.Users.FirstOrDefault(u => u.UserId == id);
            if (user != null)
            {
                user.UserComplate = option;
                _sql.SaveChanges();
            }

            return RedirectToAction("Index", "Admin");
        }


        public IActionResult Calendar(int id)
        {
            id = Convert.ToInt32(User.FindFirst(ClaimTypes.Sid).Value);
            var a = _sql.DoctorData.SingleOrDefault(x => x.DrId == id);

            ViewBag.Days = _sql.DaysOfWeeks.ToList();
            ViewBag.Users = _sql.Users.ToList();

            ViewBag.DoctorTimeSchedules = _sql.DoctorTimeSchedules.Where(x => x.DrId == id).ToList();

            return View(a);
        }
        [HttpPost]
        public IActionResult SaveSchedule(DoctorTimeSchedule schedule)
        {
            var id = schedule.DrId;
            if (id == null)
            {
                return BadRequest("Doktor ID eksik.");
            }
            ViewBag.Days = _sql.DaysOfWeeks.ToList();

            if (ModelState.IsValid)
            {
                _sql.DoctorTimeSchedules.Add(schedule);
                _sql.SaveChanges();

                TempData["SuccessMessage"] = "Çalışma saatleri başarıyla kaydedildi!";
                return RedirectToAction("Calendar", "Admin");
            }
            return View("Create");
        }


        public IActionResult TodayAppointments(int id)
        {
            id = Convert.ToInt32(User.FindFirst(ClaimTypes.Sid).Value);
            var a = _sql.DoctorData.SingleOrDefault(x=>x.DrId==id);

            ViewBag.Users = _sql.Users.ToList();
            ViewBag.Appointments = _sql.Appointments.Include(x => x.AppointmentDr).Include(x => x.AppointmentUser).ThenInclude(x => x.UserOperationType).Where(x => x.AppointmentDrId == id).ToList();
            return View(a);
        }
        public IActionResult TotalAppointments(int id)
        {
            id = Convert.ToInt32(User.FindFirst(ClaimTypes.Sid).Value);
            var a = _sql.DoctorData.SingleOrDefault(x => x.DrId == id);

            ViewBag.Users = _sql.Users.ToList();
            ViewBag.Appointments = _sql.Appointments.Include(x => x.AppointmentDr).Include(x => x.AppointmentUser).ThenInclude(x => x.UserOperationType).Where(x => x.AppointmentDrId == id).ToList();
            return View(a);

        }
        

       








    }


}

