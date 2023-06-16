﻿using Microsoft.AspNetCore.Mvc;
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




    [HttpPost("Checkout")]
    public async Task<ActionResult<List<TodosProdutosDTO>>> Checkout([FromBody] List<TodosProdutosDTO> selectedItems)
    {
        try
        {
            var quantityToRemove = new Dictionary<string, int>(); // Dicionário para armazenar a quantidade de cada item selecionado

            // Contar a quantidade de cada item selecionado
            foreach (var item in selectedItems)
            {
                if (quantityToRemove.ContainsKey(item.name))
                {
                    quantityToRemove[item.name] += 1; // Se o item já está no dicionário, incrementa a quantidade em 1
                }
                else
                {
                    quantityToRemove[item.name] = 1; // Se o item não está no dicionário, adiciona com a quantidade 1
                }
            }

            var updatedItems = new List<TodosProdutos>(); // Lista para armazenar os produtos atualizados no banco de dados

            foreach (var itemName in quantityToRemove.Keys)
            {
                var quantity = quantityToRemove[itemName]; // Obtém a quantidade do item

                // Atualizar os itens no banco de dados
                var todosProdutos = await _context.TodoProdutos.SingleOrDefaultAsync(p => p.name == itemName); // Procura o produto no banco de dados pelo nome

                if (todosProdutos != null)
                {
                    todosProdutos.quantity -= quantity; // Subtrai a quantidade do item do produto
                    todosProdutos.sold += quantity; // Adiciona a quantidade vendida ao produto

                    updatedItems.Add(todosProdutos); // Adiciona o produto atualizado à lista
                }
            }

            // Salvar as alterações no banco de dados
            await _context.SaveChangesAsync();

            // Retornar todos os produtos atualizados
            var allProducts = await _context.TodoProdutos.ToListAsync(); // Obtém todos os produtos do banco de dados
            var responseItems = allProducts.Select(p => new TodosProdutosDTO
            {
                Id = p.Id,
                name = p.name,
                quantity = p.quantity,
                price = p.price,
                sold = p.sold
            }).ToList(); // Cria uma lista de objetos TodosProdutosDTO com os produtos atualizados

            return Ok(responseItems); // Retorna a lista de produtos atualizados como resposta
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