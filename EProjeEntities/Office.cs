using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EProjeEntities
{
	public class Office
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string Location { get; set; }
		public bool Active { get; set; }
		public Guid RegistrationUser { get; set; }
		public DateTime RegistrationDate { get; set; }
		public Guid? CorrectionUser { get; set; }
		public DateTime? CorrectionDate { get; set; }

		public virtual ICollection<Admin>? Admins { get; set; }
		public virtual ICollection<Worker>? Workers { get; set; }
		public virtual ICollection<Project>? Projects { get; set; }
    }
}
