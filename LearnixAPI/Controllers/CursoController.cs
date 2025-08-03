using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Learnix.Core.DomainEntities;
using Learnix.Core.DTOs.Input;
using Learnix.Core.DTOs.Output;
using LearnixAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnixAPI.Controllers
{
    [ApiController]
    [Route("cursos")]
    public class CursoController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly IConfiguration _config;

        public CursoController(AppDbContext context, IConfiguration configuration)
        {
            _appDbContext = context;
            _config = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetCursos()
        {
            var cursosOutPut = _appDbContext.Cursos.Select(s => new CursosOutput
            {
                DataCadastro = s.DataCadastro.ToString("dd/MM/yyyy hh-mm-ss"),
                Id = s.Id,
                DataUltimaEdicao = s.DataUltimaEdicao.ToString("dd/MM/yyyy hh-mm-ss"),
                Descricao = s.Descricao,
                Nome = s.Nome,
                UsuarioCriacaoId = s.UsuarioCriacaoId,
                UsuarioCriacaoNome = _appDbContext.Usuarios.FirstOrDefault(f => f.Id == s.UsuarioCriacaoId)!.Nome
            });

            return Ok(cursosOutPut);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> PostCursos(CursoInput cursoInput)
        {
            if (await _appDbContext.Cursos.AnyAsync(a => a.Nome == cursoInput.Nome && a.Descricao == cursoInput.Descricao))
                return BadRequest("Curso já cadastrado");

            if (String.IsNullOrEmpty(cursoInput.Nome) || String.IsNullOrEmpty(cursoInput.Descricao))
                return BadRequest("Preencha todos os campos.");

            var user = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            if (user == null)
                return Unauthorized("Usuário não autenticado");

            Curso curso = new Curso
            {
                Nome = cursoInput.Nome,
                DataCadastro = DateTime.Now,
                Descricao = cursoInput.Descricao,
                UsuarioCriacaoId = int.Parse(user)
            };

            _appDbContext.Cursos.Add(curso);
            _appDbContext.SaveChanges();

            var cursoOutPut = new CursosOutput
            {
                DataCadastro = curso.DataCadastro.ToString("dd/MM/yyyy hh-mm-ss"),
                Id = curso.Id,
                DataUltimaEdicao = curso.DataUltimaEdicao.ToString("dd/MM/yyyy hh-mm-ss"),
                Descricao = curso.Descricao,
                Nome = curso.Nome,
                UsuarioCriacaoId = curso.UsuarioCriacaoId,
                UsuarioCriacaoNome = _appDbContext.Usuarios.FirstOrDefault(f => f.Id == curso.UsuarioCriacaoId)!.Nome
            };

            return Ok(cursoOutPut);
        }

        [Authorize(Roles = "Usuario")]
        [HttpPost("{CourceId}/users/")]
        public async Task<IActionResult> InscreverUsuarioEmCurso([FromRoute] int CourceId)
        {
            var curso = _appDbContext.Cursos.FirstOrDefault(f => f.Id == CourceId);
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var usuario = _appDbContext.Usuarios.FirstOrDefault(f => f.Id == userId);
            var usuarioCurso = _appDbContext.UsuarioCursos.FirstOrDefault(f => f.UsuarioId == userId && f.CursoId == CourceId);

            if (curso == null)
                return NotFound("Curso não encontrado");

            if (usuario == null)
                return NotFound("Usuário para inscrição não foi encontrado");

            if (usuarioCurso != null)
                return BadRequest($"{usuario.Nome} já está matriculado(a) no curso {curso.Nome}!");

            UsuarioCurso usuarioCursoN = new UsuarioCurso
            {
                CursoId = curso.Id,
                UsuarioId = usuario.Id,
                DataInscricao = DateTime.Now
            };

            _appDbContext.UsuarioCursos.Add(usuarioCursoN);
            _appDbContext.SaveChanges();

            return Ok($"O usuário {usuario.Nome} foi matriculado(a) no curso {curso.Nome} com sucesso!");
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCurso([FromRoute] int id, [FromBody] CursoInput cursoInput)
        {
            var cursoDB = await _appDbContext.Cursos.FirstOrDefaultAsync(f => f.Id == id);
            if (cursoDB == null)
                return NotFound("Curso não encontrado!");

            if (String.IsNullOrEmpty(cursoInput.Nome) || String.IsNullOrEmpty(cursoInput.Descricao))
                return BadRequest("Preencha todos os campos.");

            cursoDB.Nome = cursoInput.Nome;
            cursoDB.Descricao = cursoInput.Descricao;
            cursoDB.DataUltimaEdicao = DateTime.Now;

            _appDbContext.Cursos.Update(cursoDB);
            await _appDbContext.SaveChangesAsync();

            var cursoOutPut = new CursosOutput
            {
                DataCadastro = cursoDB.DataCadastro.ToString("dd/MM/yyyy hh-mm-ss"),
                Id = cursoDB.Id,
                DataUltimaEdicao = cursoDB.DataUltimaEdicao.ToString("dd/MM/yyyy hh-mm-ss"),
                Descricao = cursoDB.Descricao,
                Nome = cursoDB.Nome,
                UsuarioCriacaoId = cursoDB.UsuarioCriacaoId,
                UsuarioCriacaoNome = _appDbContext.Usuarios.FirstOrDefault(f => f.Id == cursoDB.UsuarioCriacaoId)!.Nome
            };

            return Ok(cursoOutPut);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCurso([FromRoute] int id)
        {
            var curso = _appDbContext.Cursos.FirstOrDefault(f => f.Id == id);
            if (curso == null)
                return NotFound("Curso não encontrado!");

            _appDbContext.Cursos.Remove(curso);
            _appDbContext.SaveChanges();

            return Ok($"{curso.Nome} removido com sucesso!");
        }

        [HttpDelete("{CourceId}/users/{Id}")]
        [Authorize(Roles = "Admin, Usuario")]
        public async Task<IActionResult> RemoverUsuarioDeCurso([FromRoute] int CourceId, [FromRoute] int? Id)
        {
            int? userId = 0;

            if (Id == null || Id == 0)
            {
                if (User.FindFirst(ClaimTypes.Role)!.Value == "Usuario")
                    userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                else
                    return BadRequest();
            }
            else
                userId = Id;

            var usuario = _appDbContext.Usuarios.FirstOrDefault(f => f.Id == userId);
            if (usuario == null)
                return NotFound("Usuário não encontrado!");

            if (CourceId == 0)
                return NotFound("Curso não encontrado!");

            var cursoUserDB = _appDbContext.UsuarioCursos.FirstOrDefault(f => f.UsuarioId == userId && f.CursoId == CourceId);
            if (cursoUserDB == null)
                return NotFound($"A inscrição não foi encontrada!");

            _appDbContext.UsuarioCursos.Remove(cursoUserDB);
            _appDbContext.SaveChanges();
            return Ok("A inscrição foi removida com sucesso!");
        }
    }
}