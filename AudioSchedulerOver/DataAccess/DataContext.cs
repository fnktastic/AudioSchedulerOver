using AudioSchedulerOver.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSchedulerOver.DataAccess
{
    public class Context : DbContext
    {
        public Context() : base("AudioSchedulerOverDB")
        {
        }

        public DbSet<Audio> Audios { get; set; }
        public DbSet<Machine> Machines { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Setting> Settings { get; set; }

        public override Task<int> SaveChangesAsync()
        {
            return base.SaveChangesAsync();
        }
    }

    public class DataInitializer : DropCreateDatabaseIfModelChanges<Context>
    {
        public DataInitializer()
        {
            using (var context = new Context())
            {
                InitializeDatabase(context);
            }
        }
    }
}
