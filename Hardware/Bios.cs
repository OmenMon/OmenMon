  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using Microsoft.Management.Infrastructure;
using OmenMon.Library;

namespace OmenMon.Hardware.Bios {

#region Interface
    // Defines an interface for interacting with the BIOS
    public interface IBios : IDisposable {

        public bool IsInitialized { get; }

        public void Initialize();
        public void Close();

        // Read and write
        public int Send(
            BiosData.Cmd command,
            uint commandType,
            byte[] inData,
            byte outDataSize,
            out byte[] outData);

        // Write only
        public int Send(
            BiosData.Cmd command,
            uint commandType,
            byte[] inData);

    }
#endregion

    // Provides for BIOS call error handling
    public class BiosException : Exception { 

        public BiosException(string message) : base(message) { }

    }

    // Implements the functionality for making BIOS calls via CIM (WMI)
    // Builds up on the BIOS data values and structures defined earlier
    public class Bios : BiosData, IBios {

#region Constants & Variables
        public bool IsInitialized { get; protected set; }

        private CimSession session;
        private CimInstance biosData, biosMethods;
#endregion

#region Initialization & Disposal
        // The following three statements ensure the class can be instantiated only once
        private static readonly Bios instance = new Bios();

        protected Bios() { }

        public static Bios Instance {
            get { return instance; }
        }

        // Sets up the CIM session and objects for subsequent WMI calls to the BIOS
        public void Initialize() {
            if(!this.IsInitialized) {
                try {

                    // Establish a new CIM session
                    this.session = CimSession.Create(null);

                    // Set up the BIOS data structure and pre-populate it with the shared secret
                    this.biosData = new CimInstance(this.session.GetClass(BIOS_NAMESPACE, BIOS_DATA));
                    this.biosData.CimInstanceProperties["Sign"].Value = Sign;

                    // Retrieve the BIOS methods instance
                    this.biosMethods = new CimInstance(BIOS_METHOD_CLASS, BIOS_NAMESPACE);
                    this.biosMethods.CimInstanceProperties.Add(CimProperty.Create("InstanceName", BIOS_METHOD_INSTANCE, CimFlags.Key));
                    this.biosMethods = session.GetInstance(BIOS_NAMESPACE, this.biosMethods);

                    // Alternatively, using System.Linq:
                    //this.biosMethods = this.session.QueryInstances("root\\wmi", "WQL", "SELECT * FROM hpqBIntM").SingleOrDefault();
		    
                    this.IsInitialized = true;
                 } catch {
                 }
            }
        }

        // Closes the CIM session and frees up the resources allocated to the CIM objects
        public void Close() {
            if(this.IsInitialized) {
                this.IsInitialized = false;
                try {
                    this.biosData.Dispose();
                    this.biosMethods.Dispose();
                    this.session.Dispose();
                } catch {
                }
            }
        }

        // Dispose() is just a wrapper for Close()
        public void Dispose() {
            Close();
        }
#endregion

        // Sends a command to the BIOS
        public int Send(
            BiosData.Cmd command,
            uint commandType,
            byte[] inData,
            byte outDataSize, // One of 0, 4, 128, 1024, or 4096 only
            out byte[] outData) {

            // Initialize the output variable
            outData = new byte[outDataSize];

            try {
                using(CimInstance input = new CimInstance(biosData)) {

                    // Define the input arguments for the request
                    input.CimInstanceProperties["Command"].Value = command;
                    input.CimInstanceProperties["CommandType"].Value = commandType;

                    if(inData == null) {

                        // Allow for a call with no data payload
                        input.CimInstanceProperties["Size"].Value = 0;

                    } else {

                        input.CimInstanceProperties[BIOS_DATA_FIELD].Value = inData;
                        input.CimInstanceProperties["Size"].Value = inData.Length;

                    }

                    // Prepare the method parameters
                    CimMethodParametersCollection methodParams = new();
                    methodParams.Add(CimMethodParameter.Create("InData", input, CimType.Instance, CimFlags.In));

                    // Call the pertinent method depending on the data size
                    CimMethodResult result = this.session.InvokeMethod(
                        this.biosMethods, BIOS_METHOD + Convert.ToString(outDataSize), methodParams);

                    // Retrieve the resulting data
                    using(CimInstance resultData = result.OutParameters["OutData"].Value as CimInstance) {

                        // Clean up
                        input.Dispose();
                        methodParams.Dispose();
                        result.Dispose();

                        // Populate the output data variable
                        if(outDataSize != 0)
                            outData = resultData.CimInstanceProperties["Data"].Value as byte[];

                        // Return the status code
                        return Convert.ToInt32(resultData.CimInstanceProperties[BIOS_RETURN_CODE_FIELD].Value);

                    }

                }

            } catch {

                // Return negative status code
                // for client-side exceptions
                return -1;
            }

        }

        // Wrapper for sending a BIOS command in case there is nothing to be sent as input
        public int Send(
            BiosData.Cmd command,
            uint commandType,
            byte[] inData) {

            byte[] outData = new byte[0];
            return Send(command, commandType, inData, 0, out outData);

        }

        // Evaluates the return status following a Send() call
        public void Check(int code, bool force = false) {

            // Optionally skip to make the application
            // usable with not fully-compatible models
            if(!force && !Config.BiosErrorReporting)
                return;

            // Check the return status
            switch(code) {
                case 0:
                    break;

                case -1: // Client-side exception
                    throw new BiosException(Config.GetError("ErrBiosCall|ErrBiosSend"));

                case 3: // Command not available
                    throw new BiosException(Config.GetError("ErrBiosCall|ErrBiosSendCommand"));

                case 5: // Insufficient input or output buffer size
                    throw new BiosException(Config.GetError("ErrBiosCall|ErrBiosSendSize"));

                // Note: Codes 1, 4, 6 and 46 were also observed
                // but their exact meaning is not understood

                default: // Unknown error
                    throw new BiosException(String.Format(Config.GetError("ErrBiosCall|ErrBiosSendUnknown"), code));

            }

        }

    }

}
