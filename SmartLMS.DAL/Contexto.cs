using Carubbi.GenericRepository;
using Carubbi.Utils.Persistence;
using Carubbi.Utils.Data;
using SmartLMS.DAL.Mapeamento;
using SmartLMS.Dominio;
using SmartLMS.Dominio.Entidades;
using SmartLMS.Dominio.Entidades.Historico;
using SmartLMS.Dominio.Entidades.Pessoa;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Xml.Serialization;

namespace SmartLMS.DAL
{
    public class Contexto : DbContext, IContexto
    {

        public Contexto()
              : base("name=StringConexao")
        {
        }

        private Dictionary<Type, object> _dicionarioSerializadores = new Dictionary<Type, object>();

        private Serializer<TEntidade> ObterSerializador<TEntidade>()
        {
            Type tipoEntidade = typeof(TEntidade);
            if (!_dicionarioSerializadores.ContainsKey(tipoEntidade))
            {
                _dicionarioSerializadores.Add(tipoEntidade, new Serializer<TEntidade>());
            }

            return (Serializer<TEntidade>)_dicionarioSerializadores[tipoEntidade];
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();


            modelBuilder.Entity<Parametro>().Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            modelBuilder.Entity<Log>().Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Configurations.Add(new AvisoConfiguration());
            modelBuilder.Configurations.Add(new UsuarioConfiguration());
            modelBuilder.Configurations.Add(new AlunoConfiguration());
            modelBuilder.Configurations.Add(new AreaConhecimentoConfiguration());
            modelBuilder.Configurations.Add(new AssuntoConfiguration());
            modelBuilder.Configurations.Add(new CursoConfiguration());
            modelBuilder.Configurations.Add(new AulaConfiguration());
            modelBuilder.Configurations.Add(new AcessoAulaConfiguration());
            modelBuilder.Configurations.Add(new ArquivoConfiguration());
            modelBuilder.Configurations.Add(new AcessoArquivoConfiguration());
            modelBuilder.Configurations.Add(new TurmaConfiguration());
            modelBuilder.Configurations.Add(new TurmaCursoConfiguration());
            modelBuilder.Configurations.Add(new AulaPlanejamentoConfiguration());
            modelBuilder.Configurations.Add(new UsuarioAvisoConfiguration());
        }

        public IDbSet<TEntidade> ObterLista<TEntidade>() where TEntidade : class
        {
            return base.Set<TEntidade>();
        }

        public void Atualizar<TEntidade>(TEntidade objetoAntigo, TEntidade objetoNovo) where TEntidade : class
        {
            Entry(objetoAntigo).CurrentValues.SetValues(objetoNovo);
            Entry(objetoAntigo).State = EntityState.Modified;
        }

        public void Salvar()
        {
            Salvar(null);
        }

        public void Salvar(Usuario usuarioLogado)
        {
            DateTime agora = DateTime.Now;
            if (usuarioLogado != null)
            {
                foreach (var entry in ChangeTracker?.Entries()?.Where(x => x.Entity?.GetType() != typeof(Log)))
                {
                    GerarLog(entry, usuarioLogado);
                }
            }
            SaveChanges();
        }


        public Type GetType(object entity)
        {
            var thisType = entity.GetType();

            if (thisType.Namespace == "System.Data.Entity.DynamicProxies")
                return thisType.BaseType;

            return thisType;
        }


        private void GerarLog(DbEntityEntry entry, Usuario usuarioLogado)
        {
            if (!(entry.Entity is Entidade))
            {
                return;
            }

            var serializador = this
                      .GetType()
                      .GetMethod("ObterSerializador", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                      .MakeGenericMethod(GetType(entry.Entity))
                      .Invoke(this, null);

            var tipoSerializador = serializador.GetType();

            var metodoSerializar = tipoSerializador
              .GetMethod("XmlSerialize", new Type[] { GetType(entry.Entity), typeof(XmlAttributeOverrides) });

            var xmlAntigo = string.Empty;
            var xmlNovo = string.Empty;

            if ((EntityState.Deleted | EntityState.Modified).HasFlag(entry.State))
            {
                var proxyCreation = Configuration.ProxyCreationEnabled;
                Configuration.ProxyCreationEnabled = false;
                var entity = entry.OriginalValues.ToObject();
                Configuration.ProxyCreationEnabled = proxyCreation;
                var overrides = IgnoreNavigationProperties(entity);
                xmlAntigo = metodoSerializar.Invoke(serializador, new object[] { entity, overrides }).ToString();
            }

            if ((EntityState.Added | EntityState.Modified).HasFlag(entry.State))
            {
                var proxyCreation = Configuration.ProxyCreationEnabled;
                Configuration.ProxyCreationEnabled = false;
                var entity = entry.CurrentValues.ToObject();
                Configuration.ProxyCreationEnabled = proxyCreation;
                var overrides = IgnoreNavigationProperties(entity);
                xmlNovo = metodoSerializar.Invoke(serializador, new object[] { entity, overrides }).ToString();
            }

            if (!string.IsNullOrWhiteSpace(xmlAntigo) || !string.IsNullOrWhiteSpace(xmlNovo))
            {
                Entry(new Log
                {
                    EstadoAntigo = xmlAntigo.ToString(),
                    EstadoNovo = xmlNovo.ToString(),
                    DataHora = DateTime.Now,
                    Usuario = usuarioLogado,
                    IdEntitdade = ((Entidade)entry.Entity).Id,
                    Tipo = GetType(entry.Entity).ToString()
                }).State = EntityState.Added;
            }
        }

        private XmlAttributeOverrides IgnoreNavigationProperties(object entity)
        {
       

            XmlAttributeOverrides overrides = new XmlAttributeOverrides();
            var type = GetType(entity);
            var ignore = new XmlAttributes { XmlIgnore = true };

            foreach (var property in type.GetProperties().Where(m => m.PropertyType.IsInterface || (m.PropertyType.IsClass && m.PropertyType.FullName != "System.String")))
            {
                Type overridesType = type;
                while (overridesType.BaseType != typeof(object) && overridesType.BaseType != typeof(Entidade))
                {
                    overrides.Add(overridesType, property.Name, ignore);
                    overridesType = overridesType.BaseType;
                }
                overrides.Add(overridesType, property.Name, ignore);
            }

            return overrides;
        }



        IDbSet<T> IDbContext.Set<T>()
        {
            return ObterLista<T>();
        }

        public void ConfigurarParaApi()
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }


        public T UnProxy<T>(T proxyObject) where T : class
        {
            var proxyCreationEnabled = Configuration.ProxyCreationEnabled;
            try
            {
                Configuration.ProxyCreationEnabled = false;
                T poco = Entry(proxyObject).CurrentValues.ToObject() as T;
                return poco;
            }
            finally
            {
                Configuration.ProxyCreationEnabled = proxyCreationEnabled;
            }
        }

        public void Recarregar<T>(T entidade) where T : class
        {
            Entry<T>(entidade).Reload();
        }

    }
}
