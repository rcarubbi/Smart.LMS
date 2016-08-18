namespace SmartLMS.Dominio.Entidades
{
    public class Parametro : Entidade
    {
        public const string CHAVE_CRIPTOGRAFIA = "ChaveCriptografia";
        public const string REMETENTE_EMAIL = "RemetenteEmail";
        public const string SMTP_SERVIDOR = "ServidorEmail";
        public const string SMTP_PORTA = "PortaSMTP";
        public const string SMTP_USA_SSL = "SMTPUsaSSL";
        public const string SMTP_USUARIO = "SMTPUsuario";
        public const string SMTP_SENHA = "SMTPSenha";
        public const string SMTP_USAR_CREDENCIAIS_PADRAO = "SMTPUsarCredenciaisPadrao";
        public const string CHAVE_AREA_CONHECIMENTO_PLURAL = "NomeAreaConhecimentoPlural";
        public const string CHAVE_AREA_CONHECIMENTO = "NomeAreaConhecimento";
        public const string CHAVE_ASSUNTO = "NomeAssunto";
        public const string CHAVE_CURSO = "NomeCurso";
        public const string CHAVE_AULA = "NomeAula";
        public const string CHAVE_ARQUIVO = "NomeArquivo";
        public const string NOME_PROJETO = "NomeProjeto";
        public const string CHAVE_STORAGE_ARQUIVOS = "StorageArquivos";
        public const string CHAVE_TITULO_AULAS_ASSISTIDAS = "TituloAulasAssistidas";
        public const string CHAVE_TITULO_ULTIMAS_AULAS = "TituloUltimasAulas";
        public const string CHAVE_NOME_DESTINATARIO_FALE_CONOSCO = "NomeDestinatarioFaleConosco";
        public const string CHAVE_EMAIL_DESTINATARIO_FALE_CONOSCO = "EmailDestinatarioFaleConosco";

        public static string AREA_CONHECIMENTO_PLURAL { get; set; }

        public static string PROJETO { get; set; }
        

        public string Chave { get; set; }

        public string Valor { get; set; }

        public static string AREA_CONHECIMENTO { get;  set; }
        public static string ASSUNTO { get;  set; }
        public static string CURSO { get;  set; }
        public static string AULA { get;  set; }
        public static string ARQUIVO { get;  set; }
        public static string TITULO_AULAS_ASSISTIDAS { get; set; }
        
        public static string TITULO_ULTIMAS_AULAS { get; set; }

        public static string STORAGE_ARQUIVOS { get; set; }
        }
}
