using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Learnix.Core.DomainEntities
{
    public class Curso
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nome { get; set; }

        [Required]
        [MaxLength(500)]
        public string Descricao { get; set; }

        public int UsuarioCriacaoId { get; set; }
        public DateTime DataCadastro { get; set; }
        public DateTime DataUltimaEdicao { get; set; } = DateTime.Now;
    }
}