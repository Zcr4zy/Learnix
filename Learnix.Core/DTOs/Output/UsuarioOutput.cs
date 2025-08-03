using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Learnix.Core.Enums;

namespace Learnix.Core.DTOs.Output
{
    public class UsuarioOutput
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string DataCadastro { get; set; }
        public string DataUltimaEdicao { get; set; }
        public string role { get; set; }
    }
}