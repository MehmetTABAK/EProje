using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EProjeEntities.DTOs
{
    public class ProjectDetailsDTO
    {
        public Guid Id { get; set; }
        public Guid? WorkerId { get; set; }
        public Guid? ProjectId { get; set; }
        public string WorkerName { get; set; }
        public string ProjectName { get; set; }
        public string WorkName { get; set; }
        public string WorkDetail { get; set; }
        public byte Status { get; set; }
        public DateTime? WorkStartDate { get; set; }
        public DateTime? WorkEndDate { get; set; }
        public string? TimeSpend { get; set; }
        public string RegistrationUserName { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string CorrectionUserName { get; set; }
        public DateTime? CorrectionDate { get; set; }


        public bool IsJobAssigned => WorkerId.HasValue && WorkerId != Guid.Empty;
    }
}
