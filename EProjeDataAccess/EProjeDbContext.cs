using Microsoft.EntityFrameworkCore;
using EProjeEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EProjeDataAccess
{
	public class EProjeDbContext : DbContext
	{
		public virtual DbSet<Admin> Admin { get; set; }
		public virtual DbSet<Office> Office { get; set; }
		public virtual DbSet<Project> Project { get; set; }
		public virtual DbSet<Rota> Rota { get; set; }
		public virtual DbSet<Work> Work { get; set; }
		public virtual DbSet<WorkDone> WorkDone { get; set; }
		public virtual DbSet<Worker> Worker { get; set; }

		public EProjeDbContext(DbContextOptions<EProjeDbContext> options) : base(options)
		{
		}
	}
}
