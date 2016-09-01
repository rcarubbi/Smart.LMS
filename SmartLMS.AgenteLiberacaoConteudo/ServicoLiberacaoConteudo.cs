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
            Parametro.AREA_CONHECIMENTO_PLURAL = parametroRepo.ObterValorPorChave(Parametro.CHAVE_AREA_CONHECIMENTO_PLURAL);
            Parametro.AREA_CONHECIMENTO = parametroRepo.ObterValorPorChave(Parametro.CHAVE_AREA_CONHECIMENTO);
            Parametro.CURSO_PLURAL = parametroRepo.ObterValorPorChave(Parametro.CHAVE_CURSO_PLURAL);
            Parametro.CURSO = parametroRepo.ObterValorPorChave(Parametro.CHAVE_CURSO);
            Parametro.ASSUNTO_PLURAL = parametroRepo.ObterValorPorChave(Parametro.CHAVE_ASSUNTO_PLURAL);
            Parametro.ASSUNTO = parametroRepo.ObterValorPorChave(Parametro.CHAVE_ASSUNTO);
            Parametro.AULA = parametroRepo.ObterValorPorChave(Parametro.CHAVE_AULA);
            Parametro.AULA_PLURAL = parametroRepo.ObterValorPorChave(Parametro.CHAVE_AULA_PLURAL);
            Parametro.ARQUIVO = parametroRepo.ObterValorPorChave(Parametro.CHAVE_ARQUIVO);
            Parametro.TITULO_AULAS_ASSISTIDAS = parametroRepo.ObterValorPorChave(Parametro.CHAVE_TITULO_AULAS_ASSISTIDAS);
            Parametro.STORAGE_ARQUIVOS = parametroRepo.ObterValorPorChave(Parametro.CHAVE_STORAGE_ARQUIVOS);
            Parametro.TITULO_ULTIMAS_AULAS = parametroRepo.ObterValorPorChave(Parametro.CHAVE_TITULO_ULTIMAS_AULAS);

            Console.WriteLine($"{Parametro.PROJETO} - Agente de liberação de acesso a conteúdo iniciado");            
            return true;
        }

        public bool Parar() {
            return true;
        }

    }
}
