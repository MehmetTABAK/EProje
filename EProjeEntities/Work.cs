using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EProjeEntities
{
	public class Work
	{
		public Guid Id { get; set; }
		public Guid ProjectId { get; set; }
		public string WorkName { get; set; }
		public string WorkDetail { get; set; }
		public byte Status { get; set; }
		public bool Active { get; set; }
		public Guid RegistrationUser { get; set; }
		public DateTime RegistrationDate { get; set; }
		public Guid? CorrectionUser { get; set; }
		public DateTime? CorrectionDate { get; set; }

		public virtual Project? Project { get; set; }
        public virtual ICollection<Rota>? Rotas { get; set; }
    }
}
