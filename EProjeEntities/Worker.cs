using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EProjeEntities
{
	public class Worker
	{
		public Guid Id { get; set; }
		public Guid OfficeId { get; set; }
		public string Firstname { get; set; }
		public string Lastname { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
        public string? Image { get; set; }
        public string? RolePermissions { get; set; }
		public bool Active { get; set; }
		public Guid RegistrationUser { get; set; }
		public DateTime RegistrationDate { get; set; }
		public Guid? CorrectionUser { get; set; }
		public DateTime? CorrectionDate { get; set; }

		public virtual Office? Office { get; set; }
        public virtual ICollection<Rota>? Rotas { get; set; }
    }
}
