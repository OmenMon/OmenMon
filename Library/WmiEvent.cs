  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Collections.Generic;
using Microsoft.Management.Infrastructure;
using OmenMon.Library;

namespace OmenMon.Library {

    // Implements the Windows Management Instrumentation Event Filter functionality
    // by means of the Common Information Model interface (the newer of the two APIs)
    public class WmiEvent : IDisposable {

        // WMI routine identifiers, constant
        protected const string WMI_EVENT_NAMESPACE = "root\\subscription";
        protected const string WMI_EVENT_CLASS_BINDING = "__FilterToConsumerBinding";
        protected const string WMI_EVENT_CLASS_CONSUMER_CMD = "CommandLineEventConsumer";
        protected const string WMI_EVENT_CLASS_FILTER = "__EventFilter";
        protected const string WMI_EVENT_PROPERTY_BINDING_CONSUMER = "Consumer";
        protected const string WMI_EVENT_PROPERTY_BINDING_FILTER = "Filter";
        protected const string WMI_EVENT_PROPERTY_NAME = "Name";

        // Type of binding reference
        public enum BindingLookup {
            ByConsumer,  // Look up by event consumer name
            ByFilter     // Look up by event filter name
        }

        // State flag
        public bool IsInitialized { get; protected set; }

        // Stores the session
        private CimSession session;

#region Initialization & Disposal
        // Sets up the CIM session for subsequent WMI calls
        public WmiEvent() {
            if(!this.IsInitialized) {
                try {
                    // Establish a new CIM session
                    this.session = CimSession.Create(null);
                    this.IsInitialized = true;
                } catch { }
            }
        }

        // Closes the CIM session and frees up the resources
        public void Close() {
            if(this.IsInitialized) {
                this.IsInitialized = false;
                try {
                    this.session.Dispose();
                } catch { }
            }
        }

        // Dispose() is just a wrapper for Close()
        public void Dispose() {
            Close();
        }
#endregion

#region Creation
        // Creates a binding between an event consumer and an event filter
        public bool CreateBinding(CimInstance consumer, CimInstance filter) {
            CimInstance instance = new CimInstance(this.session.GetClass(WMI_EVENT_NAMESPACE, WMI_EVENT_CLASS_BINDING));
            instance.CimInstanceProperties[WMI_EVENT_PROPERTY_BINDING_CONSUMER].Value = consumer;
            instance.CimInstanceProperties[WMI_EVENT_PROPERTY_BINDING_FILTER].Value = filter;
            this.session.CreateInstance(WMI_EVENT_NAMESPACE, instance);
            return false;
        }

        // Creates an event consumer instance
        public CimInstance CreateConsumer(Dictionary<string, object> args) {
            return CreateInstance(WMI_EVENT_CLASS_CONSUMER_CMD, args);
        }

        // Creates an event filter instance
        public CimInstance CreateFilter(Dictionary<string, object> args) {
            return CreateInstance(WMI_EVENT_CLASS_FILTER, args);
        }

        // Creates an arbitrary instance of a class in the WMI event namespace
        public CimInstance CreateInstance(string className, Dictionary<string, object> args) {
            CimInstance instance = new CimInstance(this.session.GetClass(WMI_EVENT_NAMESPACE, className));
            try {
                foreach(string key in args.Keys)
                    instance.CimInstanceProperties[key].Value = args[key];
                return this.session.CreateInstance(WMI_EVENT_NAMESPACE, instance);
            } catch { }
            return null;
        }
#endregion

#region Deletion
        // Deletes a binding between an event consumer and an event filter
        // Note: in this implementation, binding must be deleted first, 
        // before either the consumer or the filter used to look it up are deleted
        public bool DeleteBinding(string name, BindingLookup bindingLookup = BindingLookup.ByFilter) {
            try {

                // Iterate through the matching instances
                int result = 0;
                foreach(CimInstance instance in (IEnumerable<CimInstance>) session.QueryInstances(
                    WMI_EVENT_NAMESPACE, "WQL", "REFERENCES OF " +
                        // Query either by consumer or filter, depending on the parameter
                        (bindingLookup == BindingLookup.ByConsumer ?
                            // Note: Query fails with __EventConsumer directly as that is an abstract class
                            "{" + WMI_EVENT_CLASS_CONSUMER_CMD + "." + WMI_EVENT_PROPERTY_NAME + "='" + name + "'}"
                            : "{" + WMI_EVENT_CLASS_FILTER + "." + WMI_EVENT_PROPERTY_NAME + "='" + name + "'}")
                        + " WHERE ResultClass = " + WMI_EVENT_CLASS_BINDING)) {

                   // Delete the instance
                   result += DeleteInstance(instance) ? 0 : 1;

                // No errors if all the partial results were zero
                return result == 0 ? true : false;

                }
            } catch {
            }
            return false;
        }

        // Deletes an event consumer instance given its name
        public bool DeleteConsumer(string name) {
            return DeleteInstance(WMI_EVENT_CLASS_CONSUMER_CMD, name);
        }

        // Deletes an event filter instance given its name
        public bool DeleteFilter(string name) {
            return DeleteInstance(WMI_EVENT_CLASS_FILTER, name);
        }

        // Deletes an instance of an arbitrary class in the WMI event namespace given its name
        public bool DeleteInstance(string className, string name) {
            return DeleteInstance(className,
                new Dictionary<string, object>() {
                    [WMI_EVENT_PROPERTY_NAME] = name });
        }

        // Deletes an instance of an arbitrary class in the WMI event namespace given some criteria
        public bool DeleteInstance(string className, Dictionary<string, object> args) {
            CimInstance instance = new CimInstance(className, WMI_EVENT_NAMESPACE);
            foreach(string key in args.Keys)
                instance.CimInstanceProperties.Add(CimProperty.Create(key, args[key], CimFlags.Key));
            return DeleteInstance(instance);
        }

        // Deletes an instance of an arbitrary class in the WMI event namespace
        public bool DeleteInstance(CimInstance instance) {
            try {
                this.session.DeleteInstance(WMI_EVENT_NAMESPACE, instance);
                return true;
            } catch { }
            return false;
        }
#endregion

#region Retrieval
        // Retrieves an event consumer instance
        public CimInstance GetConsumer(string name) {
            return GetInstance(WMI_EVENT_CLASS_CONSUMER_CMD, name);
        }

        // Retrieves an event filter instance
        public CimInstance GetFilter(string name) {
            return GetInstance(WMI_EVENT_CLASS_FILTER, name);
        }

        // Retrieves an arbitrary instance of a class in the WMI event namespace
        public CimInstance GetInstance(string className, string name) {
            CimInstance instance = new CimInstance(className, WMI_EVENT_NAMESPACE);
            instance.CimInstanceProperties.Add(CimProperty.Create(WMI_EVENT_PROPERTY_NAME, name, CimFlags.Key));
            return this.session.GetInstance(WMI_EVENT_NAMESPACE, instance);
        }
#endregion

    }

}
