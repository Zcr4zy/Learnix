using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Learnix.Core.Enums;

namespace Learnix.Core.DomainEntities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public DateTime DataCadastro { get; set; }
        public Roles role { get; set; } = Roles.Admin;
        public DateTime DataUltimaEdicao { get; set; } = DateTime.Now;
    }
}