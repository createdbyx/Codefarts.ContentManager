﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Codefarts.ContentManager.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Codefarts.ContentManager.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No reader is available for the specified type..
        /// </summary>
        internal static string ContentManager_ERR_NoReaderIsAvailable {
            get {
                return ResourceManager.GetString("ContentManager_ERR_NoReaderIsAvailable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No writer is available for the specified type..
        /// </summary>
        internal static string ContentManager_ERR_NoWriterIsAvailable {
            get {
                return ResourceManager.GetString("ContentManager_ERR_NoWriterIsAvailable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A reader that reads the &apos;{0}&apos; type has already been added!.
        /// </summary>
        internal static string ContentManager_ERR_ReaderAlreadyAdded {
            get {
                return ResourceManager.GetString("ContentManager_ERR_ReaderAlreadyAdded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A writer that writes the &apos;{0}&apos; type has already been added!.
        /// </summary>
        internal static string ContentManager_ERR_WriterAlreadyAdded {
            get {
                return ResourceManager.GetString("ContentManager_ERR_WriterAlreadyAdded", resourceCulture);
            }
        }
    }
}
