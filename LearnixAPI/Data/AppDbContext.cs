using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Learnix.Core.DomainEntities;
using Microsoft.EntityFrameworkCore;

namespace LearnixAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { 
            
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Curso> Cursos { get; set; }
        public DbSet<UsuarioCurso> UsuarioCursos { get; set; }
    }
}