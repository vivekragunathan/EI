using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using EasyInstall.Interfaces.Entities;
using System.Collections;

namespace EasyInstall
{
   namespace Interfaces
   {
      public class ActionInfo<T>
      {
         public Action<T> action { get; set; }
         public T arg { get; set; }
      }

      public interface IEntitySchema
      {
         string EntityType { get; }
         string Category { get; }
         IEnumerable<string> SupportedEntityTypes { get; }
         IEnumerable<KeyValuePair<string, SchemaItemInfo>> Attributes { get; }
      }

      public interface IMutableEntitySchema : IEntitySchema
      {
         event EventHandler<SchemaChangedEventArgs> OnAttributeAdded;
         event EventHandler<SchemaChangedEventArgs> OnAttributeRemoved;

         void AddAttribute(string attributeName, SchemaItemInfo schemaItemInfo);
         void RemoveAttribute(string attributeName);

         IMutableEntitySchema Copy();
      }

      public interface IAction
      {
         string Name { get; set; }
         object Result { get; }
         IDictionary Parameters { get; }
         string Description { get; set; }
         void Execute(object sender, IDictionary parameters);
      }

      public interface ICustomAction : IAction
      {
         string Script { get; set; }
         ScriptLanguage Language { get; set; }
      }

      public interface IPrerequisite : IAction
      {
         string ErrorText { get; set; }
      }

      public interface ICustomPrerequisite : IPrerequisite, ICustomAction
      {

      }

      public interface IEntity : IDisposable
      {
         event EventHandler<EntityEventArgs> OnEntityAdded;
         event EventHandler<EntityEventArgs> OnEntityRemoved;
         event EventHandler<EntityAttributeEventArgs> OnAttributeChanging;
         event EventHandler<EntityAttributeEventArgs> OnAttributeChanged;

         IEntitySchema Schema { get; }

         bool IsDeployable { get; }

         string Name { get; set; }
         IEnumerable<KeyValuePair<string, object>> Attributes { get; }
         Guid ID { get; }
         IEnumerable<IEntity> SubEntities { get; }

         IEntity Parent { get; }
         IPackage Package { get; }

         object GetAttribute(string attributeName);
         T GetAttribute<T>(string attributeName);
         void SetAttribute(string attributeName, object value);

         void AddEntity(IEntity entity);
         void RemoveEntity(IEntity entity);

         // Creates an entity as a child of this entity
         IEntity CreateChild(string entityType, string name);

         bool CanAcceptChild(IEntity entity);

         // Gets the content pointed to as Stream
         Stream GetContent();
      }

      public interface IDeployableEntity : IEntity
      {
         event EventHandler<EntityEventArgs> OnDeploying;
         event EventHandler<EntityEventArgs> OnDeployed;
         event EventHandler<EntityEventArgs> OnUndeploying;
         event EventHandler<EntityEventArgs> OnUndeployed;

         IList<ICustomAction> ActionOnDeploying { get; }
         IList<ICustomAction> ActionOnDeployed { get; }
         IList<ICustomAction> ActionOnUndeploying { get; }
         IList<ICustomAction> ActionOnUndeployed { get; }

         void Deploy();
         bool CanBeDeployed();
         void Undeploy();
      }

      public interface IPackage : IDeployableEntity
      {
         event EventHandler<EntityEventArgs> OnBuildStarted;
         event EventHandler<EntityEventArgs> OnEntityPackaging;
         event EventHandler<EntityEventArgs> OnEntityPackaged;
         event EventHandler<EntityEventArgs> OnBuildCompleted;

         event EventHandler<SetupEventArgs> OnSetupBegin;
         event EventHandler<SetupEventArgs> OnSetupCompleted;

         event EventHandler<ProgressEventArgs> OnDeployProgress;
         event EventHandler<ProgressEventArgs> OnUndeployProgress;

         IList<IPrerequisite> Prerequisites { get; }

         IEntity FindEntity(Guid id);
         IEntity GetEntity(Guid id);

         Stream GetData(Guid id);

         // Prepares the package for deployment
         void Build(Stream output);

         void Build(ISetupPackager packager);

         void Save(Stream stream);
      }

      public interface ISetupInfo
      {
         string PackageFile { get; set; }
         string OutputDirectory { get; set; }
         string OutputFileName { get; set; }

         IList<string> Sources { get; }
         IList<string> SourcesDirectories { get; }
         IList<string> ExternalAssemblyReferences { get; }
         IList<string> FrameWorkAssemblyReferences { get; }
         IList<string> LibPath { get; }
         IList<string> ModuleReferences { get; }
         IList<string> EmbeddedResources { get; }
      }

      public interface ISetupPackager
      {
         ISetupInfo SetupInfo { get; }
         void Pack();
      }
   }
}