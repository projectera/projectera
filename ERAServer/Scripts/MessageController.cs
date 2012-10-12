using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERAServer.Scripts.Packages;
using ERAUtils.Logger;
using ERAServer.Data;
using System.Threading.Tasks;

namespace ERAServer.Scripts
{
    internal class MessageController
    {
        public Dictionary<String, IPackage> Packages;
        public Dictionary<String, List<String>> Overrides;
        private Interactable _interactable;

        /// <summary>
        /// The list of functions publicly callable
        /// </summary>
        public List<String> PublicFunctions { get; private set; }

        /// <summary>
        /// Creates a new MessageController.
        /// Used to manage message passing between Packages
        /// </summary>
        public MessageController()
        {
            Packages = new Dictionary<String, IPackage>();
            Overrides = new Dictionary<String, List<String>>();
            PublicFunctions = new List<String>();
        }

        /// <summary>
        /// Initializes Packages for this instance.
        /// </summary>
        internal void Setup(Interactable interactable)
        {
            _interactable = interactable;
            foreach (var p in Packages)
            {
                p.Value.Setup(interactable);
            }
        }

        /// <summary>
        /// Routes a message to the correct Package
        /// </summary>
        /// <param name="package">The destination package</param>
        /// <param name="function">The function to call</param>
        /// <param name="arguments">The arguments</param>
        internal Task<object> Call(String package, String function, Object[] arguments)
        {
            return Task.Factory.StartNew<object>(() =>
            {
                if (Overrides.ContainsKey(package + "." + function))
                {
                    function = package + "_" + function;
                    package = Overrides[package + "." + function][0];
                }

                try
                {
                    return Packages[package].ProcessMessage(function, arguments);
                }
                catch (Exception)
                {
                    Logger.Error("Calling " + package + "." + function + " failed!");
                    return null;
                }
            });
        }

        internal object CallBlocking(String package, String function, Object[] arguments)
        {
            var t = Call(package, function, arguments);
            t.Wait();
            return t.Result;
        }

        /// <summary>
        /// Adds a package
        /// </summary>
        /// <param name="package">The package to add</param>
        internal void AddPackage(IPackage package)
        {
            Packages.Add(package.Package, package);

            foreach (String function in package.Overrides)
                AddOverride(function, package.Package);

            foreach (String function in package.PublicFunctions)
                PublicFunctions.Add(package.Package + "." + function);

            package.Setup(_interactable);

            package.Initialize();
        }

        /// <summary>
        /// Removes the package and disposes it
        /// </summary>
        /// <param name="package">The package to remove</param>
        internal void RemovePackage(IPackage package)
        {
            foreach(String function in package.PublicFunctions)
                PublicFunctions.Remove(package.Package + "." + function);

            foreach (String function in package.Overrides)
                RemoveOverride(function, package.Package);

            Packages.Remove(package.Package);

            package.Dispose();
        }


        /// <summary>
        /// Adds a new override to a function
        /// Overrides added in this way need to be either removed manually from the overrides list or added to the Overrides property
        /// </summary>
        /// <param name="function">The function that needs to be overridden</param>
        /// <param name="destination">The new function destination</param>
        internal void AddOverride(String function, String destination)
        {
            List<String> targets;
            if (!Overrides.ContainsKey(function))
            {
                targets = new List<string>();
                Overrides.Add(function, targets);
            }
            else
            {
                targets = Overrides[function];
            }

            targets.Insert(0, destination);
        }

        /// <summary>
        /// Removes an override added by the AddOverride function
        /// </summary>
        /// <param name="function">The function to remove the override from</param>
        /// <param name="destination">The destination that needs to be removed</param>
        internal void RemoveOverride(String function, String destination)
        {
            if (!Overrides.ContainsKey(function))
                return;
            List<String> targets = Overrides[function];
            targets.Remove(destination);
            if (targets.Count == 0)
                Overrides.Remove(function);
        }
    }
}
