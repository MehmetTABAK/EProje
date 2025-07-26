using EProjeDataAccess;
using EProjeEntities;
using EProjeEntities.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EProjeWeb.Controllers
{
    public class ApplicationDevelopmentTrackingController : BaseController
    {
        public ApplicationDevelopmentTrackingController(EProjeDbContext dbContext) : base(dbContext)
        {
        }

        [HttpGet]
        public IActionResult AllProjects()
        {
            var userId = GetCurrentUserId();
            var officeId = GetCurrentCafeId();
            if (userId == null || officeId == null)
                return Unauthorized();

            var allProjects = _dbContext.Project
                .Where(w => w.OfficeId == officeId && w.Active)
                .Include(p => p.Works)
                .Select(s => new AllProjectDTO
                {
                    Id = s.Id,
                    ProjectName = s.ProjectName,
                    CompletedCount = s.Works.Count(c1 => c1.Status == 3 && c1.Active), //Tamamlandı
                    InProgressCount = s.Works.Count(c2 => c2.Status == 2 && c2.Active), //Yapılıyor
                    TodoCount = s.Works.Count(c3 => c3.Status == 1 && c3.Active), //Yapılacak
                })
                .ToList();

            return View(allProjects);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] Project project)
        {
            var userId = GetCurrentUserId();
            var officeId = GetCurrentCafeId();
            if (userId == null || officeId == null)
                return Unauthorized();

            // Ofis bilgisi kontrolü
            var office = _dbContext.Office.FirstOrDefault(o => o.Id == officeId && o.Active);
            if (office == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    project.Id = Guid.NewGuid();
                    project.OfficeId = officeId.Value;
                    project.Active = true;
                    project.RegistrationUser = userId.Value;
                    project.RegistrationDate = DateTime.Now;

                    _dbContext.Project.Add(project);
                    await _dbContext.SaveChangesAsync();
                    return Json(new { success = true, message = "Proje başarıyla oluşturuldu." });
                }
                catch (DbUpdateException ex) when (IsDuplicateKeyException(ex))
                {
                    // Guid çakışması durumunda tekrar dene
                    return await CreateProject(project); // Recursive çağrı (dikkatli kullanın)
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"Proje oluşturulamadı: {ex.Message}" });
                }
            }
            return Json(new { success = false, message = "Proje oluşturulamadı!" });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProject([FromBody] Project project)
        {
            var userId = GetCurrentUserId();
            var officeId = GetCurrentCafeId();
            if (userId == null || officeId == null)
                return Unauthorized();

            // Proje var mı ve bu ofise mi ait kontrolü
            var existingProject = _dbContext.Project
                .FirstOrDefault(p => p.Id == project.Id && p.OfficeId == officeId && p.Active);
            if (existingProject == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    existingProject.ProjectName = project.ProjectName;
                    existingProject.CorrectionUser = userId.Value;
                    existingProject.CorrectionDate = DateTime.Now;

                    _dbContext.Project.Update(existingProject);
                    await _dbContext.SaveChangesAsync();
                    return Json(new { success = true, message = "Proje başarıyla güncellendi." });
                }
                catch (DbUpdateException ex) when (IsDuplicateKeyException(ex))
                {
                    // Guid çakışması durumunda tekrar dene
                    return await UpdateProject(project); // Recursive çağrı (dikkatli kullanın)
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"Proje güncellenemedi: {ex.Message}" });
                }
            }
            return Json(new { success = false, message = "Proje güncellenemedi!" });
        }

        [HttpPost]
        public IActionResult DeleteProject([FromBody] JsonElement request)
        {
            var userId = GetCurrentUserId();
            var officeId = GetCurrentCafeId();
            if (userId == null || officeId == null)
                return Unauthorized();

            var projectId = Guid.Parse(request.GetProperty("id").GetString());

            // Proje var mı ve bu ofise mi ait kontrolü
            var project = _dbContext.Project
                .FirstOrDefault(p => p.Id == projectId && p.OfficeId == officeId && p.Active);
            if (project == null)
                return NotFound();

            try
            {
                project.Active = false;
                project.CorrectionUser = userId.Value;
                project.CorrectionDate = DateTime.Now;

                _dbContext.Project.Update(project);

                // Projeye bağlı tüm aktif işleri getir
                var works = _dbContext.Work
                    .Where(w => w.ProjectId == projectId && w.Active)
                    .Include(w => w.Rotas) // Rotaları da include et
                    .ToList();

                // Tüm işler ve rotaları için silme işlemi
                foreach (var work in works)
                {
                    // İşe bağlı rotaları pasif hale getir
                    foreach (var rota in work.Rotas.Where(r => r.Active))
                    {
                        rota.Active = false;
                        rota.CorrectionUser = userId.Value;
                        rota.CorrectionDate = DateTime.Now;

                        _dbContext.Rota.Update(rota);
                    }

                    // İşi pasif hale getir
                    work.Active = false;
                    work.CorrectionUser = userId.Value;
                    work.CorrectionDate = DateTime.Now;

                    _dbContext.Work.Update(work);
                }

                _dbContext.SaveChangesAsync();

                return Json(new { success = true, message = "Proje başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Proje silinemedi: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult ProjectDetails(Guid projectId)
        {
            var userId = GetCurrentUserId();
            var officeId = GetCurrentCafeId();
            if (userId == null || officeId == null)
                return Unauthorized();

            // Proje var mı ve bu ofise mi ait kontrolü
            var project = _dbContext.Project
                .FirstOrDefault(p => p.Id == projectId && p.OfficeId == officeId && p.Active);

            if (project == null)
                return NotFound();

            ViewBag.ProjectId = project.Id;
            ViewBag.ProjectName = project.ProjectName;

            // Worker listesini ViewBag'e ekleyin
            ViewBag.Workers = _dbContext.Worker
                .Where(w => w.OfficeId == officeId && w.Active)
                .Select(w => new
                {
                    Id = w.Id,
                    Name = $"{w.Firstname} {w.Lastname}"
                })
                .ToList();

            // Worker isimleri için önceden sorgu
            var workerNames = _dbContext.Worker
                .Where(w => w.OfficeId == officeId)
                .ToDictionary(w => w.Id, w => $"{w.Firstname} {w.Lastname}");

            // User isimleri için önceden sorgu (Admin + Worker)
            var allUserIds = new List<Guid>();
            var workDetails = _dbContext.Work
                .Where(w => w.ProjectId == projectId && w.Active)
                .Include(i => i.Rotas)
                .ToList();

            allUserIds.AddRange(workDetails.Select(w => w.RegistrationUser));
            allUserIds.AddRange(workDetails
                .Where(w => w.CorrectionUser.HasValue)
                .Select(w => w.CorrectionUser.Value)
                .Distinct());

            var userNames = _dbContext.Admin
                .Where(a => allUserIds.Contains(a.Id))
                .ToDictionary(a => a.Id, a => $"{a.Firstname} {a.Lastname}");

            var workerUserNames = _dbContext.Worker
                .Where(w => allUserIds.Contains(w.Id))
                .ToDictionary(w => w.Id, w => $"{w.Firstname} {w.Lastname}");

            // Tüm kullanıcı isimlerini birleştir
            var allUserNames = userNames
                .Concat(workerUserNames)
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            // Ana sorgu
            var projectDetails = workDetails
                .Select(s =>
                {
                    var rota = s.Rotas?.FirstOrDefault(r => r.Active);

                    return new ProjectDetailsDTO
                    {
                        Id = s.Id,
                        WorkName = s.WorkName,
                        WorkDetail = s.WorkDetail,
                        Status = s.Status,
                        RegistrationUserName = allUserNames.ContainsKey(s.RegistrationUser)
                            ? allUserNames[s.RegistrationUser]
                            : "Bilinmiyor",
                        RegistrationDate = s.RegistrationDate,
                        CorrectionUserName = s.CorrectionUser.HasValue && allUserNames.ContainsKey(s.CorrectionUser.Value)
                            ? allUserNames[s.CorrectionUser.Value]
                            : null,
                        CorrectionDate = s.CorrectionDate,
                        WorkStartDate = rota?.WorkStartDate,
                        WorkEndDate = rota?.WorkEndDate,
                        TimeSpend = rota?.TimeSpend,
                        WorkerId = rota?.WorkerId,
                        WorkerName = rota?.WorkerId != null && workerNames.ContainsKey(rota.WorkerId.Value)
                            ? workerNames[rota.WorkerId.Value]
                            : null,
                        ProjectId = s.ProjectId,
                        ProjectName = project.ProjectName,
                    };
                })
                .ToList();

            return View(projectDetails);
        }

        [HttpPost]
        public async Task<IActionResult> CreateWork([FromBody] WorkCreateUpdateDTO workDto)
        {
            var userId = GetCurrentUserId();
            var officeId = GetCurrentCafeId();
            if (userId == null || officeId == null)
                return Unauthorized();

            // Ofis bilgisi kontrolü
            var office = _dbContext.Office.FirstOrDefault(o => o.Id == officeId && o.Active);
            if (office == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var work = new Work
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = workDto.ProjectId,
                        WorkName = workDto.WorkName,
                        WorkDetail = workDto.WorkDetail,
                        Status = 1,
                        Active = true,
                        RegistrationUser = userId.Value,
                        RegistrationDate = DateTime.Now
                    };

                    _dbContext.Work.Add(work);

                    // Eğer atanmış kişi varsa Rota tablosuna ekleme
                    if (workDto.AssignJob && workDto.WorkerId.HasValue && workDto.WorkerId != Guid.Empty)
                    {
                        var rota = new Rota
                        {
                            Id = Guid.NewGuid(),
                            WorkId = work.Id,
                            WorkerId = workDto.WorkerId.Value,
                            WorkStartDate = null,
                            WorkEndDate = null,
                            TimeSpend = null,
                            Active = true,
                            RegistrationUser = userId.Value,
                            RegistrationDate = DateTime.Now
                        };

                        _dbContext.Rota.Add(rota);
                    }

                    await _dbContext.SaveChangesAsync();

                    return Json(new { success = true, message = "İş başarıyla oluşturuldu." });
                }
                catch (DbUpdateException ex) when (IsDuplicateKeyException(ex))
                {
                    // Guid çakışması durumunda tekrar dene
                    return await CreateWork(workDto); // Recursive çağrı (dikkatli kullanın)
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"İş oluşturulamadı: {ex.Message}" });
                }
            }
            return Json(new { success = false, message = "İş oluşturulamadı!" });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateWork([FromBody] WorkCreateUpdateDTO workDto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            // İş var mı ve bu ofise mi ait kontrolü
            var existingWork = _dbContext.Work
                .FirstOrDefault(p => p.Id == workDto.Id && p.ProjectId == workDto.ProjectId && p.Active);

            if (existingWork == null)
                return NotFound();

            var existingRota = await _dbContext.Rota
                .FirstOrDefaultAsync(r => r.WorkId == workDto.Id && r.Active);

            if (ModelState.IsValid)
            {
                try
                {
                    existingWork.WorkName = workDto.WorkName;
                    existingWork.WorkDetail = workDto.WorkDetail;
                    existingWork.CorrectionUser = userId.Value;
                    existingWork.CorrectionDate = DateTime.Now;

                    _dbContext.Work.Update(existingWork);

                    if (workDto.AssignJob && workDto.WorkerId.HasValue && workDto.WorkerId != Guid.Empty)
                    {
                        if (existingRota == null)
                        {
                            // Yeni rota oluştur
                            var newRota = new Rota
                            {
                                Id = Guid.NewGuid(),
                                WorkId = workDto.Id,
                                WorkerId = workDto.WorkerId.Value,
                                WorkStartDate = null,
                                WorkEndDate = null,
                                TimeSpend = null,
                                Active = true,
                                RegistrationUser = userId.Value,
                                RegistrationDate = DateTime.Now
                            };

                            _dbContext.Rota.Add(newRota);
                        }
                        else
                        {
                            // Var olan rotayı güncelle
                            existingRota.WorkerId = workDto.WorkerId.Value;
                            existingRota.CorrectionUser = userId.Value;
                            existingRota.CorrectionDate = DateTime.Now;

                            _dbContext.Rota.Update(existingRota);
                        }
                    }
                    else if (existingRota != null)
                    {
                        // Atama kaldırıldıysa rotayı sil
                        existingRota.WorkerId = null;
                        existingRota.CorrectionUser = userId.Value;
                        existingRota.CorrectionDate = DateTime.Now;

                        _dbContext.Rota.Update(existingRota);
                    }

                    await _dbContext.SaveChangesAsync();

                    return Json(new { success = true, message = "İş başarıyla güncellendi." });
                }
                catch (DbUpdateException ex) when (IsDuplicateKeyException(ex))
                {
                    // Guid çakışması durumunda tekrar dene
                    return await UpdateWork(workDto); // Recursive çağrı
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"İş güncellenemedi: {ex.Message}" });
                }
            }
            return Json(new { success = false, message = "İş güncellenemedi!" });
        }

        [HttpPost]
        public IActionResult DeleteWork([FromBody] JsonElement request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var workId = Guid.Parse(request.GetProperty("id").GetString());

            // Proje var mı ve bu ofise mi ait kontrolü
            var work = _dbContext.Work
                .FirstOrDefault(p => p.Id == workId && p.Active);
            if (work == null)
                return NotFound();

            // İlgili rotaları bul (aktif olanları)
            var rota = _dbContext.Rota
                .FirstOrDefault(r => r.WorkId == workId && r.Active);

            try
            {
                rota.Active = false;
                rota.CorrectionUser = userId.Value;
                rota.CorrectionDate = DateTime.Now;

                _dbContext.Rota.Update(rota);

                work.Active = false;
                work.CorrectionUser = userId.Value;
                work.CorrectionDate = DateTime.Now;

                _dbContext.Work.Update(work);

                _dbContext.SaveChangesAsync();
                return Json(new { success = true, message = "İş başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"İş silinemedi: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult MyProjects()
        {
            var userId = GetCurrentUserId();
            var officeId = GetCurrentCafeId();
            if (userId == null || officeId == null)
                return Unauthorized();

            var currentWorker = _dbContext.Worker
                .FirstOrDefault(w => w.Id == userId && w.OfficeId == officeId);

            if (currentWorker == null)
                return Unauthorized();

            ViewBag.FullName = $"{currentWorker.Firstname} {currentWorker.Lastname}";
            ViewBag.AvatarInitials = $"{currentWorker.Firstname[0]}{currentWorker.Lastname[0]}";

            var rotaList = _dbContext.Rota
                .Where(r => r.Work.Project.OfficeId == officeId &&
                            r.WorkerId == userId &&
                            r.Work.Status != 3 &&
                            r.Active &&
                            r.Work.Active)
                .Include(r => r.Work)
                    .ThenInclude(w => w.Project)
                .ToList();

            var registrationUserIds = rotaList.Select(r => r.Work.RegistrationUser).Distinct().ToList();
            var correctionUserIds = rotaList
                .Where(r => r.Work.CorrectionUser.HasValue)
                .Select(r => r.Work.CorrectionUser.Value)
                .Distinct()
                .ToList();

            var allUserIds = registrationUserIds.Union(correctionUserIds).Distinct().ToList();

            var adminUsers = _dbContext.Admin
                .Where(a => allUserIds.Contains(a.Id))
                .ToDictionary(a => a.Id, a => $"{a.Firstname} {a.Lastname}");

            var workerUsers = _dbContext.Worker
                .Where(w => allUserIds.Contains(w.Id))
                .ToDictionary(w => w.Id, w => $"{w.Firstname} {w.Lastname}");

            var allUsers = adminUsers
                .Concat(workerUsers)
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            var myProjects = rotaList.Select(r => new MyProjectsDTO
            {
                Id = r.Work.Id,
                RotaId = r.Id,
                ProjectName = r.Work.Project.ProjectName,
                WorkName = r.Work.WorkName,
                WorkDetail = r.Work.WorkDetail,
                WorkStartDate = r.WorkStartDate,
                TimeSpend = r.TimeSpend,
                Status = r.Work.Status,
                RegistrationUserName = allUsers.ContainsKey(r.Work.RegistrationUser)
                    ? allUsers[r.Work.RegistrationUser]
                    : "Bilinmiyor",
                RegistrationDate = r.Work.RegistrationDate,
                CorrectionUserName = r.Work.CorrectionUser.HasValue && allUsers.ContainsKey(r.Work.CorrectionUser.Value)
                    ? allUsers[r.Work.CorrectionUser.Value]
                    : null,
                CorrectionDate = r.Work.CorrectionDate
            }).ToList();

            return View(myProjects);
        }
    }
}
