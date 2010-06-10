using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyInstall.Interfaces;

namespace EasyInstall
{
   public class AttributeNotFoundException : Exception
   {
      public AttributeNotFoundException(string attribute)
         : this(attribute, null)
      {

      }

      public AttributeNotFoundException(string attribute, Exception innerException)
         :base(string.Format("Attribute '{0}' not found", attribute), innerException)
      {

      }
   }

   public class SchemaNotFoundException : Exception
   {
      public SchemaNotFoundException(string entityType)
         : this(entityType, null)
      {

      }

      public SchemaNotFoundException(string entityType, Exception innerException)
         : base(string.Format("Schema for entity type '{0}' not found", entityType), innerException)
      {

      }
   }

   public class DuplicateSchemaException : Exception
   {
      public DuplicateSchemaException(string name)
         : this(name, null)
      {

      }

      public DuplicateSchemaException(string name, Exception innerException)
         : base(string.Format("Duplicate schema '{0}'", name), innerException)
      {

      }
   }

   public class DuplicateSchemaItemException : Exception
   {
      public DuplicateSchemaItemException(string name)
         : this(name, null)
      {

      }

      public DuplicateSchemaItemException(string name, Exception innerException)
         : base(string.Format("Duplicate attribute '{0}' in schema", name), innerException)
      {

      }
   }

   public class EntityNotFoundException : Exception
   {
      public EntityNotFoundException(Guid id)
         : this(id, null)
      {

      }

      public EntityNotFoundException(Guid id, Exception innerException)
         : base(string.Format("Entity with ID '{0}' not found", id), innerException)
      {

      }
   }

   public class EntityTypeNotSupportedException : Exception
   {
      public EntityTypeNotSupportedException(string parentEntityType, string childEntityType)
         : this(parentEntityType, childEntityType, null)
      {

      }

      public EntityTypeNotSupportedException(string parentEntityType, string childEntityType, Exception innerException)
         : base(string.Format("Entity {0} does not support child of type '{1}'", parentEntityType, childEntityType)
                     , innerException)
      {

      }
   }

   public class InvalidSchemaException : Exception
   {
      public InvalidSchemaException()
         : this(null)
      {

      }

      public InvalidSchemaException(Exception innerException)
         : base("Specified schema is null or empty", innerException)
      {

      }
   }

   public class EntityCannotBeDeployedException : Exception
   {
      public EntityCannotBeDeployedException(string entity, string reason)
         : this(entity, reason, null)
      {
      }

      public EntityCannotBeDeployedException(string entity, string reason, Exception innerException)
         : base (string.Format("The deployment of the entity {0} failed due to '{1}'",
                        entity, reason), innerException)
      {
      }

   }

   public class DeploymentCancelledException : Exception
   {
      public IEntity Entity { get; private set; }

      public DeploymentCancelledException(IEntity entity)
         : base("Deployment has been cancelled by user", null)
      {
         this.Entity = entity;
      }
   }
}

