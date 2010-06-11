using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using EasyInstall.Interfaces;
using EasyInstall.Interfaces.Entities;

namespace EasyInstall
{
   [DataContract]
   public abstract class DeployableEntity : IDeployableEntity
   {
      protected static IDictionary<Guid, IEntity> allEntities = new Dictionary<Guid, IEntity>();
      [DataMember] private IList<ICustomAction> _actionOnDeployed = new List<ICustomAction>();
      [DataMember] private IList<ICustomAction> _actionOnDeploying = new List<ICustomAction>();
      [DataMember] private IList<ICustomAction> _actionOnUndeployed = new List<ICustomAction>();
      [DataMember] private IList<ICustomAction> _actionOnUndeploying = new List<ICustomAction>();

      [DataMember] protected IDictionary<string, object> _attributes = new Dictionary<string, object>();

      [DataMember(Name = "ID")] private Guid _id = Guid.NewGuid();
      [DataMember] private EntitySchema _overriddenSchema;
      [DataMember] private bool _overrideSchema;

      [DataMember(Name = "PackageID")] private Guid _packageID = Guid.Empty;
      [DataMember(Name = "ParentID")] private Guid _parentID = Guid.Empty;
      [DataMember(Name = "SubEntities")] protected IList<IEntity> _subEntities = new List<IEntity>();

      [DataMember]
      private string SchemaName { get; set; }

      protected virtual bool MustHaveParent
      {
         get { return true; }
      }

      #region ABSTRACT METHODS AND OVERRIDABLES

      public abstract bool CanBeDeployed();
      protected abstract void OnDeploy();
      protected abstract void OnUndeploy();

      protected virtual void AttributeChanged(EntityAttributeEventArgs e)
      {
         // Deliberate empty implementation to make the implementation optional for sub classes
      }

      protected virtual void AttributeChanging(EntityAttributeEventArgs e)
      {
         // Deliberate empty implementation to make the implementation optional for sub classes
      }

      #endregion

      #region IDeployableEntity Members

      public bool IsDeployable
      {
         get { return true; }
      }

      public event EventHandler<EntityEventArgs> OnDeploying;
      public event EventHandler<EntityEventArgs> OnDeployed;
      public event EventHandler<EntityEventArgs> OnUndeploying;
      public event EventHandler<EntityEventArgs> OnUndeployed;
      public event EventHandler<EntityAttributeEventArgs> OnAttributeChanging;
      public event EventHandler<EntityAttributeEventArgs> OnAttributeChanged;

      public IList<ICustomAction> ActionOnDeploying
      {
         get { return _actionOnDeploying; }
      }

      public IList<ICustomAction> ActionOnDeployed
      {
         get { return _actionOnDeployed; }
      }

      public IList<ICustomAction> ActionOnUndeploying
      {
         get { return _actionOnUndeploying; }
      }

      public IList<ICustomAction> ActionOnUndeployed
      {
         get { return _actionOnUndeployed; }
      }

      [DataMember]
      public string Name { get; set; }

      public IEnumerable<KeyValuePair<string, object>> Attributes
      {
         get { return _attributes; }
      }

      public IEntitySchema Schema
      {
         get
         {
            if (_overrideSchema)
               return _overriddenSchema;

            return EntitySchema.GetSchema(SchemaName);
         }
      }

      public virtual void Deploy()
      {
         if (MustHaveParent && Parent == null)
         {
            throw new Exception("Entity does not have a parent!");
         }

         if (OnDeploying != null)
            OnDeploying(this, new EntityEventArgs(this));

         ExecuteActions(ActionOnDeploying);

         if (!CanBeDeployed())
            throw new EntityCannotBeDeployedException(Name, "CanDeploy Failed");

         OnDeploy();

         if (OnDeployed != null)
            OnDeployed(this, new EntityEventArgs(this));

         ExecuteActions(ActionOnDeployed);
      }

      public virtual void Undeploy()
      {
         if (MustHaveParent && Parent == null)
         {
            throw new Exception("Entity does not have a parent!");
         }

         if (OnUndeploying != null)
            OnUndeploying(this, new EntityEventArgs(this));

         // Call the custom actions
         ExecuteActions(ActionOnUndeploying);

         OnUndeploy();

         if (OnUndeployed != null)
            OnUndeployed(this, new EntityEventArgs(this));

         ExecuteActions(ActionOnUndeployed);
      }

      public object GetAttribute(string attributeName)
      {
         if (!_attributes.ContainsKey(attributeName))
         {
            throw new AttributeNotFoundException(attributeName);
         }

         return _attributes[attributeName];
      }

      public T GetAttribute<T>(string attributeName)
      {
         if (!_attributes.ContainsKey(attributeName))
         {
            throw new AttributeNotFoundException(attributeName);
         }

         return (T) _attributes[attributeName];
      }

      public void SetAttribute(string attributeName, object value)
      {
         var item = (from i in Schema.Attributes
                     where i.Key == attributeName
                     select i);

         if (null == item)
         {
            throw new AttributeNotFoundException(attributeName);
         }

         var oldValue = _attributes[attributeName];

         if (Equals(oldValue, value))
            return;

         EntitySchema.CheckTypeCompatibility(value, item.Single().Value);

         var newValue = Convert.ChangeType(value, item.Single().Value.ItemType);

         FireAttributeChangingEvent(attributeName, oldValue, newValue);
         _attributes[attributeName] = value;
         FireAttributeChangedEvent(attributeName, oldValue, newValue);
      }

      public IEnumerable<IEntity> SubEntities
      {
         get { return _subEntities; }
      }

      public void AddEntity(IEntity entity)
      {
         if (!CanAcceptChild(entity))
         {
            throw new Exception("Entity does not support child entity of specified type");
         }

         _subEntities.Add(entity);

         if (OnEntityAdded != null)
            OnEntityAdded(this, new EntityEventArgs(entity));
      }

      public void RemoveEntity(IEntity entity)
      {
         if (!_subEntities.Contains(entity)) return;

         _subEntities.DisposeAndRemove(entity);

         if (OnEntityRemoved != null)
            OnEntityRemoved(this, new EntityEventArgs(entity));
      }

      public virtual bool CanAcceptChild(IEntity entity)
      {
         return Schema.SupportedEntityTypes.Contains(entity.Schema.EntityType);
      }

      public event EventHandler<EntityEventArgs> OnEntityAdded;
      public event EventHandler<EntityEventArgs> OnEntityRemoved;

      public Guid ID
      {
         get { return _id; }
      }

      public IEntity Parent
      {
         get
         {
            IEntity ent = FindEntity(_parentID);
            if (null == ent)
            {
               throw new Exception(string.Format("Entity with ID '{0}' not found", _parentID));
            }

            return ent;
         }
      }

      public IPackage Package
      {
         get { return PackageManager.GetPackage(_packageID); }
         private set { _packageID = value.ID; }
      }

      public virtual IEntity CreateChild(string entityType, string name)
      {
         // Check if specified entity Type is supported
         if (!CanAcceptChild(entityType))
            throw new EntityTypeNotSupportedException(Schema.EntityType, entityType);

         var entity = EntityFactory.CreateEntity(entityType, name, Package, ID, null);

         AddEntity(entity);

         return entity;
      }

      public virtual Stream GetContent()
      {
         return null;
      }

      #endregion

      #region CONSTRUCTOR

      protected DeployableEntity() // For Serialization
      {
         RegisterEvents();
      }

      protected DeployableEntity(string entityName) // For Serialization
      {
         SchemaName = EntitySchema.GetSchema(entityName).EntityType;
         RegisterEvents();
      }

      protected DeployableEntity(string name, string schema, IPackage package, Guid parentID)
      {
         if (package == null)
         {
            if (GetType() != typeof (Package))
               throw new ArgumentNullException("package");
         }

         Package = package;
         _packageID = package.ID;

         if (string.IsNullOrEmpty(schema))
         {
            throw new InvalidSchemaException();
         }

         // This will check if the specified parent entity ID exists
         var parent = Package.GetEntity(parentID);

         _parentID = parent.ID; // let the compiler not optimize the previous check
         // by making use of the returned object

         Name = name;
         SchemaName = schema;

         // Pre-Populate the attributes with default values from the schema
         foreach (var kvp in Schema.Attributes)
         {
            _attributes[kvp.Key] = kvp.Value.DefaultValue;
         }

         RegisterEvents();

         allEntities[ID] = this;
      }

      private void AttributeChangingHandler(object sender, EntityAttributeEventArgs e)
      {
         AttributeChanging(e);
      }

      private void AttributeChangedHandler(object sender, EntityAttributeEventArgs e)
      {
         AttributeChanged(e);
      }

      #endregion

      private void RegisterEvents()
      {
         OnAttributeChanged += AttributeChangedHandler;
         OnAttributeChanging += AttributeChangingHandler;
      }

      [OnDeserialized]
      internal void _OnDeserialized(StreamingContext context)
      {
         RegisterEvents();

         OnDeserialized(context);

         foreach (IEntity ent in SubEntities)
         {
            var de = ent as DeployableEntity;

            if (de != null)
            {
               de.OnParentDeserialized(this);
            }
         }
      }

      protected virtual void OnDeserialized(StreamingContext context)
      {
         allEntities[ID] = this;
      }

      protected virtual void OnParentDeserialized(IEntity entity)
      {
      }

      internal IEntity FindEntity(Guid id)
      {
         if (id == ID)
         {
            return this;
         }

         if (allEntities.ContainsKey(id))
         {
            return allEntities[id];
         }

         return null;
      }

      // Some entities might want to override the existing schema with a new schema
      protected void OverrideSchema(EntitySchema newSchema)
      {
         if (ReferenceEquals(newSchema, Schema))
            return;

         if (null == Schema)
            throw new ArgumentNullException("newSchema");

         _overriddenSchema = newSchema;
         _overrideSchema = true;
      }

      protected void ExecuteActions<T>(IEnumerable<T> actions) where T : IAction
      {
         foreach (T action in actions)
         {
            action.Execute(this, action.Parameters);
         }
      }

      private void FireAttributeChangedEvent(string attrName, object oldVal, object newVal)
      {
         if (OnAttributeChanged != null)
         {
            OnAttributeChanged(this, new EntityAttributeEventArgs(this, attrName, oldVal, newVal));
         }
      }

      private void FireAttributeChangingEvent(string attrName, object oldVal, object newVal)
      {
         if (OnAttributeChanging != null)
         {
            OnAttributeChanging(this, new EntityAttributeEventArgs(this, attrName, oldVal, newVal));
         }
      }

      public virtual bool CanAcceptChild(string entityType)
      {
         return Schema.SupportedEntityTypes.Contains(entityType);
      }

      public override string ToString()
      {
         return Name;
      }

      #region Dispose Pattern

      protected bool disposed { get; private set; }

      public void Dispose()
      {
         Dispose(true);

         GC.SuppressFinalize(this);
      }

      protected virtual void Dispose(bool disposing)
      {
         if (!disposed)
         {
            if (disposing)
            {
               // Dispose any unmanaged resources here
            }

            OnAttributeChanged = null;
            OnAttributeChanging = null;
            OnDeployed = null;
            OnDeploying = null;
            OnEntityAdded = null;
            OnEntityRemoved = null;
            OnUndeployed = null;
            OnUndeploying = null;

            if (_subEntities != null)
            {
               // This check is needed here since the dispose
               // could be called from the finalizer when the object's ctor throws an exception
               _subEntities.DisposeAndClear();
            }

            disposed = true;

            Trace.WriteLine(string.Format("Disposed: Deployable entity {0}", ToString()));
         }
      }

      ~DeployableEntity()
      {
         Dispose(false);
      }

      #endregion
   }
}