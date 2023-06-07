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
    [HttpGet("ListadeProdutos")]
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




    // INPUT: É O QUE ENVIA PARA DENTRO, OUTPUT: É O QUE RECEBE PARA FORA.


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
                    name = todosProdutosDTO.name,
                    quantity = todosProdutosDTO.quantity,
                    price = todosProdutosDTO.price,
                    sold = todosProdutosDTO.sold
                };

                _context.TodoProdutos.Add(novoProduto);
            }
            else
            {
                // O produto já existe no banco de dados, então vamos atualizá-lo
                produtoExistente.name = todosProdutosDTO.name;
                produtoExistente.quantity = todosProdutosDTO.quantity;
                produtoExistente.price = todosProdutosDTO.price;
                produtoExistente.sold = todosProdutosDTO.sold;
            }
        }

        // Salvar as alterações no banco de dados
        await _context.SaveChangesAsync();

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
           name = todoProdutos.name,
           quantity = todoProdutos.quantity,
           price = todoProdutos.price,
       };
}