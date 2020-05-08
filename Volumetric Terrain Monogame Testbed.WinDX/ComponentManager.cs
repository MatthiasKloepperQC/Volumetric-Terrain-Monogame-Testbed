using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SolConsulting.MonoGame.Testbed.VolumetricTerrain
{
    /// <summary>
    /// Manages all registered IGameComponent instances for easy and fast access in separate dictionaries.
    /// </summary>
    class ComponentManager
    {
        #region Fields
        private readonly List<IGameComponent> components;
        private readonly Dictionary<Type, IList> componentsByType;
        #endregion

        #region Constructor
        internal ComponentManager()
        {
            this.components = new List<IGameComponent>();
            this.componentsByType = new Dictionary<Type, IList>();
            this.InterfacesToIgnore = new HashSet<Type>(1)
            {
                typeof(IGameComponent)
            };
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the number of registered components.
        /// </summary>
        internal int ComponentCount => this.components.Count;

        internal HashSet<Type> InterfacesToIgnore { get; set; }
        #endregion

        #region Methods
        internal List<T> GetComponentsByInterface<T>()
        {
            if (!typeof(T).IsInterface)
            {
                throw new ArgumentException("Components are differentiated by the interfaces they implement. Please provide an interface.", nameof(T));
            }

            if (!this.componentsByType.TryGetValue(typeof(T), out IList components))
            {
                components = new List<T>();
                this.componentsByType.Add(typeof(T), components);
            }

            return (List<T>)components;
        }

        /// <summary>
        /// Registers a component with this ComponentManager.
        /// </summary>
        /// <param name="component">The component to register.</param>
        internal void RegisterComponent(IGameComponent component)
        {
            Type listType;

            this.components.Add(component);

            // Get all additional interfaces to link the component into all corresponding type specific lists.
            foreach (Type componentInterface in component.GetType().GetInterfaces())
            {
                listType = typeof(List<>).MakeGenericType(componentInterface);

                // Do not use a type specific list for the IGameComponent interface since all components are forced to implement it.
                if ((!this.InterfacesToIgnore.Contains(componentInterface)) && (!componentInterface.FullName.StartsWith("System.")))
                {
                    if (!this.componentsByType.TryGetValue(componentInterface, out IList typeSpecificComponents))
                    {
                        // Type specific list for this interface does not yet exist.
                        // Create list and add to dictionary.
                        typeSpecificComponents = (IList)Activator.CreateInstance(listType);
                        this.componentsByType.Add(componentInterface, typeSpecificComponents);
                    }
                    // At this point the type specific list definitely exists. Just add the component.
                    _ = typeSpecificComponents.Add(component);

                    // Is the component specific list sortable?
                    if (typeof(IComparable<>).MakeGenericType(componentInterface).IsAssignableFrom(componentInterface))
                    {
                        // Somehow the current interface implements inherits from the generic ICompareable<T> interface with T being the current interface itself.
                        // Since this pattern ensures that the type specific list is sortable, sort it.
                        // Sort the component specific list by getting the correspondig Sort() method and invoking it.
                        MethodInfo mi = listType.GetMethod("Sort", new Type[] { });
                        _ = mi.Invoke(typeSpecificComponents, null);
                    }
                }
            }
        }
        #endregion
    }
}