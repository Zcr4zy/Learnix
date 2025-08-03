using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Learnix.Core.DTOs.Output
{
    public class CursosOutput
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public int UsuarioCriacaoId { get; set; }
        public string UsuarioCriacaoNome { get; set; }
        public string DataCadastro { get; set; }
        public string DataUltimaEdicao { get; set; }
    }
}