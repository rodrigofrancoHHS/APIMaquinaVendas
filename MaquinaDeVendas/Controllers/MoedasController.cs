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
    public class MoedasController : ControllerBase
    {
        private readonly MoedasContext _context;

        public MoedasController(MoedasContext context)
        {
            _context = context;
        }

        string connectionString = "Server=localhost;Port=3306;Database=vendingmachine;Uid=root;";


        [HttpGet("ListaDeMoedas")]
        public async Task<ActionResult<IEnumerable<MoedaDTO>>> GetMoedas()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Moeda, MoedaDTO>();
            });

            AutoMapper.IMapper _mapper = config.CreateMapper();

            using (var db = new Database(connectionString, "MySql.Data.MySqlClient"))
            {
                var todasMoedas = await db.FetchAsync<Moeda>("SELECT * FROM moedas");

                var responseMoedas = _mapper.Map<List<MoedaDTO>>(todasMoedas);
                /* var responseItems = todosProdutos.Select(p => new TodosProdutosDTO
                {
                    Id = p.Id,
                    name = p.name,
                    quantity = p.quantity,
                    price = p.price,
                    sold = p.sold
                }).ToList(); */

                return Ok(responseMoedas);
            }
        }

        [HttpGet("MoedaPorId/{id}")]
        public async Task<ActionResult<MoedaDTO>> GetMoeda(long id)
        {
            var moeda = await _context.Moedas.FindAsync(id);

            if (moeda == null)
            {
                return NotFound();
            }

            var moedaDTO = new MoedaDTO
            {
                Id = moeda.Id,
                name = moeda.name,
                price = moeda.price,
                sold = moeda.sold
            };

            return moedaDTO;
        }

        [HttpPost("InserirAtualizarMoedas")]
        public async Task<ActionResult> PostMoedas([FromBody] List<MoedaDTO> moedasDTOList)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<MoedaDTO, Moeda>();
            });

            AutoMapper.IMapper _mapper = config.CreateMapper();

            using (var db = new Database(connectionString, "MySql.Data.MySqlClient"))
            {
                foreach (var todasMoedasDTO in moedasDTOList)
                {
                    var moedaExistente = await db.SingleOrDefaultAsync<Moeda>("SELECT * FROM moedas WHERE id = @0", todasMoedasDTO.Id);

                    if (moedaExistente == null)
                    {
                        var novaMoeda = _mapper.Map<Moeda>(todasMoedasDTO);

                        await db.InsertAsync("moedas", "id", true, novaMoeda);
                    }
                    else
                    {
                        var moedaAtualizado = _mapper.Map<MoedaDTO, Moeda>(todasMoedasDTO, moedaExistente);

                        await db.UpdateAsync("moedas", "id", moedaExistente);
                    }
                }
            }

            return Ok();
        }

        [HttpPost("EliminarMoedas")]
        public async Task<ActionResult> DeleteMoedas([FromBody] List<long> idList)
        {
            var moedas = await _context.Moedas.Where(m => idList.Contains(m.Id)).ToListAsync();

            _context.Moedas.RemoveRange(moedas);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool MoedaExists(long id)
        {
            return _context.Moedas.Any(e => e.Id == id);
        }
    }
}