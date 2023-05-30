namespace MaquinaDeVendas.Models
{
    public class TodosProdutosDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public decimal Price { get; set; } // Propriedade para o preço
        public int Quantity { get; set; } // Propriedade para a quantidade
        public int Sold { get; set; } // Propriedade para o sold
    }
}
