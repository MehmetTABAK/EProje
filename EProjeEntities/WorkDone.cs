using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EProjeEntities
{
	public class WorkDone
    {
		public Guid Id { get; set; }
        public Guid RotaId { get; set; }
        public string WorkDoneDetail { get; set; }
		public bool Active { get; set; }
		public Guid RegistrationUser { get; set; }
		public DateTime RegistrationDate { get; set; }
		public Guid? CorrectionUser { get; set; }
		public DateTime? CorrectionDate { get; set; }

		public virtual Rota? Rota { get; set; }
    }
}
