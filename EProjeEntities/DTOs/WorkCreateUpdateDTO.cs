using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EProjeEntities.DTOs
{
    public class WorkCreateUpdateDTO
    {
        public Guid Id { get; set; }
        public string WorkName { get; set; }
        public string WorkDetail { get; set; }
        public Guid ProjectId { get; set; }
        public bool AssignJob { get; set; }
        public Guid? WorkerId { get; set; }
    }
}
