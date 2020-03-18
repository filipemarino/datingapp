using DatingApp.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.WebAPI.Context
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options){}

        public DbSet<Value> Values { get; set; }
    }
}