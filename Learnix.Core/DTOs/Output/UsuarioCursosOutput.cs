using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Learnix.Core.DTOs.Output
{
    public class UsuarioCursosOutput
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string NomeUsuario { get; set; }
        public int CursoId { get; set; }
        public string NomeCurso { get; set; }
        public string DataInscricao { get; set; }
    }
}