  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OmenMon.External {

    // Windows Task Scheduler API (taskschd.dll) Resources
    // Used for task scheduling and removal of scheduled tasks
    public class TaskSchd {

#region Actions
        // Action types
        public enum ActionType {
            Execute     = 0,  // Run a program
            ComHandler  = 5,  // Run a COM object
            SendEmail   = 6,  // Send an e-mail (deprecated)
            ShowMessage = 7   // Display a message (deprecated)
        }

        // Generic action
        [ComImport, Guid("BAE54997-48B1-4CBE-9965-D6BE263EBEA4"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface IAction {
            string Id { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            ActionType Type { get; }
        }

        // Execute action
        [ComImport, Guid("4C3D624D-FD6B-49A3-B9B7-09CB3CD3F047"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface IExecAction : IAction {
            new string Id { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new ActionType Type { get; }

            string Path { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            string Arguments { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            string WorkingDirectory { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
        }

        // Component Object Model handler action
        [ComImport, Guid("6D2FD252-75C5-4F66-90BA-2A7D8CC3039F"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface IComHandlerAction : IAction {
            new string Id { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new ActionType Type { get; }
            string ClassId { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            string Data { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
        }

        // Named-value pair
        [ComImport, Guid("39038068-2B46-4AFD-8662-7BB6F868D221"), System.Security.SuppressUnmanagedCodeSecurity, DefaultMember("Name")]
        public interface ITaskNamedValuePair {
            string Name { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            string Value { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
        }

        // Named-value collection
        // Used by event trigger and send e-mail action
        [ComImport, Guid("B4EF826B-63C3-46E4-A504-EF69E4F7EA4D"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface ITaskNamedValueCollection {
            int Count { get; }
            ITaskNamedValuePair this[int index] { [return: MarshalAs(UnmanagedType.Interface)] get; }
            [return: MarshalAs(UnmanagedType.Interface)]
            IEnumerator GetEnumerator();
            [return: MarshalAs(UnmanagedType.Interface)]
            ITaskNamedValuePair Create([In, MarshalAs(UnmanagedType.BStr)] string Name, [In, MarshalAs(UnmanagedType.BStr)] string Value);
            void Remove([In] int index);
            void Clear();
        }

        // Send e-mail action
        [ComImport, Guid("10F62C64-7E16-4314-A0C2-0C3683F99D40"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface IEmailAction : IAction {
            new string Id { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new ActionType Type { get; }

            string Server { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            string Subject { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            string To { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            string Cc { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            string Bcc { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            string ReplyTo { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            string From { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            ITaskNamedValueCollection HeaderFields { [return: MarshalAs(UnmanagedType.Interface)] get; [param: In, MarshalAs(UnmanagedType.Interface)] set; }
            string Body { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            object[] Attachments { [return: MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)] get; [param: In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)] set; }
        }

        // Show message action
        [ComImport, Guid("505E9E68-AF89-46B8-A30F-56162A83D537"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface IShowMessageAction : IAction {
            new string Id { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new ActionType Type { get; }

            string Title { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            string MessageBody { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
        }

        // Action collection
        [ComImport, Guid("02820E19-7B98-4ED2-B2E8-FDCCCEFF619B"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface IActionCollection {
            int Count { get; }
            IAction this[int index] { [return: MarshalAs(UnmanagedType.Interface)] get; }
            [return: MarshalAs(UnmanagedType.Interface)]
            IEnumerator GetEnumerator();
            string XmlText { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            [return: MarshalAs(UnmanagedType.Interface)]
            IAction Create([In] ActionType Type);
            void Remove([In, MarshalAs(UnmanagedType.Struct)] object index);
            void Clear();
            string Context { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
        }
#endregion

#region Triggers
        // Trigger types
        public enum TriggerType {
            Event              =  0,  // On an event
            Time               =  1,  // Specific time of day (1.2, the default)
            Daily              =  2,  // Every day
            Weekly             =  3,  // Every week
            Monthly            =  4,  // Every month
            MonthlyDayOfWeek   =  5,  // Every month, on the same day of the week
            Idle               =  6,  // When computer is idle
            Registration       =  7,  // On registration (1.2)
            Boot               =  8,  // When Windows starts
            Logon              =  9,  // When user logs in
            SessionStateChange = 11,  // When a user session changes state (1.2)
            Custom             = 12   // Custom trigger (1.3)
        }

        // Repetition pattern
        [ComImport, Guid("7FB9ACF1-26BE-400E-85B5-294B9C75DFD6"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface IRepetitionPattern {
            string Interval { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            string Duration { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            bool StopAtDurationEnd { get; [param: In] set; }
        }

        // Generic trigger
        [ComImport, Guid("09941815-EA89-4B5B-89E0-2A773801FAC3"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface ITrigger {
            TriggerType Type { get; }
            string Id { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            IRepetitionPattern Repetition { [return: MarshalAs(UnmanagedType.Interface)] get; [param: In, MarshalAs(UnmanagedType.Interface)] set; }
            string ExecutionTimeLimit { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            string StartBoundary { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            string EndBoundary { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            bool Enabled { get; [param: In] set; }
        }

        // Event trigger
        // Named-value collection also used by send e-mail trigger, see definition there
        [ComImport, Guid("D45B0167-9653-4EEF-B94F-0732CA7AF251"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface IEventTrigger : ITrigger {
            new TriggerType Type { get; }
            new string Id { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new IRepetitionPattern Repetition { [return: MarshalAs(UnmanagedType.Interface)] get; [param: In, MarshalAs(UnmanagedType.Interface)] set; }
            new string ExecutionTimeLimit { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new string StartBoundary { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new string EndBoundary { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new bool Enabled { get; [param: In] set; }

            string Subscription { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            string Delay { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            ITaskNamedValueCollection ValueQueries { [return: MarshalAs(UnmanagedType.Interface)] get; [param: In, MarshalAs(UnmanagedType.Interface)] set; }
        }

        // Time trigger
        [ComImport, Guid("B45747E0-EBA7-4276-9F29-85C5BB300006"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface ITimeTrigger : ITrigger {
            new TriggerType Type { get; }
            new string Id { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new IRepetitionPattern Repetition { [return: MarshalAs(UnmanagedType.Interface)] get; [param: In, MarshalAs(UnmanagedType.Interface)] set; }
            new string ExecutionTimeLimit { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new string StartBoundary { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new string EndBoundary { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new bool Enabled { get; [param: In] set; }

            string RandomDelay { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
        }

        // Daily trigger
        [ComImport, Guid("126C5CD8-B288-41D5-8DBF-E491446ADC5C"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface IDailyTrigger : ITrigger {
            new TriggerType Type { get; }
            new string Id { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new IRepetitionPattern Repetition { [return: MarshalAs(UnmanagedType.Interface)] get; [param: In, MarshalAs(UnmanagedType.Interface)] set; }
            new string ExecutionTimeLimit { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new string StartBoundary { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new string EndBoundary { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new bool Enabled { get; [param: In] set; }

            short DaysInterval { get; [param: In] set; }
            string RandomDelay { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
        }

        // Day of the week values
        [Flags]
        public enum DaysOfWeek : short {
            Sunday    = 0x01,
            Monday    = 0x02,
            Tuesday   = 0x04,
            Wednesday = 0x08,
            Thursday  = 0x10,
            Friday    = 0x20,
            Saturday  = 0x40,
            AllDays   = 0x7F   // Each day
        }

        // Weekly trigger
        [ComImport, Guid("5038FC98-82FF-436D-8728-A512A57C9DC1"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface IWeeklyTrigger : ITrigger {
            new TriggerType Type { get; }
            new string Id { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new IRepetitionPattern Repetition { [return: MarshalAs(UnmanagedType.Interface)] get; [param: In, MarshalAs(UnmanagedType.Interface)] set; }
            new string ExecutionTimeLimit { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new string StartBoundary { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new string EndBoundary { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new bool Enabled { get; [param: In] set; }

            short DaysOfWeek { get; [param: In] set; }
            short WeeksInterval { get; [param: In] set; }
            string RandomDelay { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
        }

        // Month of the year values
        [Flags]
        public enum MonthsOfYear : short {
            January   = 0x0001,
            February  = 0x0002,
            March     = 0x0004,
            April     = 0x0008,
            May       = 0x0010,
            June      = 0x0020,
            July      = 0x0040,
            August    = 0x0080,
            September = 0x0100,
            October   = 0x0200,
            November  = 0x0400,
            December  = 0x0800,
            AllMonths = 0x0FFF   // Each month
        }

        // Monthly trigger
        [ComImport, Guid("97C45EF1-6B02-4A1A-9C0E-1EBFBA1500AC"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface IMonthlyTrigger : ITrigger {
            new TriggerType Type { get; }
            new string Id { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new IRepetitionPattern Repetition { [return: MarshalAs(UnmanagedType.Interface)] get; [param: In, MarshalAs(UnmanagedType.Interface)] set; }
            new string ExecutionTimeLimit { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new string StartBoundary { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new string EndBoundary { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new bool Enabled { get; [param: In] set; }

            int DaysOfMonth { get; [param: In] set; }
            short MonthsOfYear { get; [param: In] set; }
            bool RunOnLastDayOfMonth { get; [param: In] set; }
            string RandomDelay { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
        }

        // Week of the month values
        [Flags]
        public enum WeekOfMonth : short {
            FirstWeek  = 0x01,  // 1st week of the month
            SecondWeek = 0x02,  // 2nd week of the month
            ThirdWeek  = 0x04,  // 3rd week of the month
            FourthWeek = 0x08,  // 4th week of the month
            LastWeek   = 0x10,  // Last week of the month
            AllWeeks   = 0x1F   // Each week
        }

        // Monthly day-of-week trigger
        [ComImport, Guid("77D025A3-90FA-43AA-B52E-CDA5499B946A"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface IMonthlyDOWTrigger : ITrigger {
            new TriggerType Type { get; }
            new string Id { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new IRepetitionPattern Repetition { [return: MarshalAs(UnmanagedType.Interface)] get; [param: In, MarshalAs(UnmanagedType.Interface)] set; }
            new string ExecutionTimeLimit { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new string StartBoundary { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new string EndBoundary { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new bool Enabled { get; [param: In] set; }

            short DaysOfWeek { get; [param: In] set; }
            short WeeksOfMonth { get; [param: In] set; }
            short MonthsOfYear { get; [param: In] set; }
            bool RunOnLastWeekOfMonth { get; [param: In] set; }
            string RandomDelay { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
        }

        // Idle trigger
        [ComImport, Guid("D537D2B0-9FB3-4D34-9739-1FF5CE7B1EF3"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface IIdleTrigger : ITrigger {
            new TriggerType Type { get; }
            new string Id { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new IRepetitionPattern Repetition { [return: MarshalAs(UnmanagedType.Interface)] get; [param: In, MarshalAs(UnmanagedType.Interface)] set; }
            new string ExecutionTimeLimit { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new string StartBoundary { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new string EndBoundary { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new bool Enabled { get; [param: In] set; }
        }

        // Registration trigger
        [ComImport, Guid("4C8FEC3A-C218-4E0C-B23D-629024DB91A2"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface IRegistrationTrigger : ITrigger {
            new TriggerType Type { get; }
            new string Id { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new IRepetitionPattern Repetition { [return: MarshalAs(UnmanagedType.Interface)] get; [param: In, MarshalAs(UnmanagedType.Interface)] set; }
            new string ExecutionTimeLimit { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new string StartBoundary { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new string EndBoundary { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new bool Enabled { get; [param: In] set; }

            string Delay { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
        }

        // Boot trigger
        [ComImport, Guid("2A9C35DA-D357-41F4-BBC1-207AC1B1F3CB"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface IBootTrigger : ITrigger {
            new TriggerType Type { get; }
            new string Id { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new IRepetitionPattern Repetition { [return: MarshalAs(UnmanagedType.Interface)] get; [param: In, MarshalAs(UnmanagedType.Interface)] set; }
            new string ExecutionTimeLimit { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new string StartBoundary { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new string EndBoundary { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new bool Enabled { get; [param: In] set; }

            string Delay { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
        }

        // Logon trigger
        [ComImport, Guid("72DADE38-FAE4-4B3E-BAF4-5D009AF02B1C"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface ILogonTrigger : ITrigger {
            new TriggerType Type { get; }
            new string Id { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new IRepetitionPattern Repetition { [return: MarshalAs(UnmanagedType.Interface)] get; [param: In, MarshalAs(UnmanagedType.Interface)] set; }
            new string ExecutionTimeLimit { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new string StartBoundary { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new string EndBoundary { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new bool Enabled { get; [param: In] set; }

            string Delay { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            string UserId { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
        }

        // Session state change type
        public enum SessionStateChangeType {
            ConsoleConnect    = 1,  // Local session start
            ConsoleDisconnect = 2,  // Local session end
            RemoteConnect     = 3,  // Remote session start
            RemoteDisconnect  = 4,  // Remote session end
            SessionLock       = 7,  // Session locked
            SessionUnlock     = 8   // Session unlocked
        }

        // Session state change trigger
        [ComImport, Guid("754DA71B-4385-4475-9DD9-598294FA3641"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface ISessionStateChangeTrigger : ITrigger {
            new TriggerType Type { get; }
            new string Id { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new IRepetitionPattern Repetition { [return: MarshalAs(UnmanagedType.Interface)] get; [param: In, MarshalAs(UnmanagedType.Interface)] set; }
            new string ExecutionTimeLimit { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new string StartBoundary { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new string EndBoundary { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new bool Enabled { get; [param: In] set; }

            string Delay { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            string UserId { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            SessionStateChangeType StateChange { get; [param: In] set; }
        }

        // Trigger collection
        [ComImport, Guid("85DF5081-1B24-4F32-878A-D9D14DF4CB77"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface ITriggerCollection {
            int Count { get; }
            ITrigger this[int index] { [return: MarshalAs(UnmanagedType.Interface)] get; }
            [return: MarshalAs(UnmanagedType.Interface)]
            IEnumerator GetEnumerator();
            [return: MarshalAs(UnmanagedType.Interface)]
            ITrigger Create([In] TriggerType Type);
            void Remove([In, MarshalAs(UnmanagedType.Struct)] object index);
            void Clear();
        }
#endregion

#region Principal
        // Logon type
        // Also used by TaskFolder::RegisterTask, ::RegisterTaskDefinition
        public enum LogonType {
            None,                       // Not specified
            Password,                   // Password supplied when registering the task
            S4U,                        // Service for user existing interactive token (the default)
            InteractiveToken,           // User must be logged on
            Group,                      // Group activation
            ServiceAccount,             // Run using a System account
            InteractiveTokenOrPassword  // Password unless user is already logged in
        }

        // Run level
        public enum RunLevel {
            LUA,     // Least privilege
            Highest  // Highest available
        }

        // Principal
        [ComImport, Guid("D98D51E5-C9B4-496A-A9C1-18980261CF0F"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface IPrincipal {
            string Id { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            string DisplayName { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            string UserId { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            LogonType LogonType { get; set; }
            string GroupId { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            RunLevel RunLevel { get; set; }
        }

        // Token SID type
        public enum ProcessTokenSidType {
           None         = 0,  // No changes to token SID
           Unrestricted = 1,  // Restricts the ACL to a generated task SID
           Default      = 2   // Default settings
        }

        // Principal privilege
        public enum PrincipalPrivilege {
            SeCreateTokenPrivilege = 1,       // Create a token
            SeAssignPrimaryTokenPrivilege,    // Replace a token
            SeLockMemoryPrivilege,            // Lock physical memory pages
            SeIncreaseQuotaPrivilege,         // Adjust process memory quota
            SeUnsolicitedInputPrivilege,      // Read terminal
            SeMachineAccountPrivilege,        // Create a computer account
            SeTcbPrivilege,                   // Trusted computer base
            SeSecurityPrivilege,              // Security and audit access
            SeTakeOwnershipPrivilege,         // Take ownership
            SeLoadDriverPrivilege,            // Load and unload device drivers
            SeSystemProfilePrivilege,         // System performance profiling
            SeSystemtimePrivilege,            // Change system time
            SeProfileSingleProcessPrivilege,  // Single-process performance profiling
            SeIncreaseBasePriorityPrivilege,  // Change scheduling priority
            SeCreatePagefilePrivilege,        // Create a page file
            SeCreatePermanentPrivilege,       // Create a permanent object
            SeBackupPrivilege,                // Back up files
            SeRestorePrivilege,               // Restore from backup
            SeShutdownPrivilege,              // Shut down system
            SeDebugPrivilege,                 // Debug processes
            SeAuditPrivilege,                 // Generate audit logs
            SeSystemEnvironmentPrivilege,     // Modify firmware non-volatile memory
            SeChangeNotifyPrivilege,          // Be notified of file changes, traversal rights
            SeRemoteShutdownPrivilege,        // Shut down with a network request
            SeUndockPrivilege,                // Remove a laptop from docking station
            SeSyncAgentPrivilege,             // Use LDAP synchronization services
            SeEnableDelegationPrivilege,      // Mark accounts as trusted to delegate
            SeManageVolumePrivilege,          // Manage storage volumes
            SeImpersonatePrivilege,           // Impersonate a client after authentication
            SeCreateGlobalPrivilege,          // Create objects in the global namespace
            SeTrustedCredManAccessPrivilege,  // Access credential manager as a trusted caller
            SeRelabelPrivilege,               // Modify object integrity level
            SeIncreaseWorkingSetPrivilege,    // Allocate memory for applications
            SeTimeZonePrivilege,              // Change time zone
            SeCreateSymbolicLinkPrivilege     // Create symbolic links
        }

        // Principal 2
        [ComImport, Guid("248919AE-E345-4A6D-8AEB-E0D3165C904E"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface IPrincipal2 {
            ProcessTokenSidType ProcessTokenSidType { get; [param: In] set; }
            int RequiredPrivilegeCount { get; }
            string this[int index] { [return: MarshalAs(UnmanagedType.BStr)] get; }
            void AddRequiredPrivilege([In, MarshalAs(UnmanagedType.BStr)] string privilege);
        }
#endregion

#region Running
        // State
        // Used by registered-task and running-task interfaces
        public enum State {
            Unknown,   // State not known
            Disabled,  // Registered but not running or available to run
            Queued,    // Waiting to run
            Ready,     // Not running but ready to run
            Running    // Currently running
        }

        // Running task
        [ComImport, Guid("653758FB-7B9A-4F1E-A471-BEEB8E9B834E"), System.Security.SuppressUnmanagedCodeSecurity, DefaultMember("InstanceGuid")]
        public interface IRunningTask {
            string Name { [return: MarshalAs(UnmanagedType.BStr)] get; }
            string InstanceGuid { [return: MarshalAs(UnmanagedType.BStr)] get; }
            string Path { [return: MarshalAs(UnmanagedType.BStr)] get; }
            State State { get; }
            string CurrentAction { [return: MarshalAs(UnmanagedType.BStr)] get; }
            void Stop();
            void Refresh();
            uint EnginePID { get; }
        }

        // Running task collection
        [ComImport, Guid("6A67614B-6828-4FEC-AA54-6D52E8F1F2DB"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface IRunningTaskCollection {
            int Count { get; }
            IRunningTask this[object index] { [return: MarshalAs(UnmanagedType.Interface)] get; }
            [return: MarshalAs(UnmanagedType.Interface)]
            IEnumerator GetEnumerator();
        }
#endregion

#region Settings
        // Version compatibility
        public enum Compatibility {
            AT,    // POSIX `at` command
            V1,    // Windows 2000 and XP, Windows Server 2003
            V2,    // Windows Vista, Windows Server 2008
            V2_1,  // Windows 7, Windows Server 2008 Release 2
            V2_2,  // Windows 8, Windows Server 2012
            V2_3   // Windows 10, Windows Server 2016
        }

        // Multiple-instance policy
        public enum InstancesPolicy {
            Parallel,     // Another instance starts in parallel
            Queue,        // Another instance starts after previous instances done
            IgnoreNew,    // Only one instance allowed (the default)
            StopExisting  // Current instance is stopped before running a new one
        }

        // Idle settings
        [ComImport, Guid("84594461-0053-4342-A8FD-088FABF11F32"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface IIdleSettings {
            string IdleDuration { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            string WaitTimeout { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            bool StopOnIdleEnd { get; [param: In] set; }
            bool RestartOnIdle { get; [param: In] set; }
        }

        // Network settings
        [ComImport, Guid("9F7DEA84-C30B-4245-80B6-00E9F646F1B4"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface INetworkSettings {
            string Name { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            string Id { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
        }

        // Priority
        public enum Priority {
            RealTime        =  0,  // Critical
            Highest         =  1,  // High
            AboveNormalHigh =  2,
            AboveNormalLow  =  3,
            NormalHigh      =  4,
            Normal          =  5,
            NormalLow       =  6,
            BelowNormalHigh =  7,
            BelowNormalLow  =  8,
            Lowest          =  9,
            Idle            = 10
        }

        // Task settings
        [ComImport, Guid("8FD4711D-2D02-4C8C-87E3-EFF699DE127E"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface ITaskSettings {
            bool AllowDemandStart { get; [param: In] set; }
            string RestartInterval { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            int RestartCount { get; [param: In] set; }
            InstancesPolicy MultipleInstances { get; [param: In] set; }
            bool StopIfGoingOnBatteries { get; [param: In] set; }
            bool DisallowStartIfOnBatteries { get; [param: In] set; }
            bool AllowHardTerminate { get; [param: In] set; }
            bool StartWhenAvailable { get; [param: In] set; }
            string XmlText { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            bool RunOnlyIfNetworkAvailable { get; [param: In] set; }
            string ExecutionTimeLimit { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            bool Enabled { get; [param: In] set; }
            string DeleteExpiredTaskAfter { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            Priority Priority { get; [param: In] set; }
            Compatibility Compatibility { get; [param: In] set; }
            bool Hidden { get; [param: In] set; }
            IIdleSettings IdleSettings { [return: MarshalAs(UnmanagedType.Interface)] get; [param: In, MarshalAs(UnmanagedType.Interface)] set; }
            bool RunOnlyIfIdle { get; [param: In] set; }
            bool WakeToRun { get; [param: In] set; }
            INetworkSettings NetworkSettings { [return: MarshalAs(UnmanagedType.Interface)] get; [param: In, MarshalAs(UnmanagedType.Interface)] set; }
        }

        // Task settings 2
        [ComImport, Guid("2C05C3F0-6EED-4c05-A15F-ED7D7A98A369"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface ITaskSettings2 {
            bool DisallowStartOnRemoteAppSession { get; [param: In] set; }
            bool UseUnifiedSchedulingEngine { get; [param: In] set; }
        }

        // Maintenance settings
        [ComImport, Guid("A6024FA8-9652-4ADB-A6BF-5CFCD877A7BA"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface IMaintenanceSettings {
            string Period { [param: In, MarshalAs(UnmanagedType.BStr)] set; [return: MarshalAs(UnmanagedType.BStr)] get; }
            string Deadline { [param: In, MarshalAs(UnmanagedType.BStr)] set; [return: MarshalAs(UnmanagedType.BStr)] get; }
            bool Exclusive { [param: In] set; get; }
        }

        // Task settings 3
        [ComImport, Guid("0AD9D0D7-0C7F-4EBB-9A5F-D1C648DCA528"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface ITaskSettings3 : ITaskSettings {
            new bool AllowDemandStart { get; [param: In] set; }
            new string RestartInterval { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new int RestartCount { get; [param: In] set; }
            new InstancesPolicy MultipleInstances { get; [param: In] set; }
            new bool StopIfGoingOnBatteries { get; [param: In] set; }
            new bool DisallowStartIfOnBatteries { get; [param: In] set; }
            new bool AllowHardTerminate { get; [param: In] set; }
            new bool StartWhenAvailable { get; [param: In] set; }
            new string XmlText { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new bool RunOnlyIfNetworkAvailable { get; [param: In] set; }
            new string ExecutionTimeLimit { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new bool Enabled { get; [param: In] set; }
            new string DeleteExpiredTaskAfter { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            new Priority Priority { get; [param: In] set; }
            new Compatibility Compatibility { get; [param: In] set; }
            new bool Hidden { get; [param: In] set; }
            new IIdleSettings IdleSettings { [return: MarshalAs(UnmanagedType.Interface)] get; [param: In, MarshalAs(UnmanagedType.Interface)] set; }
            new bool RunOnlyIfIdle { get; [param: In] set; }
            new bool WakeToRun { get; [param: In] set; }
            new INetworkSettings NetworkSettings { [return: MarshalAs(UnmanagedType.Interface)] get; [param: In, MarshalAs(UnmanagedType.Interface)] set; }

            bool DisallowStartOnRemoteAppSession { get; [param: In] set; }
            bool UseUnifiedSchedulingEngine { get; [param: In] set; }
            IMaintenanceSettings MaintenanceSettings { [return: MarshalAs(UnmanagedType.Interface)] get; [param: In, MarshalAs(UnmanagedType.Interface)] set; }
            [return: MarshalAs(UnmanagedType.Interface)]
            IMaintenanceSettings CreateMaintenanceSettings();
            bool Volatile { get; [param: In] set; }
        }
#endregion

#region Registration
        // Registration info
        [ComImport, Guid("416D8B73-CB41-4EA1-805C-9BE9A5AC4A74"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface IRegistrationInfo {
            string Description { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            string Author { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            string Version { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            string Date { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            string Documentation { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            string XmlText { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            string URI { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            object SecurityDescriptor { [return: MarshalAs(UnmanagedType.Struct)] get; [param: In, MarshalAs(UnmanagedType.Struct)] set; }
            string Source { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
        }

        // Definition
        [ComImport, Guid("F5BC8FC5-536D-4F77-B852-FBC1356FDEB6"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface ITaskDefinition {
            IRegistrationInfo RegistrationInfo { [return: MarshalAs(UnmanagedType.Interface)] get; [param: In, MarshalAs(UnmanagedType.Interface)] set; }
            ITriggerCollection Triggers { [return: MarshalAs(UnmanagedType.Interface)] get; [param: In, MarshalAs(UnmanagedType.Interface)] set; }
            ITaskSettings Settings { [return: MarshalAs(UnmanagedType.Interface)] get; [param: In, MarshalAs(UnmanagedType.Interface)] set; }
            string Data { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
            IPrincipal Principal { [return: MarshalAs(UnmanagedType.Interface)] get; [param: In, MarshalAs(UnmanagedType.Interface)] set; }
            IActionCollection Actions { [return: MarshalAs(UnmanagedType.Interface)] get; [param: In, MarshalAs(UnmanagedType.Interface)] set; }
            string XmlText { [return: MarshalAs(UnmanagedType.BStr)] get; [param: In, MarshalAs(UnmanagedType.BStr)] set; }
        }

        // Registered task
        [ComImport, Guid("9C86F320-DEE3-4DD1-B972-A303F26B061E"), System.Security.SuppressUnmanagedCodeSecurity, DefaultMember("Path")]
        public interface IRegisteredTask {
            string Name { [return: MarshalAs(UnmanagedType.BStr)] get; }
            string Path { [return: MarshalAs(UnmanagedType.BStr)] get; }
            State State { get; }
            bool Enabled { get; set; }
            [return: MarshalAs(UnmanagedType.Interface)]
            IRunningTask Run([In, MarshalAs(UnmanagedType.Struct)] object parameters);
            [return: MarshalAs(UnmanagedType.Interface)]
            IRunningTask RunEx([In, MarshalAs(UnmanagedType.Struct)] object parameters, [In] int flags, [In] int sessionID, [In, MarshalAs(UnmanagedType.BStr)] string user);
            [return: MarshalAs(UnmanagedType.Interface)]
            IRunningTaskCollection GetInstances(int flags);
            DateTime LastRunTime { get; }
            int LastTaskResult { get; }
            int NumberOfMissedRuns { get; }
            DateTime NextRunTime { get; }
            ITaskDefinition Definition { [return: MarshalAs(UnmanagedType.Interface)] get; }
            string Xml { [return: MarshalAs(UnmanagedType.BStr)] get; }
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetSecurityDescriptor(int securityInformation);
            void SetSecurityDescriptor([In, MarshalAs(UnmanagedType.BStr)] string sddl, [In] int flags);
            void Stop(int flags);
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x60020011)]
            void GetRunTimes([In] ref WinBase.SYSTEMTIME pstStart, [In] ref WinBase.SYSTEMTIME pstEnd, [In, Out] ref uint pCount, [In, Out] ref IntPtr pRunTimes);
        }

        // Registered task collection
        [ComImport, Guid("86627EB4-42A7-41E4-A4D9-AC33A72F2D52"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface IRegisteredTaskCollection {
            int Count { get; }
            IRegisteredTask this[object index] { [return: MarshalAs(UnmanagedType.Interface)] get; }
            [return: MarshalAs(UnmanagedType.Interface)]
            IEnumerator GetEnumerator();
        }
#endregion

#region Folder
        // Query flags (argument to GetTasks)
        public enum Query {
            Hidden = 1  // Also retrieve hidden tasks
        }

        // Registration flags (argument to RegisterTask and RegisterTaskDefinition)
        [Flags]
        public enum Registration {
            ValidateOnly               = 0x01,  // Syntax check only, no changes
            Create                     = 0x02,  // Create only
            Update                     = 0x04,  // Update only
            CreateOrUpdate             = 0x06,  // Create or update
            Disable                    = 0x08,  // Register but disable
            DontAddPrincipalAce        = 0x10,  // Do not modify the access-control entry context principal
            IgnoreRegistrationTriggers = 0x20   // Will not run a task with registration trigger
        }

        // Run flags
        [Flags]
        public enum Run {
            NoFlags           = 0x00,  // No flags set
            AsSelf            = 0x01,  // Run as the calling user
            IgnoreConstraints = 0x02,  // Ignore restrictions that otherwise prevent the task from running
            UseSessionId      = 0x04,  // Using a terminal server session identifier
            UserSid           = 0x08   // Using a security identifier
        }

        // Folder
        [ComImport, Guid("8CFAC062-A080-4C15-9A88-AA7C2AF80DFC"), System.Security.SuppressUnmanagedCodeSecurity, DefaultMember("Path")]
        public interface ITaskFolder {
            string Name { [return: MarshalAs(UnmanagedType.BStr)] get; }
            string Path { [return: MarshalAs(UnmanagedType.BStr)] get; }
            [return: MarshalAs(UnmanagedType.Interface)]
            ITaskFolder GetFolder([MarshalAs(UnmanagedType.BStr)] string Path);
            [return: MarshalAs(UnmanagedType.Interface)]
            ITaskFolderCollection GetFolders(int flags);
            [return: MarshalAs(UnmanagedType.Interface)]
            ITaskFolder CreateFolder([In, MarshalAs(UnmanagedType.BStr)] string subFolderName, [In, Optional, MarshalAs(UnmanagedType.Struct)] object sddl);
            void DeleteFolder([MarshalAs(UnmanagedType.BStr)] string subFolderName, [In] int flags);
            [return: MarshalAs(UnmanagedType.Interface)]
            IRegisteredTask GetTask([MarshalAs(UnmanagedType.BStr)] string Path);
            [return: MarshalAs(UnmanagedType.Interface)]
            IRegisteredTaskCollection GetTasks(int flags);
            void DeleteTask([In, MarshalAs(UnmanagedType.BStr)] string Name, [In] int flags);
            [return: MarshalAs(UnmanagedType.Interface)]
            IRegisteredTask RegisterTask([In, MarshalAs(UnmanagedType.BStr)] string Path, [In, MarshalAs(UnmanagedType.BStr)] string XmlText, [In] Registration flags, [In, MarshalAs(UnmanagedType.Struct)] object UserId, [In, MarshalAs(UnmanagedType.Struct)] object password, [In] LogonType LogonType, [In, Optional, MarshalAs(UnmanagedType.Struct)] object sddl);
            [return: MarshalAs(UnmanagedType.Interface)]
            IRegisteredTask RegisterTaskDefinition([In, MarshalAs(UnmanagedType.BStr)] string Path, [In, MarshalAs(UnmanagedType.Interface)] ITaskDefinition pDefinition, [In] Registration flags, [In, MarshalAs(UnmanagedType.Struct)] object UserId, [In, MarshalAs(UnmanagedType.Struct)] object password, [In] LogonType LogonType, [In, Optional, MarshalAs(UnmanagedType.Struct)] object sddl);
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetSecurityDescriptor(int securityInformation);
            void SetSecurityDescriptor([In, MarshalAs(UnmanagedType.BStr)] string sddl, [In] int flags);
        }

        // Folder collection
        [ComImport, Guid("79184A66-8664-423F-97F1-637356A5D812"), System.Security.SuppressUnmanagedCodeSecurity]
        public interface ITaskFolderCollection {
            int Count { get; }
            ITaskFolder this[object index] { [return: MarshalAs(UnmanagedType.Interface)] get; }
            [return: MarshalAs(UnmanagedType.Interface)]
            IEnumerator GetEnumerator();
        }
#endregion

#region Service
        // Task service
        [ComImport, DefaultMember("TargetServer"), Guid("2FABA4C7-4DA9-4013-9697-20CC3FD40F85"), System.Security.SuppressUnmanagedCodeSecurity, CoClass(typeof(TaskSchedulerClass))]
        public interface ITaskService {
            [return: MarshalAs(UnmanagedType.Interface)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(1)]
            ITaskFolder GetFolder([In, MarshalAs(UnmanagedType.BStr)] string Path);
            [return: MarshalAs(UnmanagedType.Interface)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(2)]
            IRunningTaskCollection GetRunningTasks(int flags);
            [return: MarshalAs(UnmanagedType.Interface)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(3)]
            ITaskDefinition NewTask([In] uint flags);
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(4)]
            void Connect([In, Optional, MarshalAs(UnmanagedType.Struct)] object serverName, [In, Optional, MarshalAs(UnmanagedType.Struct)] object user, [In, Optional, MarshalAs(UnmanagedType.Struct)] object domain, [In, Optional, MarshalAs(UnmanagedType.Struct)] object password);
            [DispId(5)]
            bool Connected { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(5)] get; }
            [DispId(0)]
            string TargetServer { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0)] get; }
            [DispId(6)]
            string ConnectedUser { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(6)] get; }
            [DispId(7)]
            string ConnectedDomain { [return: MarshalAs(UnmanagedType.BStr)] [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(7)] get; }
            [DispId(8)]
            uint HighestVersion { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(8)] get; }
        }
#endregion

#region Class
        // Task Scheduler class
        [ComImport, DefaultMember("TargetServer"), Guid("0F87369F-A4E5-4CFC-BD3E-73E6154572DD"), ClassInterface((short)0), System.Security.SuppressUnmanagedCodeSecurity]
        public class TaskSchedulerClass {
        }
#endregion

#region Unused
        // Task variables
        // Not implemented per the Task Scheduler API documentation
        [ComImport, Guid("3E4C9351-D966-4B8B-BB87-CEBA68BB0107"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), System.Security.SuppressUnmanagedCodeSecurity]
        public interface ITaskVariables {
            [return: MarshalAs(UnmanagedType.BStr)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            string GetInput();
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetOutput([In, MarshalAs(UnmanagedType.BStr)] string input);
            [return: MarshalAs(UnmanagedType.BStr)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            string GetContext();
        }
#endregion

    }

}
