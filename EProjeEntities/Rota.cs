using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EProjeEntities
{
	public class Rota
	{
		public Guid Id { get; set; }
		public Guid WorkId { get; set; }
		public Guid? WorkerId { get; set; }
		public DateTime? WorkStartDate { get; set; }
		public DateTime? WorkEndDate { get; set; }
		public string? TimeSpend { get; set; }
		public bool Active { get; set; }
		public Guid RegistrationUser { get; set; }
		public DateTime RegistrationDate { get; set; }
		public Guid? CorrectionUser { get; set; }
		public DateTime? CorrectionDate { get; set; }

		public virtual Work? Work { get; set; }
		public virtual Worker? Worker { get; set; }
        public virtual ICollection<WorkDone>? WorkDones { get; set; }
    }
}
