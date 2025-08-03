using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.InteropServices.Swift;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Learnix.Core.DomainEntities;
using Learnix.Core.DTOs.Input;
using Learnix.Core.DTOs.Output;
using LearnixAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LearnixAPI.Controllers
{
    [ApiController]
    [Route("usuarios")]
    public class UsuarioController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly IConfiguration _config;

        public UsuarioController(AppDbContext context, IConfiguration configuration)
        {
            _appDbContext = context;
            _config = configuration;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetUsuarios()
        {
            var usuariosQuery = _appDbContext.Usuarios.Select(s => new UsuarioOutput
            {
                DataCadastro = s.DataCadastro.ToString("dd/MM/yyyy hh-mm-ss"),
                DataUltimaEdicao = s.DataUltimaEdicao.ToString("dd/MM/yyyy hh-mm-ss"),
                Id = s.Id,
                Email = s.Email,
                Nome = s.Nome,
                role = s.role.ToString()
            });

            return Ok(usuariosQuery);
        }

        [Authorize(Roles = "Admin, Usuario")]
        [HttpGet("{Id}/cursos")]
        public async Task<IActionResult> RecuperarCursosDoUsuario([FromRoute] int? Id)
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

            var cursosUser = _appDbContext.UsuarioCursos.Where(w => w.UsuarioId == userId).ToList();
            List<UsuarioCursosOutput> usuarioCursosOutput = new List<UsuarioCursosOutput>();

            foreach (var usuarioCurso in cursosUser)
            {
                usuarioCursosOutput.Add(new UsuarioCursosOutput
                {
                    Id = usuarioCurso.Id,
                    CursoId = usuarioCurso.CursoId,
                    NomeCurso = _appDbContext.Cursos.FirstOrDefault(f => f.Id == usuarioCurso.CursoId)!.Nome,
                    UsuarioId = userId ?? 0,
                    NomeUsuario = _appDbContext.Usuarios.FirstOrDefault(f => f.Id == userId)!.Nome,
                    DataInscricao = usuarioCurso.DataInscricao.ToString("dd/MM/yyyy hh-mm-ss"),
                });
            }

            return Ok(usuarioCursosOutput);
        }
        
        [HttpPost]
        public async Task<IActionResult> PostUsuario(UsuarioInput usuarioInput)
        {
            if (await _appDbContext.Usuarios.AnyAsync(a => a.Nome == usuarioInput.Nome && a.Email == usuarioInput.Email))
                return BadRequest("Usuário já cadastrado");

            if (String.IsNullOrEmpty(usuarioInput.Nome) || String.IsNullOrEmpty(usuarioInput.Email) || String.IsNullOrEmpty(usuarioInput.Senha))
                return BadRequest("Preencha todos os campos.");

            Usuario usuario = new Usuario
            {
                Email = usuarioInput.Email,
                Senha = BCrypt.Net.BCrypt.HashPassword(usuarioInput.Senha),
                Nome = usuarioInput.Nome,
                role = Learnix.Core.Enums.Roles.Usuario,
                DataCadastro = DateTime.Now
            };

            _appDbContext.Usuarios.Add(usuario);
            _appDbContext.SaveChanges();

            var usuarioOutput = new UsuarioOutput
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                DataCadastro = usuario.DataCadastro.ToString("dd/MM/yyyy"),
                DataUltimaEdicao = usuario.DataUltimaEdicao.ToString("dd/MM/yyyy hh-mm-ss"),
                role = usuario.role.ToString()
            };

            return Ok(usuarioOutput);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{Id}")]
        public async Task<IActionResult> UpdateUsuario([FromRoute] int Id, [FromBody] UsuarioInput usuarioInput)
        {
            var usuarioDB = await _appDbContext.Usuarios.FirstOrDefaultAsync(f => f.Id == Id);
            if (usuarioDB == null)
                return NotFound("Usuário não encontrado!");
                
            if (String.IsNullOrEmpty(usuarioInput.Nome) || String.IsNullOrEmpty(usuarioInput.Email) || String.IsNullOrEmpty(usuarioInput.Senha))
                return BadRequest("Preencha todos os campos.");

            usuarioDB.Nome = usuarioInput.Nome;
            usuarioDB.Email = usuarioInput.Email;
            usuarioDB.Senha = BCrypt.Net.BCrypt.HashPassword(usuarioInput.Senha);
            usuarioDB.DataUltimaEdicao = DateTime.Now;

            _appDbContext.Usuarios.Update(usuarioDB);
            await _appDbContext.SaveChangesAsync();

            var usuarioOutput = new UsuarioOutput
            {
                Id = usuarioDB.Id,
                Nome = usuarioDB.Nome,
                Email = usuarioDB.Email,
                DataCadastro = usuarioDB.DataCadastro.ToString("dd/MM/yyyy"),
                DataUltimaEdicao = usuarioDB.DataUltimaEdicao.ToString("dd/MM/yyyy hh-mm-ss"),
                role = usuarioDB.role.ToString()
            };

            return Ok(usuarioOutput);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{Id}")]
        public async Task<IActionResult> DeleteUsuario([FromRoute] int Id)
        {
            var usuario = _appDbContext.Usuarios.FirstOrDefault(f => f.Id == Id);
            if (usuario == null)
                return NotFound("Usuário não encontrado!");

            _appDbContext.Usuarios.Remove(usuario);
            _appDbContext.SaveChanges();

            return Ok($"{usuario.Nome} removido com sucesso!");
        }

        [HttpPost("login")]
        public async Task<IActionResult> UsuarioLogin([FromBody] UsuarioLogin usuarioLogin)
        {
            var usuario = _appDbContext.Usuarios.FirstOrDefault(f => f.Email == usuarioLogin.Email);
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(usuarioLogin.Senha, usuario.Senha))
                return Unauthorized("Usuário ou senha inválido!");

            var token = GerarToken(usuario);
            return Ok ( new { token });
        }


        private string GerarToken(Usuario usuario)
        {
            var claims = new[]{
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt"] ?? ""));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}