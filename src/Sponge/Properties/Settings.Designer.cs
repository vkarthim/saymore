//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.4927
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SIL.Sponge.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "9.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.SettingsProviderAttribute(typeof(SilUtils.PortableSettingsProvider))]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Collections.Specialized.StringCollection MRUList {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["MRUList"]));
            }
            set {
                this["MRUList"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.SettingsProviderAttribute(typeof(SilUtils.PortableSettingsProvider))]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public int LocalizationDlgSplitterPos {
            get {
                return ((int)(this["LocalizationDlgSplitterPos"]));
            }
            set {
                this["LocalizationDlgSplitterPos"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.SettingsProviderAttribute(typeof(SilUtils.PortableSettingsProvider))]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0, 0, 0, -1")]
        public global::System.Drawing.Rectangle LocalizationDlgBounds {
            get {
                return ((global::System.Drawing.Rectangle)(this["LocalizationDlgBounds"]));
            }
            set {
                this["LocalizationDlgBounds"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.SettingsProviderAttribute(typeof(SilUtils.PortableSettingsProvider))]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0, 0, 0, -1")]
        public global::System.Drawing.Rectangle MainWndBounds {
            get {
                return ((global::System.Drawing.Rectangle)(this["MainWndBounds"]));
            }
            set {
                this["MainWndBounds"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.SettingsProviderAttribute(typeof(SilUtils.PortableSettingsProvider))]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0, 0, 0, -1")]
        public global::System.Drawing.Rectangle WelcomeDlgBounds {
            get {
                return ((global::System.Drawing.Rectangle)(this["WelcomeDlgBounds"]));
            }
            set {
                this["WelcomeDlgBounds"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.SettingsProviderAttribute(typeof(SilUtils.PortableSettingsProvider))]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public int SessionVwSplitterPos {
            get {
                return ((int)(this["SessionVwSplitterPos"]));
            }
            set {
                this["SessionVwSplitterPos"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Configuration.SettingsProviderAttribute(typeof(SilUtils.PortableSettingsProvider))]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public string SessionFileCols {
            get {
                return ((string)(this["SessionFileCols"]));
            }
            set {
                this["SessionFileCols"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Configuration.SettingsProviderAttribute(typeof(SilUtils.PortableSettingsProvider))]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public int SessionFileInfoPanelSplitterPos {
            get {
                return ((int)(this["SessionFileInfoPanelSplitterPos"]));
            }
            set {
                this["SessionFileInfoPanelSplitterPos"] = value;
            }
        }
    }
}
