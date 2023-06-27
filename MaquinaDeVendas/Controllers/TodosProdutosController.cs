using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaquinaDeVendas.Models;
using Newtonsoft.Json;
using System.IO;    
using PetaPoco;
using System.Data;
using MySql.Data.MySqlClient;
using AutoMapper;


namespace MaquinaDeVendas.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TodosProdutosController : ControllerBase
{
    private readonly ProdutosContext _context;

    public TodosProdutosController(ProdutosContext context)
    {
        _context = context;
        
    }


    string connectionString = "Server=localhost;Port=3306;Database=vendingmachine;Uid=root;";

    // GET: api/TodoItems
    [HttpGet("ListadeProdutos")]
    public async Task<ActionResult<IEnumerable<TodosProdutosDTO>>> GetTodoItems()
    {

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<TodosProdutos, TodosProdutosDTO>();
        });

        AutoMapper.IMapper _mapper = config.CreateMapper();


        using (var db = new Database(connectionString, "MySql.Data.MySqlClient")) // Substitua "NomeDaSuaConnectionString" pela sua string de conexão do MySQL
        {
            var todosProdutos = await db.FetchAsync<TodosProdutos>("SELECT * FROM products");

            var responseItems = _mapper.Map<List<TodosProdutosDTO>>(todosProdutos);
            /*var responseItems = todosProdutos.Select(p => new TodosProdutosDTO
            {
                Id = p.Id,
                name = p.name,
                quantity = p.quantity,
                price = p.price,
                sold = p.sold
            }).ToList(); */

            return Ok(responseItems); 
        }
    }

    // GET: api/TodoItems/5
    // <snippet_GetByID>
    [HttpGet("Lista de Produtos por {id}")]
    public async Task<ActionResult<TodosProdutosDTO>> GetTodoProdutos(long id)
    {
        var todosProdutos = await _context.TodoProdutos.FindAsync(id);

        if (todosProdutos == null)
        {
            return NotFound();
        }

        return ProdutosToDTO(todosProdutos);
    }




    // INPUT: É O QUE ENVIA PARA DENTRO, OUTPUT: É O QUE RECEBE PARA FORA.



    [HttpPost("InserirAtualizarProdutos")]

    public async Task<ActionResult> PostTodosProdutos([FromBody] List<TodosProdutosDTO> todosProdutosDTOList)
    {
        using (var db = new Database(connectionString, "MySql.Data.MySqlClient"))
        {
            foreach (var todosProdutosDTO in todosProdutosDTOList)
            {
                var produtoExistente = await db.SingleOrDefaultAsync<TodosProdutos>("SELECT * FROM products WHERE id = @0", todosProdutosDTO.Id);

                if (produtoExistente == null)
                {
                    // O produto não existe no banco de dados, então vamos adicioná-lo
                    var novoProduto = new TodosProdutos
                    {
                        name = todosProdutosDTO.name,
                        quantity = todosProdutosDTO.quantity,
                        price = todosProdutosDTO.price,
                        sold = todosProdutosDTO.sold
                    };

                    await db.InsertAsync("products", "id", true, novoProduto);
                }
                else
                {
                    // O produto já existe no banco de dados, então vamos atualizá-lo
                    produtoExistente.name = todosProdutosDTO.name;
                    produtoExistente.quantity = todosProdutosDTO.quantity;
                    produtoExistente.price = todosProdutosDTO.price;
                    produtoExistente.sold = todosProdutosDTO.sold;

                    await db.UpdateAsync("products", "id", produtoExistente);
                }
            }
        }

        return Ok();
    }






    [HttpPost("Eliminar Produtos")]
    public async Task<ActionResult> DeleteTodosProdutos([FromBody] List<long> idList)
    {
        using (var db = new Database(connectionString, "MySql.Data.MySqlClient")) // Substitua "NomeDaSuaConnectionString" pela sua string de conexão do MySQL
        {
            foreach (var id in idList)
            {
                await db.DeleteAsync("products", "id", null, id);
            }
        }

        return Ok();
    }




    [HttpPost("Checkout")]
    public async Task<ActionResult<List<TodosProdutosDTO>>> Checkout([FromBody] List<TodosProdutosDTO> selectedItems)
    {
        try
        {
            using (var db = new Database(connectionString, "MySql.Data.MySqlClient")) // Substitua "NomeDaSuaConnectionString" pela sua string de conexão do MySQL
            {
                var quantityToRemove = new Dictionary<string, int>();

                foreach (var item in selectedItems)
                {
                    if (quantityToRemove.ContainsKey(item.name))
                    {
                        quantityToRemove[item.name] += 1;
                    }
                    else
                    {
                        quantityToRemove[item.name] = 1;
                    }
                }

                var updatedItems = new List<TodosProdutos>();

                foreach (var itemName in quantityToRemove.Keys)
                {
                    var quantity = quantityToRemove[itemName];

                    var todosProdutos = await db.SingleOrDefaultAsync<TodosProdutos>("SELECT * FROM products WHERE name = @0", itemName);

                    if (todosProdutos != null)
                    {
                        todosProdutos.quantity -= quantity;
                        todosProdutos.sold += quantity;

                        updatedItems.Add(todosProdutos);

                        await db.UpdateAsync("products", "id", todosProdutos);
                    }
                }

                var allProducts = await db.FetchAsync<TodosProdutos>("SELECT * FROM products");

                var responseItems = allProducts.Select(p => new TodosProdutosDTO
                {
                    Id = p.Id,
                    name = p.name,
                    quantity = p.quantity,
                    price = p.price,
                    sold = p.sold
                }).ToList();

                return Ok(responseItems);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Erro ao atualizar os produtos no banco de dados: " + ex.Message);
            return StatusCode(500, "Erro ao atualizar os produtos no banco de dados.");
        }
    }








    private bool TodoProdutosExists(long id)
    {
        return _context.TodoProdutos.Any(e => e.Id == id);
    }

    private static TodosProdutosDTO ProdutosToDTO(TodosProdutos todoProdutos) =>
       new TodosProdutosDTO
       {
           Id = todoProdutos.Id,
           name = todoProdutos.name,
           quantity = todoProdutos.quantity,
           price = todoProdutos.price,
           sold = todoProdutos.sold,
       };
}