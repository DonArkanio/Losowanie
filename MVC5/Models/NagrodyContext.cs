using System.Data.Entity;

namespace MVC5.Models
{
    public class NagrodyContext : DbContext
    {   
        public NagrodyContext() : base("name=NagrodyContext")
        {
        }

        public DbSet<NagrodyModel> NagrodyModels { get; set; }
    
    }
}
