using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaquinaDeVendas.Models;
using Newtonsoft.Json;
using System.IO;
using PetaPoco;
using System.Data;
using MySql.Data.MySqlClient;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaquinaDeVendas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodosProdutosController : ControllerBase
    {
        private readonly ProdutosContext _context;
        private readonly AutoMapper.IMapper _mapper;

        public TodosProdutosController(ProdutosContext context, AutoMapper.IMapper mapper)
        {
            _context = context;
            _mapper = mapper;


            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TodosProdutos, TodosProdutosDTO>();
            });

            _mapper = config.CreateMapper();
        }

        string connectionString = "Server=localhost;Port=3306;Database=vendingmachine;Uid=root;";

        [HttpGet("ListadeProdutos")]
        public async Task<ActionResult<IEnumerable<TodosProdutosDTO>>> GetTodoItems()
        {
            using (var db = new Database(connectionString, "MySql.Data.MySqlClient"))
            {
                var todosProdutos = await db.FetchAsync<TodosProdutos>("SELECT * FROM products");

                var responseItems = todosProdutos.Select(p => new TodosProdutosDTO
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

        [HttpGet("ListaDeProdutosPorId/{id}")]
        public async Task<ActionResult<TodosProdutosDTO>> GetTodoProdutos(long id)
        {
            var todosProdutos = await _context.TodoProdutos.FindAsync(id);

            if (todosProdutos == null)
            {
                return NotFound();
            }

            return ProdutosToDTO(todosProdutos);
        }

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

        [HttpPost("EliminarProdutos")]
        public async Task<ActionResult> DeleteTodosProdutos([FromBody] List<long> idList)
        {
            using (var db = new Database(connectionString, "MySql.Data.MySqlClient"))
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
                using (var db = new Database(connectionString, "MySql.Data.MySqlClient"))
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
}
