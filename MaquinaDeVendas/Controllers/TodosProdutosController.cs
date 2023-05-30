using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaquinaDeVendas.Models;
using Newtonsoft.Json;
using System.IO;

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

    // GET: api/TodoItems
    [HttpGet("Lista de Produtos")]
    public async Task<ActionResult<IEnumerable<TodosProdutosDTO>>> GetTodoItems()
    {
        return await _context.TodoProdutos
            .Select(x => ProdutosToDTO(x))
            .ToListAsync();
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







    [HttpPost("InserirAtualizarProdutos")]
    public async Task<ActionResult> PostTodosProdutos([FromBody] List<TodosProdutosDTO> todosProdutosDTOList)
    {
        foreach (var todosProdutosDTO in todosProdutosDTOList)
        {
            var produtoExistente = await _context.TodoProdutos.FindAsync(todosProdutosDTO.Id);

            if (produtoExistente == null)
            {
                // O produto não existe no banco de dados, então vamos adicioná-lo
                var novoProduto = new TodosProdutos
                {
                    Name = todosProdutosDTO.Name,
                    Quantity = todosProdutosDTO.Quantity,
                    Price = todosProdutosDTO.Price,
                    Sold = todosProdutosDTO.Sold
                };

                _context.TodoProdutos.Add(novoProduto);
            }
            else
            {
                // O produto já existe no banco de dados, então vamos atualizá-lo
                produtoExistente.Name = todosProdutosDTO.Name;
                produtoExistente.Quantity = todosProdutosDTO.Quantity;
                produtoExistente.Price = todosProdutosDTO.Price;
                produtoExistente.Sold = todosProdutosDTO.Sold;
            }
        }

        // Salvar as alterações no banco de dados
        await _context.SaveChangesAsync();

        // Ler os produtos atualizados do banco de dados
        var todosProdutos = await _context.TodoProdutos.ToListAsync();

        // Criar a instância da classe TodosProdutosJson e atribuir a lista de produtos
        var todosProdutosJson = new TodosProdutosJson
        {
            Produtos = todosProdutos.Select(ProdutosToDTO).ToList()
        };

        // Serializar para JSON
        var json = JsonConvert.SerializeObject(todosProdutosJson);

        // Escrever o JSON em um arquivo
        var filePath = "C:\\Users\\video\\Downloads\\arquivo.json"; // Defina o caminho e nome do arquivo desejado
        await System.IO.File.WriteAllTextAsync(filePath, json);

        return Ok();
    }





    [HttpPost("Eliminar Produtos")]
    public async Task<ActionResult> DeleteTodosProdutos([FromBody] List<long> idList)
    {
        foreach (var id in idList)
        {
            var todosProdutos = await _context.TodoProdutos.FindAsync(id);

            if (todosProdutos != null)
            {
                _context.TodoProdutos.Remove(todosProdutos);
            }
        }

        await _context.SaveChangesAsync();

        return Ok();
    }












    private bool TodoProdutosExists(long id)
    {
        return _context.TodoProdutos.Any(e => e.Id == id);
    }

    private static TodosProdutosDTO ProdutosToDTO(TodosProdutos todoProdutos) =>
       new TodosProdutosDTO
       {
           Id = todoProdutos.Id,
           Name = todoProdutos.Name,
           Quantity = todoProdutos.Quantity,
           Price = todoProdutos.Price,
       };
}