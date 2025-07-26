using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using EProjeDataAccess;
using EProjeEntities;
using System.Text.Json;
using EProjeWeb.Controllers;
using EProjeWeb.Attributes;

namespace EProjeWeb.Controllers
{
	[Authorize]
	public class ProfileController : BaseController
	{
		public ProfileController(EProjeDbContext dbContext) : base(dbContext)
		{
		}

        [Route("profilim")]
        public IActionResult MyProfile()
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var officeId = GetCurrentCafeId();
            if (userId == null || officeId == null)
                return Unauthorized();

            var admin = _dbContext.Admin
                .Where(mc => mc.Active && mc.Id == userId)
                .ToList();

            return View(admin);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAdmin([FromBody] Admin admin)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            if (userId == null)
                return Unauthorized();

            var existing = await _dbContext.Admin
                .FirstOrDefaultAsync(mc => mc.Id == admin.Id);

            if (existing == null)
                return Json(new { success = false, message = "Güncellenecek admin bulunamadı!" });

            if (ModelState.IsValid)
            {
                existing.Firstname = admin.Firstname;
                existing.Lastname = admin.Lastname;
                existing.Image = admin.Image;
                existing.Email = admin.Email;
                existing.Password = admin.Password;
                existing.CorrectionUser = userId.Value;
                existing.CorrectionDate = DateTime.Now;

                await _dbContext.SaveChangesAsync();
                return Json(new { success = true, message = "Admin başarıyla güncellendi!" });
            }
            return Json(new { success = false, message = "Admin güncellenemedi!" });
        }

        [AuthorizeWithPermission("ViewWorker")]
        [Route("calisanlar")]
        public IActionResult WorkerProfile()
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var officeId = GetCurrentCafeId();
            if (userId == null || officeId == null)
                return Unauthorized();

            List<Worker> workers = _dbContext.Worker.Include(mc => mc.Office)
                .Where(mc => mc.Active && mc.Office.Id == officeId)
                .ToList();

            return View(workers);
        }

        [AuthorizeWithPermission("AddWorker")]
        [HttpPost]
        public async Task<IActionResult> AddWorker([FromBody] Worker worker)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var officeId = GetCurrentCafeId();
            if (userId == null || officeId == null)
                return Unauthorized();

            var officeIds = _dbContext.Office
                .Where(c => c.Id == officeId)
                .Select(c => c.Id)
                .FirstOrDefault();

            if (officeIds == null)
                return Json(new { success = false, message = "Kafe bulunamadı!" });

            if (ModelState.IsValid)
            {
                worker.OfficeId = officeIds;
                worker.RegistrationUser = userId.Value;
                worker.RegistrationDate = DateTime.Now;

                if (worker.RolePermissions == null)
                    worker.RolePermissions = "[]";

                _dbContext.Worker.Add(worker);
                await _dbContext.SaveChangesAsync();
                return Json(new { success = true, message = "Çalışan başarıyla eklendi!" });
            }

            return Json(new { success = false, message = "Çalışan eklenemedi!" });
        }

        [AuthorizeWithPermission("UpdateWorker")]
        [HttpPost]
        public async Task<IActionResult> UpdateWorker([FromBody] Worker worker)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var officeId = GetCurrentCafeId();
            if (userId == null || officeId == null)
                return Unauthorized();

            var existing = await _dbContext.Worker
                .Include(mc => mc.Office)
                .FirstOrDefaultAsync(mc => mc.Id == worker.Id && mc.Office.Id == officeId);

            if (existing == null)
                return Json(new { success = false, message = "Güncellenecek çalışan bulunamadı!" });

            if (ModelState.IsValid)
            {
                existing.Firstname = worker.Firstname;
                existing.Lastname = worker.Lastname;
                existing.Image = worker.Image;
                existing.Email = worker.Email;
                existing.Password = worker.Password;
                existing.RolePermissions = worker.RolePermissions ?? "[]";
                existing.CorrectionUser = userId.Value;
                existing.CorrectionDate = DateTime.Now;

                await _dbContext.SaveChangesAsync();
                return Json(new { success = true, message = "Çalışan başarıyla güncellendi!" });
            }
            return Json(new { success = false, message = "Çalışan güncellenemedi!" });
        }

        [AuthorizeWithPermission("DeleteWorker")]
        [HttpPost]
        public IActionResult DeleteWorker([FromBody] JsonElement request)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var officeId = GetCurrentCafeId();
            if (userId == null || officeId == null)
                return Unauthorized();

            var workerId = Guid.Parse(request.GetProperty("id").GetString());

            var worker = _dbContext.Worker
                .Include(mc => mc.Office)
                .FirstOrDefault(mc => mc.Id == workerId && mc.Office.Id == officeId);

            if (worker == null)
                return Json(new { success = false, message = "Çalışan bulunamadı veya yetkiniz yok." });

            worker.Active = false;
            worker.CorrectionUser = userId.Value;
            worker.CorrectionDate = DateTime.Now;

            _dbContext.SaveChanges();

            return Json(new { success = true, message = "Çalışan silindi!" });
        }
    }
}
