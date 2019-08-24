﻿using AudioSchedulerOver.Model;
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
