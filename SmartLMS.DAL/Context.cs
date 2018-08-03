using Carubbi.GenericRepository;
using Carubbi.Utils.Persistence;
using SmartLMS.DAL.Mapping;
using SmartLMS.Domain;
using SmartLMS.Domain.Entities;
using SmartLMS.Domain.Entities.History;
using SmartLMS.Domain.Entities.UserAccess;
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
    public class Context : DbContext, IContext
    {

        public Context()
              : base("name=connectionString")
        {
        }

        private readonly Dictionary<Type, object> _serializersDictionary = new Dictionary<Type, object>();

        private Serializer<TEntity> GetSerializer<TEntity>()
        {
            Type entityType = typeof(TEntity);
            if (!_serializersDictionary.ContainsKey(entityType))
            {
                _serializersDictionary.Add(entityType, new Serializer<TEntity>());
            }

            return (Serializer<TEntity>)_serializersDictionary[entityType];
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();


            modelBuilder.Entity<Parameter>().Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            modelBuilder.Entity<Log>().Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Configurations.Add(new NoticeConfiguration());
            modelBuilder.Configurations.Add(new UserConfiguration());
            modelBuilder.Configurations.Add(new StudentConfiguration());
            modelBuilder.Configurations.Add(new KnowledgeAreaConfiguration());
            modelBuilder.Configurations.Add(new SubjectConfiguration());
            modelBuilder.Configurations.Add(new CourseConfiguration());
            modelBuilder.Configurations.Add(new ClassConfiguration());
            modelBuilder.Configurations.Add(new ClassAccessConfiguration());
            modelBuilder.Configurations.Add(new FileConfiguration());
            modelBuilder.Configurations.Add(new FileAccessConfiguration());
            modelBuilder.Configurations.Add(new ClassroomConfiguration());
            modelBuilder.Configurations.Add(new ClassroomCourseConfiguration());
            modelBuilder.Configurations.Add(new ClassDeliveryPlanConfiguration());
            modelBuilder.Configurations.Add(new UserNoticeConfiguration());
        }

        public IDbSet<TEntidade> GetList<TEntidade>() where TEntidade : class
        {
            return base.Set<TEntidade>();
        }

        public void Update<TEntidade>(TEntidade objetoAntigo, TEntidade objetoNovo) where TEntidade : class
        {
            Entry(objetoAntigo).CurrentValues.SetValues(objetoNovo);
            Entry(objetoAntigo).State = EntityState.Modified;
        }

        public void Save()
        {
            Save(null);
        }

        public void Save(User loggedUser)
        {

            if (loggedUser != null && ChangeTracker != null)
            {
                foreach (var entry in ChangeTracker?.Entries())
                {
                    if (entry.Entity?.GetType() != typeof(Log))
                    {
                        GerarLog(entry, loggedUser);
                    }
                }

            }
            SaveChanges();
        }


        public Type GetType(object entity)
        {
            var thisType = entity.GetType();
            return thisType.Namespace == "System.Data.Entity.DynamicProxies" ? thisType.BaseType : thisType;
        }


        private void GerarLog(DbEntityEntry entry, User loggedUser)
        {
            if (!(entry.Entity is Entity))
            {
                return;
            }

            var serializer = this
                      .GetType()
                      .GetMethod("GetSerializer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.MakeGenericMethod(GetType(entry.Entity))
                      .Invoke(this, null);

            var serializerType = serializer.GetType();

            var serializeMethodInfo = serializerType
              .GetMethod("XmlSerialize", new[] { GetType(entry.Entity), typeof(XmlAttributeOverrides) });

            var oldXml = string.Empty;
            var newXml = string.Empty;

            if ((EntityState.Deleted | EntityState.Modified).HasFlag(entry.State))
            {
                var proxyCreation = Configuration.ProxyCreationEnabled;
                Configuration.ProxyCreationEnabled = false;
                var entity = entry.OriginalValues.ToObject();
                Configuration.ProxyCreationEnabled = proxyCreation;
                var overrides = IgnoreNavigationProperties(entity);
                oldXml = serializeMethodInfo?.Invoke(serializer, new object[] { entity, overrides }).ToString();
            }

            if ((EntityState.Added | EntityState.Modified).HasFlag(entry.State))
            {
                var proxyCreation = Configuration.ProxyCreationEnabled;
                Configuration.ProxyCreationEnabled = false;
                var entity = entry.CurrentValues.ToObject();
                Configuration.ProxyCreationEnabled = proxyCreation;
                var overrides = IgnoreNavigationProperties(entity);
                newXml = serializeMethodInfo?.Invoke(serializer, new object[] { entity, overrides }).ToString();
            }

            if (!string.IsNullOrWhiteSpace(oldXml) || !string.IsNullOrWhiteSpace(newXml))
            {
                Entry(new Log
                {
                    OldState = oldXml.ToString(),
                    NewState = newXml.ToString(),
                    DateTime = DateTime.Now,
                    User = loggedUser,
                    EntityId = ((Entity)entry.Entity).Id,
                    Type = GetType(entry.Entity).ToString()
                }).State = EntityState.Added;
            }
        }

        private XmlAttributeOverrides IgnoreNavigationProperties(object entity)
        {
            var overrides = new XmlAttributeOverrides();
            var type = GetType(entity);
            var ignore = new XmlAttributes { XmlIgnore = true };

            foreach (var property in type.GetProperties().Where(m => m.PropertyType.IsInterface || (m.PropertyType.IsClass && m.PropertyType.FullName != "System.String")))
            {
                var overridesType = type;
                while (overridesType != null && (overridesType.BaseType != typeof(object) && overridesType.BaseType != typeof(Entity)))
                {
                    overrides.Add(overridesType, property.Name, ignore);
                    overridesType = overridesType.BaseType;
                }
                overrides.Add(overridesType ?? throw new InvalidOperationException(), property.Name, ignore);
            }

            return overrides;
        }



        IDbSet<T> IDbContext.Set<T>()
        {
            return GetList<T>();
        }

        public void ConfigureAPI()
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }


        public TEntity UnProxy<TEntity>(TEntity proxyObject) where TEntity : class
        {
            var proxyCreationEnabled = Configuration.ProxyCreationEnabled;
            try
            {
                Configuration.ProxyCreationEnabled = false;
                var entity = Entry(proxyObject).CurrentValues.ToObject() as TEntity;
                return entity;
            }
            finally
            {
                Configuration.ProxyCreationEnabled = proxyCreationEnabled;
            }
        }

        public void Reload<TEntity>(TEntity entity) where TEntity : class
        {
            Entry<TEntity>(entity).Reload();
        }

    }
}
