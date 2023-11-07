  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Collections.Generic;
using Microsoft.Management.Infrastructure;
using OmenMon.Library;

namespace OmenMon.Hardware.Platform {

    // Implements the Windows Management Instrumentation functionality
    // by means of the Common Information Model interface to query hardware info
    public class WmiInfo : IDisposable {

        // WMI routine identifiers, constant
        protected const string WMI_INFO_NAMESPACE = "root\\cimv2";
        protected const string WMI_INFO_CLASS_BASEBOARD = "Win32_BaseBoard";
        protected const string WMI_INFO_PROPERTY_TAG = "Tag";
        protected const string WMI_INFO_TAG_BASEBOARD = "Base Board";

        // State flag
        public bool IsInitialized { get; protected set; }

        // Stores the session
        private CimSession session;

#region Initialization & Disposal
        // Sets up the CIM session for subsequent WMI calls
        public WmiInfo() {
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

#region Retrieval
        // Retrieves an instance of an arbitrary class in a namespace given some criteria
        public CimInstance GetInstance(
            string className,
            Dictionary<string, object> args,
            string scope = WMI_INFO_NAMESPACE) {

            // Create a new instance from a class
            CimInstance instance = new CimInstance(className, scope);

            // Add search criteria
            foreach(string key in args.Keys)
                instance.CimInstanceProperties.Add(CimProperty.Create(key, args[key], CimFlags.Key));

            // Retrieve and return the instance
            return this.session.GetInstance(scope, instance);

        }

        // Retrieves an instance of an arbitrary class in a namespace given its tag
        public CimInstance GetInstance(
            string className,
            string tag,
            string scope = WMI_INFO_NAMESPACE) {

            return GetInstance(className,
                new Dictionary<string, object>() {
                    [WMI_INFO_PROPERTY_TAG] = tag },
                scope);

        }

        // Retrieves all properties from an instance into a dictionary given an instance
        public Dictionary<string, string> GetProperties(CimInstance instance) {

            Dictionary<string, string> properties =
                new Dictionary<string, string>();

            foreach(CimProperty prop in instance.CimInstanceProperties)
                properties[prop.Name] = prop.Value == null ? "" : prop.Value.ToString();

            return properties;

        }

        // Retrieves all properties from an instance into a dictionary given instance data
        public Dictionary<string, string> GetProperties(
            string className,
            string tag,
            string scope = WMI_INFO_NAMESPACE) {

            using(CimInstance instance = GetInstance(className, tag, scope))
                return GetProperties(instance);

        }

        // Retrieves baseboard information
        public Dictionary<string, string> GetBaseBoard() {

            return GetProperties(
                WMI_INFO_CLASS_BASEBOARD,
                WMI_INFO_TAG_BASEBOARD,
                WMI_INFO_NAMESPACE);

        }
#endregion

    }

}
