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
        // Ler o conteúdo do arquivo JSON
        var filePath = "C:\\Users\\video\\Documents\\GitHub\\APIMaquinaVendas\\MaquinaDeVendas\\arquivo.json";
        var conteudoArquivo = await System.IO.File.ReadAllTextAsync(filePath);

        // Desserializar o conteúdo do arquivo em objetos
        var produtosDeserializados = JsonConvert.DeserializeObject<TodosProdutosJson>(conteudoArquivo);

        // Obter a lista de produtos do objeto desserializado
        var produtosCarregados = produtosDeserializados?.Produtos ?? new List<TodosProdutosDTO>();

        return produtosCarregados;
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
        var filePath = "C:\\Users\\video\\Documents\\GitHub\\APIMaquinaVendas\\MaquinaDeVendas\\arquivo.json"; // Defina o caminho e nome do arquivo desejado
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

        // Atualizar o arquivo JSON após a remoção dos produtos
        var filePath = "C:\\Users\\video\\Documents\\GitHub\\APIMaquinaVendas\\MaquinaDeVendas\\arquivo.json"; // Defina o caminho e nome do arquivo desejado
        var conteudoArquivo = await System.IO.File.ReadAllTextAsync(filePath);

        // Desserializar o conteúdo do arquivo em objetos
        var produtosDeserializados = JsonConvert.DeserializeObject<TodosProdutosJson>(conteudoArquivo);

        // Remover os produtos com os IDs fornecidos da lista carregada
        produtosDeserializados?.Produtos.RemoveAll(p => idList.Contains(p.Id));

        // Serializar a lista atualizada de produtos para JSON
        var json = JsonConvert.SerializeObject(produtosDeserializados);

        // Escrever o JSON de volta para o arquivo
        await System.IO.File.WriteAllTextAsync(filePath, json);

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