using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EProjeEntities
{
	public class Project
    {
		public Guid Id { get; set; }
		public Guid OfficeId { get; set; }
		public string ProjectName { get; set; }
		public bool Active { get; set; }
		public Guid RegistrationUser { get; set; }
		public DateTime RegistrationDate { get; set; }
		public Guid? CorrectionUser { get; set; }
		public DateTime? CorrectionDate { get; set; }

		public virtual Office? Office { get; set; }
        public virtual ICollection<Work>? Works { get; set; }
    }
}
