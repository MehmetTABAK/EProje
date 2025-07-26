using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using EProjeDataAccess;
using System.Security.Claims;
using EProjeEntities;

namespace EProjeWeb.Controllers
{
	public class BaseController : Controller
	{
		protected readonly EProjeDbContext _dbContext;

		public BaseController(EProjeDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		protected Guid? GetCurrentUserId()
        {
            var idStr = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            return Guid.TryParse(idStr, out var id) ? id : null;
        }

		protected string? GetCurrentUserRole()
		{
			return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
		}

		protected Guid? GetCurrentCafeId()
		{
			var userId = GetCurrentUserId();
			var role = GetCurrentUserRole();

			if (userId != null && role != null)
            {
                if (role == "Admin")
                {
                    var cafe = _dbContext.Admin.FirstOrDefault(c => c.Id == userId);
                    return cafe?.OfficeId;
                }
                else if (role == "Worker")
                {
                    var worker = _dbContext.Worker.FirstOrDefault(w => w.Id == userId);
                    return worker?.OfficeId;
                }
            }

            // Eğer login yapılmamışsa (anonim müşteri ise) Session'dan al
            var sessionCafeIdString = HttpContext.Session.GetString("CafeId");

            if (Guid.TryParse(sessionCafeIdString, out Guid cafeGuid))
            {
                return cafeGuid;
            }
            return null;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString();
            var role = GetCurrentUserRole();

            if (role != "Admin")
            {
                var cafeId = GetCurrentCafeId();

                if (cafeId == null)
                {
                    base.OnActionExecuting(context);
                    return;
                }
            }

            base.OnActionExecuting(context);
        }

        public bool IsDuplicateKeyException(DbUpdateException ex)
        {
            return ex.InnerException is SqlException sqlEx &&
                   (sqlEx.Number == 2627 || sqlEx.Number == 2601); // PK veya unique key violation
        }
    }
}
