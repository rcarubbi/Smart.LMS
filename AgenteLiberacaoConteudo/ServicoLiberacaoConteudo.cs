using SmartLMS.Dominio;
using SmartLMS.Dominio.Entidades;
using SmartLMS.Dominio.Repositorios;
using System;
using System.Collections.Generic;

namespace AgenteLiberacaoConteudo
{
    public class ServicoLiberacaoConteudo
    {
        public static List<Parametro> Parametros { get; set; }


        private IContexto _contexto;
        public ServicoLiberacaoConteudo(IContexto contexto)
        {
            _contexto = contexto;
        }

        public bool Iniciar() {
            var parametroRepo = new RepositorioParametro(_contexto);
            Parametro.PROJETO = parametroRepo.ObterValorPorChave(Parametro.NOME_PROJETO);
            Console.WriteLine($"{Parametro.PROJETO} - Agente de liberação de acesso a conteúdo iniciado");            
            return true;
        }

        public bool Parar() {
            return true;
        }

    }
}
