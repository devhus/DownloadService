﻿#pragma checksum "..\..\OptionsWindow.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "C32CC196AA3E4C5B057757E7BD8BF7E16DFE7F82E55CE5011E8AD3975C5E2312"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Devhus.DownloadService.Wpf;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace Devhus.DownloadService.Wpf {
    
    
    /// <summary>
    /// OptionsWindow
    /// </summary>
    public partial class OptionsWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 12 "..\..\OptionsWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel optionsPanel;
        
        #line default
        #line hidden
        
        
        #line 18 "..\..\OptionsWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox inputMaxCoresUsage;
        
        #line default
        #line hidden
        
        
        #line 25 "..\..\OptionsWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox inputDownloadsPath;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\OptionsWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox inputAutoQueueHandling;
        
        #line default
        #line hidden
        
        
        #line 38 "..\..\OptionsWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox inputRequiredSizeForChunks;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/Devhus.DownloadService.Wpf;component/optionswindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\OptionsWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 8 "..\..\OptionsWindow.xaml"
            ((Devhus.DownloadService.Wpf.OptionsWindow)(target)).Loaded += new System.Windows.RoutedEventHandler(this.Window_Loaded);
            
            #line default
            #line hidden
            
            #line 9 "..\..\OptionsWindow.xaml"
            ((Devhus.DownloadService.Wpf.OptionsWindow)(target)).Closing += new System.ComponentModel.CancelEventHandler(this.Window_Closing);
            
            #line default
            #line hidden
            return;
            case 2:
            this.optionsPanel = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 3:
            this.inputMaxCoresUsage = ((System.Windows.Controls.ComboBox)(target));
            
            #line 18 "..\..\OptionsWindow.xaml"
            this.inputMaxCoresUsage.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.inputMaxCoresUsage_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 4:
            this.inputDownloadsPath = ((System.Windows.Controls.TextBox)(target));
            
            #line 25 "..\..\OptionsWindow.xaml"
            this.inputDownloadsPath.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.inputDownloadsPath_TextChanged);
            
            #line default
            #line hidden
            return;
            case 5:
            this.inputAutoQueueHandling = ((System.Windows.Controls.ComboBox)(target));
            
            #line 30 "..\..\OptionsWindow.xaml"
            this.inputAutoQueueHandling.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.inputAutoQueueHandling_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 6:
            this.inputRequiredSizeForChunks = ((System.Windows.Controls.TextBox)(target));
            
            #line 38 "..\..\OptionsWindow.xaml"
            this.inputRequiredSizeForChunks.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.inputRequiredSizeForChunks_TextChanged);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

