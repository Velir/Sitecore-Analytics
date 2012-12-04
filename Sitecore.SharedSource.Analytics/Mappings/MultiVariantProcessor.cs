using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;

namespace Sitecore.SharedSource.Analytics.Mappings
{
	public class MultiVariantProcessor
	{
		/// <summary>
		/// Returns a template item from one of the mappings
		/// </summary>
		/// <param name="templateId"></param>
		/// <param name="database"></param>
		/// <returns></returns>
		public TemplateItem GetTemplate(string templateId, Database database)
		{
			if(string.IsNullOrEmpty(templateId))
			{
				return null;
			}

			List<IMultiVariantMapping> mappings = MultiVariantMappings;
			if(mappings == null || mappings.Count == 0)
			{
				return null;
			}

			foreach (IMultiVariantMapping mapping in mappings)
			{
				if(mapping == null || !mapping.TemplateIds.Contains(templateId))
				{
					continue;
				}

				TemplateItem templateItem = database.GetItem(mapping.VariantTemplateId);
				if(templateItem == null)
				{
					continue;
				}

				return templateItem;
			}

			return null;
		}

		/// <summary>
		/// Verified that the passed template is a variant template within the mappings
		/// </summary>
		/// <param name="templateId"></param>
		/// <returns></returns>
		public bool IsVariantTemplate(string templateId)
		{
			List<IMultiVariantMapping> mappings = MultiVariantMappings;
			if (mappings == null || mappings.Count == 0)
			{
				return false;
			}

			foreach (IMultiVariantMapping mapping in mappings)
			{
				if (mapping == null || mapping.VariantTemplateId != templateId)
				{
					continue;
				}

				return true;
			}

			return false;
		}

		/// <summary>
		/// Returns a mapping object
		/// </summary>
		/// <param name="templateId"></param>
		/// <param name="database"></param>
		/// <returns></returns>
		public IMultiVariantMapping GetMapping(string templateId, Database database)
		{
			if (string.IsNullOrEmpty(templateId))
			{
				return null;
			}

			List<IMultiVariantMapping> mappings = MultiVariantMappings;
			if (mappings == null || mappings.Count == 0)
			{
				return null;
			}

			foreach (IMultiVariantMapping mapping in mappings)
			{
				if (mapping == null || !mapping.TemplateIds.Contains(templateId))
				{
					continue;
				}

				return mapping;
			}

			return null;
		}

		/// <summary>
		/// Returns the Gutter Items
		/// </summary>
		private List<IMultiVariantMapping> MultiVariantMappings
		{
			get
			{
				if (HttpContext.Current.Cache["Analytics.VariantMappingItems"] != null)
				{
					return (List<IMultiVariantMapping>)HttpContext.Current.Cache["Analytics.VariantMappingItems"];
				}

				XmlNode dmsNode = Factory.GetConfigNode("analytics");
				if (dmsNode == null || dmsNode.ChildNodes.Count == 0)
				{
					return null;
				}

				List<IMultiVariantMapping> multiVariantMappings = new List<IMultiVariantMapping>();
				foreach (XmlNode node in dmsNode.ChildNodes)
				{
					IMultiVariantMapping multiVariantMapping = GetItem_FromXmlNode(node);
					if (multiVariantMapping == null)
					{
						continue;
					}

					multiVariantMappings.Add(multiVariantMapping);
				}

				HttpContext.Current.Cache["Analytics.VariantMappingItems"] = multiVariantMappings;
				return multiVariantMappings;
			}
		}

		private IMultiVariantMapping GetItem_FromXmlNode(XmlNode validationNode)
		{
			if (validationNode.Name != "VariantMapping"
				|| validationNode.Attributes["type"] == null 
				|| validationNode.Attributes["template"] == null 
				|| validationNode.Attributes["variantTemplate"] == null)
			{
				return null;
			}

			string variantTemplate = validationNode.Attributes["variantTemplate"].Value;
			if(string.IsNullOrEmpty(variantTemplate))
			{
				return null;
			}

			//check to verify that xml was not malformed
			string fullNameSpace = validationNode.Attributes["type"].Value;
			if (string.IsNullOrEmpty(fullNameSpace))
			{
				return null;
			}

			//verify we can break up the type string into a namespace and assembly name
			string[] split = fullNameSpace.Split(',');
			if (split.Length == 0)
			{
				return null;
			}

			string nameSpace = split[0];
			string assemblyName = split[1];

			IMultiVariantMapping multiVariantMapping = GetItem_FromReflection(nameSpace, assemblyName, validationNode);
			if (multiVariantMapping == null)
			{
				return null;
			}

			return multiVariantMapping;
		}

		/// <summary>
		/// Uses reflection to instantiate the IFieldGutter class
		/// </summary>
		/// <param name="nameSpace"></param>
		/// <param name="assemblyName"></param>
		/// <param name="validationNode"></param>
		/// <returns></returns>
		protected IMultiVariantMapping GetItem_FromReflection(string nameSpace, string assemblyName, XmlNode validationNode)
		{
			// load the assemly
			Assembly assembly = GetAssembly(assemblyName);

			// Walk through each type in the assembly looking for our class
			Type type = assembly.GetType(nameSpace);
			if (type == null || !type.IsClass)
			{
				return null;
			}

			object[] parameters = new object[1];
			parameters[0] = validationNode;

			//cast to class
			IMultiVariantMapping multiVariantMapping = (IMultiVariantMapping)Activator.CreateInstance(type, parameters);
			if (multiVariantMapping == null)
			{
				return null;
			}

			return multiVariantMapping;
		}

		private Assembly GetAssembly(string assemblyName)
		{
			//try and find it in the currently loaded assemblies
			AppDomain appDomain = AppDomain.CurrentDomain;
			foreach (Assembly assembly in appDomain.GetAssemblies())
			{
				if (assembly.FullName == assemblyName)
					return assembly;
			}

			//load assembly
			return appDomain.Load(assemblyName);
		}
	}
}
