using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Learnix.Core.DomainEntities
{
    public class UsuarioCurso
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }

        public int CursoId { get; set; }
        [ForeignKey("CursoId")]
        public Curso Curso { get; set; }

        public DateTime DataInscricao { get; set; } = DateTime.Now;
    }
}