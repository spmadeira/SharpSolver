﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SharpSolverTest {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class InputsResource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal InputsResource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("SharpSolverTest.InputsResource", typeof(InputsResource).Assembly);
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
        ///   Looks up a localized string similar to MAX Z = X1
        ///X1 &gt;= 1
        ///X1 &lt;= -1.
        /// </summary>
        internal static string Infeasible1 {
            get {
                return ResourceManager.GetString("Infeasible1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MAX Z = 3X1 + 5X2
        ///
        ///X1 &lt;= 4
        ///2X2 &lt;= 12
        ///3X1 + 2X2 &lt;= 18.
        /// </summary>
        internal static string Linear1 {
            get {
                return ResourceManager.GetString("Linear1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MAX Z = 3X1 + 2X2 - 5X3
        ///4X1 - 2X2 + 2X3 &lt;= 4
        ///-2X1 + X2 - X3 &gt;= -1.
        /// </summary>
        internal static string Unbounded1 {
            get {
                return ResourceManager.GetString("Unbounded1", resourceCulture);
            }
        }
    }
}
