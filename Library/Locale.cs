  //\\   OmenMon: Hardware Monitoring & Control Utility
 //  \\  Copyright © 2023 Piotr Szczepański * License: GPL3
     //  https://omenmon.github.io/

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using OmenMon.Library;

namespace OmenMon.Library.Locale {

#region Interface
    // Defines an interface for the retrieval
    // of localization-dependent natural language messages
    public interface ILocale : IDisposable {

        // Language retrieval methods
        public LocaleData.Language GetLanguage();
        public LocaleData.Language GetLanguage(string languageName);
        public string GetLanguageName(LocaleData.Language language);
        public string[] GetLanguages();

        // Language setting methods
        public void SetLanguage(LocaleData.Language language);
        public void SetLanguage(string languageName);

        // Localized message string methods
        public string Get(string messageId);
        public string GetDefault(string messageId);

    }
#endregion

    // Implements the general functionality for message localization
    // including the default text for all localizable messages
    public abstract class LocaleAbstract : LocaleData, ILocale {

        // The identifier of the currently-selected language
        protected Language lang;

        // Localizable message dictionary
        protected Dictionary<string, string>[] msg;

#region Initialization & Disposal
        // Constructs an instance
        public LocaleAbstract() {

            // Initialize the dictionary array
            msg = new Dictionary<string, string>[GetLanguages().Length];

            // Initialize the per-locale dictionaries
            foreach(string language in GetLanguages()) {
                msg[(int) GetLanguage(language)] =
                    new Dictionary<string, string>();
            }

            // Define the default fallback messages
            msg[(int) Language.Fallback] = msgFallback;

            // Set the language to the default fallback
            SetLanguage(Language.Fallback);

            // Note: This also loads the messages, so has to run
            // only when the dictionaries are already initalized

        }

        // Frees up the resources
        public void Dispose() {
        }
#endregion

#region Language Retrieval & Setting Methods
        // Retrieves the currently-set language given its enumerated identifier
        public virtual Language GetLanguage() {
            return this.lang;
        }

        // Retrieves the currently-set language given its identifier as a string
        public virtual Language GetLanguage(string languageName) {
            return (Language) Enum.Parse(typeof(Language), languageName);
        }

        // Retrieves the identifiers of all languages as a string array
        public virtual string[] GetLanguages() {
            return Enum.GetNames(typeof(Language));
        }

        // Retrieves the descriptive name of a language
        public virtual string GetLanguageName(Language language) {
            return Enum.GetName(typeof(Language), language);
        }

        // Sets the current language given its identifier as a string
        // and loads the messages for it
        public virtual void SetLanguage(Language language) {
            this.lang = language;

            if(language != Language.Fallback)
                Load(language);
        }

        // Sets the current language given its enumerated identifier
        public virtual void SetLanguage(string languageName) {
            SetLanguage(GetLanguage(languageName));
        }
#endregion

#region Localization Methods
        // Retrieves the default fallback localized message given its identifier
        public virtual string Get(string messageId) {
            return GetDefault(messageId);

        }

        // Retrieves the default fallback localized message given its identifier
        public virtual string GetDefault(string messageId) {
            string message;

            // Try to get the message for the default fallback language
            if(msg[(int) Language.Fallback].TryGetValue(messageId, out message))
                return message;

            // If no message can be retrieved
            else // Just return the identifier
                return messageId;

        }

        // Loads messages for a given language silently ignoring any errors
        protected virtual void Load(Language language) {
            Load(language, false);
        }

        // Loads messages for a given language and optionally report an error
        protected abstract void Load(Language language, bool showError);

    }
#endregion

    // Implements the language-specific functionality for message localization
    public sealed class Locale : LocaleAbstract, ILocale {

        // The following three statements ensure the class can be instantiated only once
        private static readonly Locale instance = new Locale();

        private Locale() { }

        public static Locale Instance {
            get { return instance; }
        }

        // Implements loading messages for a given language
        protected override void Load(Language language, bool showError = false) {

            if(Config.FilePath != "" && File.Exists(Config.FilePath)) {

                try {

                    // Load the file
                    XmlDocument xml = new XmlDocument();
                    xml.Load(Config.FilePath);

                   // Iterate through the nodes, populating the message dictionary
                   XmlNodeList messages = xml.SelectNodes("OmenMon/Messages/String");
                   foreach(XmlNode node in messages)
                       msg[(int) language].Add(node.Attributes["Key"].Value, node.InnerText);

                } catch {
		 
                    // Show an error message if the file is present but malformed
                    if(File.Exists(Config.FilePath) && showError)
                        App.Error("ErrLocaleLoad");

                }

            }

        }

        // Retrieves the localized natural-language message given its identifier
        // Or the default fallback if the message could not be found
        public override string Get(string messageId) {
            string message;

            // Try to get the value for the currently-selected language first
            if(msg[(int) lang].TryGetValue(messageId, out message))
                return message;

            // If no message can be retrieved
            else // Fallback to the default implementation
                return base.Get(messageId);
        }

    }

}
