using Microsoft.EntityFrameworkCore;

namespace MaquinaDeVendas.Models
{
    public class ProdutosContext : DbContext
    {
        public ProdutosContext(DbContextOptions<ProdutosContext> options) : base(options)
        { 
        }
        public DbSet<TodosProdutos> TodoProdutos { get; set; } = null!;
    }
}
