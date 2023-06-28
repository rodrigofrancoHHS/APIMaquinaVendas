namespace MaquinaDeVendas.Models
{
    public class TodosProdutos
    {
        public long Id { get; set; }
        public string name { get; set; }

        public decimal price { get; set; } // Propriedade para o preço

        public int quantity { get; set; } // Propriedade para a quantidade
        public int sold { get; set; } // Propriedade para a quantidade

    }
}
