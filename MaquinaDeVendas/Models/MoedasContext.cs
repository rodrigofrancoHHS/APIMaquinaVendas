using Microsoft.EntityFrameworkCore;

namespace MaquinaDeVendas.Models
{
    public class MoedasContext : DbContext
    {
        public MoedasContext(DbContextOptions<MoedasContext> options) : base(options)
        {
        }

        public DbSet<Moeda> Moedas { get; set; } = null!;
    }
}
