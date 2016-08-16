using SmartLMS.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.DAL.Mapeamento
{
    public class AcessoArquivoConfiguration : EntityTypeConfiguration<AcessoArquivo>
    {
        public AcessoArquivoConfiguration()
        {
            HasRequired(x => x.Usuario);
            HasRequired(x => x.Arquivo);

        }
    }
}
