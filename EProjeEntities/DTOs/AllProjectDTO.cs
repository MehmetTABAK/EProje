using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EProjeEntities.DTOs
{
    public class AllProjectDTO
    {
        public Guid Id { get; set; }
        public string ProjectName { get; set; }
        public int CompletedCount { get; set; }
        public int InProgressCount { get; set; }
        public int TodoCount { get; set; }
    }
}
