// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Matthew Ward" email="mrward@users.sourceforge.net"/>
//     <version>$Revision: 5716 $</version>
// </file>

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using ICSharpCode.PythonBinding;

namespace PythonBinding.Tests.Utils
{
	public class MockComponentCreator : IComponentCreator
	{
		List <CreatedComponent> createdComponents = new List<CreatedComponent>();
		List <CreatedInstance> createdInstances = new List<CreatedInstance>();
		List <AddedComponent> addedComponents = new List<AddedComponent>();
		List<string> typeNames = new List<string>();
		PropertyDescriptor propertyDescriptor;
		EventDescriptor eventDescriptor;
		IComponent rootComponent;
		bool getResourceReaderCalled;
		CultureInfo cultureInfoPassedToGetResourceReader;
		IResourceWriter resourceWriter;
		bool getResourceWriterCalled;
		CultureInfo cultureInfoPassedToGetResourceWriter;
		IResourceReader resourceReader;
		Dictionary<string, Type> types = new Dictionary<string, Type>();
		
		public MockComponentCreator()
		{
		}
		
		public IComponent RootComponent {
			get { return rootComponent; }
		}
		
		public IComponent CreateComponent(Type componentClass, string name)
		{						
			object instance = componentClass.Assembly.CreateInstance(componentClass.FullName);
	
			if (rootComponent == null) {
				rootComponent = instance as IComponent;
			}
			
			CreatedComponent c = new CreatedComponent(componentClass.FullName, name, (IComponent)instance);
			createdComponents.Add(c);
			
			return (IComponent)instance;
		}
		
		public void Add(IComponent component, string name)
		{
			if (component == null) {
				throw new ArgumentNullException("component");
			}
			
			AddedComponent addedComponent = new AddedComponent(component, name);
			addedComponents.Add(addedComponent);
		}
		
		public IComponent GetComponent(string name)
		{
			foreach (AddedComponent addedComponent in addedComponents) {
				if (addedComponent.Name == name) {
					return addedComponent.Component;
				}
			}
			foreach (CreatedComponent createdComponent in createdComponents) {
				if (!String.IsNullOrEmpty(createdComponent.Name)) {
					if (createdComponent.Name == name) {
						return createdComponent.Component;
					}
				}
			}
			return null;
		}

		public void AddInstance(object obj, string name)
		{
			CreatedInstance createdInstance = new CreatedInstance(obj.GetType(), new object[0], name, false);
			createdInstance.Object = obj;
			createdInstances.Add(createdInstance);
		}
		
		public object CreateInstance(Type type, ICollection arguments, string name, bool addToContainer)
		{
			CreatedInstance createdInstance = new CreatedInstance(type, arguments, name, addToContainer);
			createdInstances.Add(createdInstance);
		
			object[] argumentsArray = new object[arguments.Count];
			arguments.CopyTo(argumentsArray, 0);
			
			object o = null;
			DesignerSerializationManager designerSerializationManager = new DesignerSerializationManager();
			using (designerSerializationManager.CreateSession()) {
				IDesignerSerializationManager manager = designerSerializationManager as IDesignerSerializationManager;
				o = manager.CreateInstance(type, arguments, name, addToContainer);
			}
			createdInstance.Object = o;
			return o;
		}
		
		/// <summary>
		/// Adds a type that can be returned from the GetType method.
		/// </summary>
		public void AddType(string name, Type type)
		{
			types.Add(name, type);
		}
		
		public Type GetType(string typeName)
		{
			typeNames.Add(typeName);
			
			// Lookup type in System.Windows.Forms assembly.
			Type type = typeof(Form).Assembly.GetType(typeName);
			if (type == null) {
				// Lookup type in System.Drawing assembly.
				type = typeof(Size).Assembly.GetType(typeName);
			}
			if (type == null) {
				type = typeof(String).Assembly.GetType(typeName);
			}
			if (type == null) {
				type = typeof(Component).Assembly.GetType(typeName);
			}
			if (type == null) {
				type = typeof(DataTable).Assembly.GetType(typeName);
			}
			if (type == null) {
				types.TryGetValue(typeName, out type);
			}
			
			return type;
		}
		
		public PropertyDescriptor GetEventProperty(EventDescriptor e)
		{
			this.eventDescriptor = e;
			return propertyDescriptor;
		}
		
		public EventDescriptor EventDescriptorPassedToGetEventProperty {
			get { return eventDescriptor; }
		}
		
		/// <summary>
		/// Sets the property descriptor to return from the GetEventProperty method.
		/// </summary>
		public void SetEventPropertyDescriptor(PropertyDescriptor propertyDescriptor)
		{
			this.propertyDescriptor = propertyDescriptor;
		}
		
		public List<CreatedComponent> CreatedComponents {
			get { return createdComponents; }
		}

		public List<AddedComponent> AddedComponents {
			get { return addedComponents; }
		}
		
		public List<CreatedInstance> CreatedInstances {
			get { return createdInstances; }
		}

		public List<string> TypeNames {
			get { return typeNames; }
		}

		public string LastTypeNameResolved {
			get { return TypeNames[TypeNames.Count - 1]; }
		}
		
		public CreatedInstance GetCreatedInstance(Type type)
		{
			foreach (CreatedInstance instance in createdInstances) {
				if (instance.InstanceType == type) {
					return instance;
				}
			}
			return null;
		}
		
		public CreatedInstance GetCreatedInstance(string name)
		{
			foreach (CreatedInstance instance in createdInstances) {
				if (instance.Name == name) {
					return instance;
				}
			}
			return null;
		}
		
		public object GetInstance(string name)
		{
			CreatedInstance instance = GetCreatedInstance(name);
			if (instance != null) {
				return instance.Object;
			}
			return null;
		}

		public void SetResourceReader(IResourceReader reader)
		{
			resourceReader = reader;
		}
		
		public bool GetResourceReaderCalled {
			get { return getResourceReaderCalled; }
		}

		public CultureInfo CultureInfoPassedToGetResourceReader {
			get { return cultureInfoPassedToGetResourceReader; }
		}
		
		public IResourceReader GetResourceReader(CultureInfo info)
		{
			getResourceReaderCalled = true;
			cultureInfoPassedToGetResourceReader = info;
			return resourceReader;
		}
		
		public void SetResourceWriter(IResourceWriter writer)
		{
			resourceWriter = writer;
		}
		
		public IResourceWriter GetResourceWriter(CultureInfo info)
		{
			getResourceWriterCalled = true;
			cultureInfoPassedToGetResourceWriter = info;
			return resourceWriter;
		}		
		
		public bool GetResourceWriterCalled {
			get { return getResourceWriterCalled; }
		}

		public CultureInfo CultureInfoPassedToGetResourceWriter {
			get { return cultureInfoPassedToGetResourceWriter; }
		}		
	}
}
