using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyInstall.Interfaces;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Runtime.Serialization;
using EasyInstall.Interfaces.Entities;

namespace EasyInstall
{
   [DataContract]
   public class Package : DeployableEntity, IPackage
   {
      const string DEFAULTPACKAGEEXT = ".EPK";
      const string TOC = "TOC.xml";

      #region EVENTS
      public event EventHandler<EntityEventArgs> OnBuildStarted;
      public event EventHandler<EntityEventArgs> OnEntityPackaging;
      public event EventHandler<EntityEventArgs> OnEntityPackaged;
      public event EventHandler<EntityEventArgs> OnBuildCompleted;

      public event EventHandler<SetupEventArgs> OnSetupBegin;
      public event EventHandler<SetupEventArgs> OnSetupCompleted;

      public event EventHandler<ProgressEventArgs> OnDeployProgress;
      public event EventHandler<ProgressEventArgs> OnUndeployProgress;
      #endregion

      [DataMember]
      string PackageName
      {
         get;
         set;
      }

      [DataMember]
      IList<IPrerequisite> _Prerequisites = new List<IPrerequisite>();

      public Package()
      {
         PackageName = "Data" + DEFAULTPACKAGEEXT;
      }

      public Package(string name)
         : base(EntityTypes.PACKAGEENTITY)
      {
         if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Package Name cannot be null or empty");

         this.Name = name;

         PackageName = name + DEFAULTPACKAGEEXT;

         // Pre-Populate the attributes with default values from the schema
         foreach ( var kvp in Schema.Attributes )
         {
            _attributes[kvp.Key] = kvp.Value.DefaultValue;
         }

         allEntities[ID] = this;
      }

      protected override bool MustHaveParent
      {
         get
         {
            return false;
         }
      }

      MemoryStream DataStream
      {
         get;
         set;
      }

      public override bool CanAcceptChild(IEntity entity)
      {
         // A package can accept any type of entity
         return true;
      }

      public override bool CanAcceptChild(string entityType)
      {
         EntitySchema schema = EntitySchema.GetSchema(entityType);

         return true;
      }

      void RegisterSubEntityDeployevents()
      {
         foreach (var entity in DeployableEntity.allEntities)
         {
            IDeployableEntity de = entity.Value as IDeployableEntity;

            if (de != null)
            {
               de.OnDeploying += SubEntity_OnDeploying;
               de.OnUndeploying += SubEntity_OnUndeploying;
            }
         }
      }

      void UnregisterSubEntityDeployevents()
      {
         foreach (var entity in DeployableEntity.allEntities)
         {
            IDeployableEntity de = entity.Value as IDeployableEntity;

            if (de != null)
            {
               de.OnDeploying -= SubEntity_OnDeploying;
               de.OnUndeploying -= SubEntity_OnUndeploying;
            }
         }
      }

      double progressPercent = 0;
      int deployCount = 0;
      int totalEntities = 0;
      public override void Deploy()
      {
         // Register for events on all Entities
         // to track progress
         RegisterSubEntityDeployevents();
         try
         {
            entitiesForRollback.Clear();
            totalEntities = allEntities.Count();
            base.Deploy();
         }
         catch (DeploymentCancelledException)
         {
            // Start the roll back process
            Rollback();
         }
         finally
         {
            UnregisterSubEntityDeployevents();
            progressPercent = 0;
         }
      }

      public void Rollback()
      {
         foreach (var e in entitiesForRollback)
         {
            IDeployableEntity de = e as IDeployableEntity;

            if (de != null)
            {
               de.Undeploy();
            }
         }
      }

      Stack<IEntity> entitiesForRollback = new Stack<IEntity>();
      void SubEntity_OnDeploying(object sender, EntityEventArgs e)
      {
         ++deployCount;
         progressPercent = (double)deployCount/totalEntities * 100;

         entitiesForRollback.Push(e.Entity);

         if (OnDeployProgress != null)
         {
            ProgressEventArgs pe = new ProgressEventArgs(e.Entity, progressPercent);
            OnDeployProgress(this, pe);

            if (pe.Cancel)
            {
               throw new DeploymentCancelledException(e.Entity);
            }
         }
      }

      void SubEntity_OnUndeploying(object sender, EntityEventArgs e)
      {
         --deployCount;
         progressPercent = (double)deployCount / totalEntities * 100;

         if (OnUndeployProgress != null)
         {
            ProgressEventArgs pe = new ProgressEventArgs(e.Entity, progressPercent);
            OnUndeployProgress(this, pe);
         }
      }

      public override void Undeploy()
      {
         RegisterSubEntityDeployevents();
         try
         {
            base.Undeploy();
         }
         finally
         {
            UnregisterSubEntityDeployevents();
         }
      }

      ZipFile _zf;
      void PrepareToDeploy()
      {
         // TODO; Any proparation for deployment
         // like executing prerequisite custom actions
         ExecuteActions(Prerequisites);
      }

      protected override void OnDeploy()
      {
         try
         {
            if (DataStream == null)
            {
               throw new Exception("Package has not been built");
            }

            DataStream.Seek(0, SeekOrigin.Begin);

            using ( _zf = new ZipFile(DataStream) )
            {
               PrepareToDeploy();

               if ( !CanBeDeployed() )
                  throw new Exception("Package is not deployable!");

               foreach ( var entity in from de in SubEntities
                                       where de is IDeployableEntity
                                       select de as IDeployableEntity )
               {
                  entity.Deploy();
               }
            }
         }
         finally
         {
            _zf = null;
         }
      }

      public override IEntity CreateChild(string entityType, string name)
      {
         // Check if specified entity Type is su pported
         if ( !CanAcceptChild(entityType) )
            throw new EntityTypeNotSupportedException(Schema.EntityType, entityType);

         IEntity entity = EntityFactory.CreateEntity(entityType, name, this, this.ID, null);

         this.AddEntity(entity);

         return entity;
      }

      public override bool CanBeDeployed()
      {
         return (from de in SubEntities
                 where de is IDeployableEntity
                 select de as IDeployableEntity)
            .All(e => e.CanBeDeployed());
      }

      protected override void OnUndeploy()
      {
         foreach ( var entity in from de in SubEntities
                                 where de is IDeployableEntity
                                 select de as IDeployableEntity )
         {
            entity.Undeploy();
         }
      }

      #region PACKAGE METHODS

      public new IEntity FindEntity(Guid id)
      {
         return base.FindEntity(id);
      }
      public IEntity GetEntity(Guid id)
      {
         IEntity entity = FindEntity(id);

         if ( entity == null )
         {
            throw new EntityNotFoundException(id);
         }

         return entity;
      }

      public Stream GetData(Guid Id)
      {
         if ( _zf == null )
         {
            _zf = new ZipFile(PackageName);
         }
         long index = _zf.FindEntry(Id.ToString(), true);
         if ( index != -1 )
         {
            return _zf.GetInputStream(index);
         }
         
         return null;
      }

      void _Build(ZipOutputStream ostream, IEntity entity)
      {
         using ( Stream s = entity.GetContent() )
         {
            if ( s != null )
            {
               if (OnEntityPackaging != null)
                  OnEntityPackaging(this, new EntityEventArgs(entity));

               // as the resulting path is not absolute.
               ZipEntry entry = new ZipEntry(entity.ID.ToString());
               // Setup the entry data as required.

               // Crc and size are handled by the library for seakable streams
               // so no need to do them here.
               entry.Comment = entity.ToString();
               // Could also use the last write time or similar for the file.
               entry.DateTime = DateTime.Now;
               ostream.PutNextEntry(entry);

               s.CopyTo(ostream);

               if (OnEntityPackaged != null)
                  OnEntityPackaged(this, new EntityEventArgs(entity));
            }
         }

         foreach ( var e in entity.SubEntities )
         {
            _Build(ostream, e);
         }
      }

      public void Build(Stream output)
      {

         using (ZipOutputStream zo = new ZipOutputStream(output))
         {
            if (OnBuildStarted != null)
            {
               OnBuildStarted(this, new EntityEventArgs(this));
            }
            byte[] buffer = new byte[4096];
            zo.SetLevel(9); // 0 - store only to 9 - means best compression
            _Build(zo, this);

            // Serialize this package and store it in the 
            // zip stream as TOC.xml
            ZipEntry entry = new ZipEntry("TOC.xml");
            entry.Comment = "Table Of Contents";

            zo.PutNextEntry(entry);

            NetDataContractSerializer sr = new NetDataContractSerializer();
            sr.WriteObject(zo, this);

            zo.Finish();

            if (OnBuildCompleted != null)
            {
               OnBuildCompleted(this, new EntityEventArgs(this));
            }

            DataStream = new MemoryStream();
            output.Seek(0, SeekOrigin.Begin);
            output.CopyTo(DataStream);
         }
      }

      string PackageFileName
      {
         get
         {
            return Path.Combine(Path.GetTempPath(), Name + DEFAULTPACKAGEEXT);
         }
      }

      public void Build(ISetupPackager packager)
      {
         if (null == packager)
         {
            throw new ArgumentNullException("packager");
         }
         string fileName = Path.Combine(packager.SetupInfo.OutputDirectory, "Data" + DEFAULTPACKAGEEXT);
         FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
         
         try
         {
            Build(fs);
         }
         finally
         {
            fs.Close();
         }

         packager.SetupInfo.PackageFile = fileName;
         
         if ( OnSetupBegin != null )
            OnSetupBegin(this, new SetupEventArgs(this, packager.SetupInfo));

         packager.Pack();

         if ( OnSetupCompleted != null )
            OnSetupCompleted(this, new SetupEventArgs(this, packager.SetupInfo));
      }

      public void Save(Stream stream)
      {
         NetDataContractSerializer sr = new NetDataContractSerializer();
         sr.WriteObject(stream, this);
      }

      #endregion

      #region STATIC METHODS

      public static IPackage Create(string name)
      {
         IPackage package = EntityFactory.CreatePackage(name);
         PackageManager.AddPackage(package);

         return package;
      }

      public static IPackage Load(Stream stream)
      {
         if ( null == stream )
            throw new ArgumentNullException("stream");

         using ( ZipFile zf = new ZipFile(stream) )
         {
            long entryIndex = zf.FindEntry(TOC, true);

            if ( entryIndex == -1 )
            {
               throw new Exception("Invalid package. TOC entry not found");
            }

            Stream s = zf.GetInputStream(entryIndex);

            NetDataContractSerializer sr = new NetDataContractSerializer();
            Package package = sr.ReadObject(s) as Package;

            if (package == null)
            {
               throw new Exception("Unsupported package");
            }

            package.DataStream = new MemoryStream();
            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(package.DataStream);

            PackageManager.AddPackage(package);

            return package;
         }
      }

      public static IPackage Load(string packageName)
      {
         if ( !File.Exists(packageName) )
         {
            throw new FileNotFoundException(packageName);
         }

         if (Path.GetExtension(packageName) != DEFAULTPACKAGEEXT)
            throw new Exception("Invalid Package Extension");

         return Load(new FileStream(packageName, FileMode.Open, FileAccess.Read));
      }

      public static IPackage LoadFromTOC(string tocFile)
      {
         if ( !File.Exists(tocFile) )
         {
            throw new FileNotFoundException(tocFile);
         }

         FileStream fs = new FileStream(tocFile, FileMode.Open, FileAccess.ReadWrite);

         NetDataContractSerializer sr = new NetDataContractSerializer();
         IPackage package =  sr.ReadObject(fs) as IPackage;
         
         PackageManager.AddPackage(package);

         return package;
      }

      #endregion

      public IList<IPrerequisite> Prerequisites
      {
         get
         {
            return _Prerequisites;
         }
      }
   }
}