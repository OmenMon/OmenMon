  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using OmenMon.Hardware;
using OmenMon.Hardware.Ec;
using OmenMon.Library;

namespace OmenMon.Hardware.Platform {

    // Defines a common interface for all platform components
    public interface IPlatformComponent {

        // Retrieves the access type
        public PlatformData.AccessType GetAccessType();

        // Retrieves the data size
        public PlatformData.DataSize GetDataSize();

        // Retrieves the link type
        public PlatformData.LinkType GetLinkType();

        // Retrieves the name
        public string GetName();

        // Sets the name
        public void SetName(string name);

    }

    // Defines an interface for interacting with a readable component
    public interface IPlatformReadComponent : IPlatformComponent {

        // Retrieves the current constraint value
        public int GetConstraint();

        // Retrieves the current sensor value
        public int GetValue();

        // Retrieves the value trend
        public PlatformData.ValueTrend GetValueTrend();

        // Sets the constraint value
        public void SetConstraint(int constraint);

        // Updates the sensor value
        public bool Update();

    }

    // Defines an interface for interacting with a writeable component
    public interface IPlatformWriteComponent : IPlatformComponent {

        // Sets the component value
        public void SetValue(int value);

    }

    // Defines an interface for interacting with
    // a component that is both readable and writeable
    public interface IPlatformReadWriteComponent :
        IPlatformReadComponent, IPlatformWriteComponent {

    }

    // Provides common base for all kinds of components
    public abstract class PlatformComponentAbstract : IPlatformReadWriteComponent {

        // Stores the access type
        protected PlatformData.AccessType AccessType;

        // Stores the constraint value
        protected int Constraint;

        // Stores the component name
        protected string Name;

        // Stores the data size
        protected PlatformData.DataSize Size;

        // Stores the linktype
        protected PlatformData.LinkType LinkType;

        // Constructs a component instance
        public PlatformComponentAbstract(
            PlatformData.AccessType access = PlatformData.AccessType.Read) {

            // Set the access type
            this.AccessType = access;
        }

        // Retrieves the access type
        public PlatformData.AccessType GetAccessType() {
            return this.AccessType;
        }

        // Retrieves the constraint value
        public int GetConstraint() {
            return this.Constraint;
        }

        // Retrieves the data size
        public PlatformData.DataSize GetDataSize() {
            return this.Size;
        }

        // Retrieves the link type
        public PlatformData.LinkType GetLinkType() {
            return this.LinkType;
        }

        // Retrieves the component name
        public virtual string GetName() {
            return this.Name;
        }

        // Sets the constraint value
        public void SetConstraint(int constraint) {
            this.Constraint = constraint;
        }

        // Sets the component name
        public virtual void SetName(string name) {
            this.Name = name;
        }

        // Reading operations

        // Stores the last and previous values
        protected int LastValue;
        protected int PreviousValue;

        // Checks whether a read or write operation is valid for the component
        protected virtual void AssertHasAccess(PlatformData.AccessType access) {
            if(!this.AccessType.HasFlag(access))
                throw new InvalidOperationException();
        }

        // Retrieves the current component value
        public virtual int GetValue() {
            // Ensure the component can be read from
            AssertHasAccess(PlatformData.AccessType.Read);
            return this.LastValue;
        }

        // Retrieves the component value trend
        public virtual PlatformData.ValueTrend GetValueTrend() {
            // Ensure the component can be read from
            AssertHasAccess(PlatformData.AccessType.Read);
            return LastValue != PreviousValue ?
                LastValue > PreviousValue ?
                    PlatformData.ValueTrend.Ascending
                    : PlatformData.ValueTrend.Descending
                    : PlatformData.ValueTrend.Unchanged;
        }

        // Updates the component value
        public virtual bool Update() {
            // Ensure the component can be read from
            AssertHasAccess(PlatformData.AccessType.Read);

            try {

                // Read the value
                int value = Read();

                // Hold off on one additional time
                // for values that might be intermittently zeroed
                if(value == 0 && this.PreviousValue != 0) {
                    this.PreviousValue = 0;
                    return false;
                }

                // Only update if the reading
                // is not obviously incorrect
                if(value <= this.Constraint) {

                    // Update the previous value
                    this.PreviousValue = this.LastValue;
                    this.LastValue = value;

                    // Update succeeded
                    return true;

                }

            } catch { }

                // Update failed
                return false;

        }

        // Writing operations

        // Sets the current component value
        public virtual void SetValue(int value) {
            // Ensure the component can be written to
            AssertHasAccess(PlatformData.AccessType.Write);

            // Set the value
            Write(value);

            // If the component can also be read from, update the value
            if(this.AccessType.HasFlag(PlatformData.AccessType.Read))
                Update();

        }

        // Implements component value retrieval
        protected abstract int Read();

        // Required due to inheritance, and implemented here so that
        // it does not have to be repeated in every derived class,
        // even if the class does not implement the write interface
        protected virtual void Write(int value) {
            return;
        }

    }

    // Implements an Embedded Controller component
    public class EcComponent : PlatformComponentAbstract, IPlatformReadWriteComponent {

        // Stores the Embedded Controller register associated with the sensor
        protected byte Register;

        // Constructs an Embedded Controller readable component instance
        public EcComponent(
            byte register,
            PlatformData.AccessType access = PlatformData.AccessType.Read,
            PlatformData.DataSize size = PlatformData.DataSize.Byte,
            int constraint = int.MaxValue) {

            this.AccessType = access;
            this.Constraint = constraint;
            this.LinkType = PlatformData.LinkType.EmbeddedController;
            this.Register = register;
            this.Size = size;

            SetName();

        }

        // Defines a constructor for read-only non byte-sized data
        public EcComponent(byte register, PlatformData.DataSize size)
            : this(register, PlatformData.AccessType.Read, size) {

        }

        // Defines a constructor for read-only constrained-value data
        public EcComponent(byte register, int constraint)
            : this(register, PlatformData.AccessType.Read, PlatformData.DataSize.Byte, constraint) {

        }

        // Sets the name of an Embedded Controller sensor
        public override void SetName(string name = "") {
            if(name != "")
                this.Name = name;
            else try {
                // Use the DSDT table entry as the name
                this.Name = Enum.GetName(typeof(EmbeddedControllerData.Register), this.Register);
            } catch {
                // Set a generic name based on the register number
                this.Name = "R" + Conv.GetString(this.Register, 3, 10);
            }
        }

        // Reads a value from the Embedded Controller
        protected override int Read() {
            if(this.Size == PlatformData.DataSize.Byte)
                return Hw.EcGetByte(this.Register);
            else
                return Hw.EcGetWord(this.Register);
        }

        // Writes a value to the Embedded Controller
        protected override void Write(int value) {
            if(this.Size == PlatformData.DataSize.Byte)
                Hw.EcSetByte(this.Register, (byte) value);
            else
                Hw.EcSetWord(this.Register, (ushort) value);
        }

    }

    // Implements a BIOS Temperature component
    public class WmiBiosTemperatureComponent : PlatformComponentAbstract, IPlatformReadComponent {

        // Constructs an BIOS Temperature readable component instance
        public WmiBiosTemperatureComponent(int constraint = int.MaxValue) {

            this.AccessType = PlatformData.AccessType.Read;
            this.Constraint = constraint;
            this.LinkType = PlatformData.LinkType.WmiBios;
            this.Size = PlatformData.DataSize.Byte;

            SetName();

        }

        // Sets the name of an Embedded Controller sensor
        public override void SetName(string name = "") {
            if(name != "")
                this.Name = name;
            else
                this.Name = "BIOS";
        }

        // Reads a temperature value from the BIOS
        protected override int Read() {
            return Hw.BiosGet(Hw.Bios.GetTemperature);
        }

    }

}
