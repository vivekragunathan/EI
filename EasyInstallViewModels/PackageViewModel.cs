using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyInstall.Interfaces;
using EasyInstall.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using EasyInstall.Interfaces.Entities;

namespace EasyInstall.ViewModels
{
   public static class EVMFactory
   {
      public static EntityViewModel CreateEntity(IEntity entity)
      {
         if ( entity is IDeployableEntity )
            return new DeployableEntityViewModel(entity as IDeployableEntity);

         return new EntityViewModel(entity);
      }
   }

   public class EntityViewModel : ViewModelBase, ICustomTypeDescriptor
   {
      public IEntity Entity { get; protected set; }

      private PropertyDescriptorCollection m_PropertyDescriptorCollectionCache;

      protected EntityViewModel AddedEVM
      {
         get;
         private set;
      }

      protected EntityViewModel RemovedEVM
      {
         get;
         private set;
      }

      public EntityViewModel(IEntity entity)
      {
         this.Entity = entity;

         SubEntities = new AsyncObservableCollection<EntityViewModel>();

         foreach ( var e in entity.SubEntities )
         {
            SubEntities.Add(EVMFactory.CreateEntity(e));
         }

         entity.OnEntityAdded += new EventHandler<EntityEventArgs>(entity_OnEntityAdded);
         entity.OnEntityRemoved += new EventHandler<EntityEventArgs>(entity_OnEntityRemoved);
      }

      void entity_OnEntityRemoved(object sender, EntityEventArgs e)
      {
         RemovedEVM = SubEntities.First((en) => en.Entity == e.Entity);
         SubEntities.Remove(RemovedEVM);
      }

      void entity_OnEntityAdded(object sender, EntityEventArgs e)
      {
         AddedEVM = EVMFactory.CreateEntity(e.Entity);
         SubEntities.Add(AddedEVM);
      }

      public AsyncObservableCollection<EntityViewModel> SubEntities
      {
         get;
         private set;
      }

      #region ICustomTypeDescriptor Members

      public AttributeCollection GetAttributes()
      {
         return TypeDescriptor.GetAttributes(Entity, true);
      }

      public string GetClassName()
      {
         return Entity.Schema.EntityType;
      }

      public string GetComponentName()
      {
         return TypeDescriptor.GetComponentName(this, true);
      }

      public TypeConverter GetConverter()
      {
         throw new NotImplementedException();
      }

      public EventDescriptor GetDefaultEvent()
      {
         return null;
      }

      public PropertyDescriptor GetDefaultProperty()
      {
         return null;
      }

      public object GetEditor(Type editorBaseType)
      {
         return null;
      }

      public EventDescriptorCollection GetEvents(Attribute[] attributes)
      {
         return null;
      }

      public EventDescriptorCollection GetEvents()
      {
         return null;
      }

      PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
      {
         if ( m_PropertyDescriptorCollectionCache == null )
         {
            PropertyDescriptor[] properties = new PropertyDescriptor[Entity.Attributes.Count()];
            int i = 0;

            foreach ( var attr in Entity.Attributes )
            {
               properties[i] = new EntityPropertDescriptor(Entity, attr.Key);
               i++;
            }
            m_PropertyDescriptorCollectionCache = new PropertyDescriptorCollection(properties);
         }
         return m_PropertyDescriptorCollectionCache;
      }

      public PropertyDescriptorCollection GetProperties()
      {
         return ((ICustomTypeDescriptor)this).GetProperties(null);
      }

      public object GetPropertyOwner(PropertyDescriptor pd)
      {
         return this;
      }

      #endregion
   }

   public class DeployableEntityViewModel : EntityViewModel
   {
      public IDeployableEntity DeployableEntity
      {
         get
         {
            return Entity as IDeployableEntity;
         }
      }

      public DeployableEntityViewModel(IDeployableEntity entity)
         : base(entity)
      {

      }
   }

   public class PackageViewModel : DeployableEntityViewModel
   {
      public IPackage Package
      {
         get
         {
            return DeployableEntity as IPackage;
         }
      }

      void RegisterEventHandlers()
      {
         Package.OnEntityAdded += entity_OnEntityAdded;
         Package.OnEntityRemoved += entity_OnEntityRemoved;

         Package.OnBuildStarted += Package_OnBuildStarted;
         Package.OnBuildCompleted += Package_OnBuildCompleted;
         Package.OnEntityPackaging += Package_OnEntityPackaging;
         Package.OnEntityPackaged += Package_OnEntityPackaged;

         Package.OnSetupBegin += Package_OnSetupBegin;
         Package.OnSetupCompleted += Package_OnSetupCompleted;
      }

      void UnregisterEventHandlers()
      {
         Package.OnEntityAdded -= entity_OnEntityAdded;
         Package.OnEntityRemoved -= entity_OnEntityRemoved;

         Package.OnBuildStarted -= Package_OnBuildStarted;
         Package.OnBuildCompleted -= Package_OnBuildCompleted;
         Package.OnEntityPackaging -= Package_OnEntityPackaging;
         Package.OnEntityPackaged -= Package_OnEntityPackaged;

         Package.OnSetupBegin -= Package_OnSetupBegin;
         Package.OnSetupCompleted -= Package_OnSetupCompleted;
      }

      public PackageViewModel(string name)
         : this(EasyInstall.Package.Create(name))
      {

      }

      public PackageViewModel(IPackage entity)
         : base(entity)
      {
         CategorizedEntities = new AsyncObservableCollection<KeyValuePair<string, AsyncObservableCollection<EntityViewModel>>>();
         // Initialize the Categories

         foreach ( var cat in EntitySchema.Categories )
         {
            CategorizedEntities.Add(new KeyValuePair<string, AsyncObservableCollection<EntityViewModel>>(cat,
               new AsyncObservableCollection<EntityViewModel>()));
         }

         foreach ( var item in SubEntities )
         {
            string category = item.Entity.Schema.Category;
            GetEntityListForCatagory(category).Add(item);
         }

         BuildOutput = new AsyncObservableCollection<string>();

         RegisterEventHandlers();
      }

      void entity_OnEntityRemoved(object sender, EntityEventArgs e)
      {
         GetEntityListForCatagory(e.Entity.Schema.Category).Remove(RemovedEVM);
      }

      IList<EntityViewModel> GetEntityListForCatagory(string category)
      {
         return (from item in CategorizedEntities
                 where item.Key == category
                 select item.Value).Single();
      }

      void entity_OnEntityAdded(object sender, EntityEventArgs e)
      {
         var ret = from item in CategorizedEntities
                   where item.Key == e.Entity.Schema.Category
                   select item;

         AsyncObservableCollection<EntityViewModel> catItems;

         if ( ret.Count() == 0 )
         {
            catItems = new AsyncObservableCollection<EntityViewModel>();

            CategorizedEntities.Add(new KeyValuePair<string, AsyncObservableCollection<EntityViewModel>>(
               AddedEVM.Entity.Schema.Category, catItems));
         }
         else
         {
            catItems = ret.Single().Value;
         }

         catItems.Add(AddedEVM);
      }

      void Package_OnBuildCompleted(object sender, EntityEventArgs e)
      {
         BuildOutput.Add(string.Format("-------------------- Package {0} built successfully! -------------------- ",
            e.Entity.ToString()));
      }

      void Package_OnBuildStarted(object sender, EntityEventArgs e)
      {
         BuildOutput.Clear();
         BuildOutput.Add(string.Format("-------------------- Build Started ({0}) --------------------",
            e.Entity.ToString()));
      }

      void Package_OnEntityPackaged(object sender, EntityEventArgs e)
      {

      }

      void Package_OnEntityPackaging(object sender, EntityEventArgs e)
      {
         BuildOutput.Add(string.Format("Packaging {0}...", e.Entity.ToString()));
      }

      void Package_OnSetupCompleted(object sender, SetupEventArgs e)
      {
         BuildOutput.Add(string.Format("Executable {0} Generated Successfully!",
            Path.Combine(e.SetupInfo.OutputDirectory, e.SetupInfo.OutputFileName)));
      }

      void Package_OnSetupBegin(object sender, SetupEventArgs e)
      {
         BuildOutput.Add(string.Format("Generating executable {0}...",
            Path.Combine(e.SetupInfo.OutputDirectory, e.SetupInfo.OutputFileName)));
      }

      object buildLock = new Object();
      bool buildInProgress;
      public bool IsBuildInProgress
      {
         get
         {
            lock ( buildLock )
            {
               return buildInProgress;
            }
         }
         set
         {
            lock ( buildLock )
            {
               buildInProgress = value;
            }
         }
      }
      public void Build()
      {
         try
         {
            IsBuildInProgress = true;
            SetupPackager sp = new SetupPackager();
            sp.OutputDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            sp.OutputFileName = @"Setup.exe";
            Package.Build(sp);
         }
         catch ( Exception e )
         {
            BuildOutput.Add(string.Format("Build failed with exception '{0}'", e.ToString()));
         }
         finally
         {
            IsBuildInProgress = false;
         }

      }

      public AsyncObservableCollection<KeyValuePair<string, AsyncObservableCollection<EntityViewModel>>> CategorizedEntities
      {
         get;
         private set;
      }

      public AsyncObservableCollection<string> BuildOutput { get; private set; }
   }
}
