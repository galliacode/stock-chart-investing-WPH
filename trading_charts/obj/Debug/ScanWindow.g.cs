﻿#pragma checksum "..\..\ScanWindow.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "A6820304C7CD624C6AA389B41BFAFBDBFE946A3D82917EEB5752D0BFD5E46941"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

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
using trading_charts;


namespace trading_charts {
    
    
    /// <summary>
    /// ScanWindow
    /// </summary>
    public partial class ScanWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 17 "..\..\ScanWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox FolderPathTextBox;
        
        #line default
        #line hidden
        
        
        #line 18 "..\..\ScanWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ScanOpenButton;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\ScanWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TraciLevelTextBox;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\ScanWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox TraciScanType;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\ScanWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBoxItem UP;
        
        #line default
        #line hidden
        
        
        #line 25 "..\..\ScanWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBoxItem DOWN;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\ScanWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid ScanFileGridList;
        
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
            System.Uri resourceLocater = new System.Uri("/trading_charts;component/scanwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\ScanWindow.xaml"
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
            
            #line 9 "..\..\ScanWindow.xaml"
            ((trading_charts.ScanWindow)(target)).Closing += new System.ComponentModel.CancelEventHandler(this.Window_Closing);
            
            #line default
            #line hidden
            
            #line 9 "..\..\ScanWindow.xaml"
            ((trading_charts.ScanWindow)(target)).StateChanged += new System.EventHandler(this.Window_StateChanged);
            
            #line default
            #line hidden
            return;
            case 2:
            this.FolderPathTextBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 17 "..\..\ScanWindow.xaml"
            this.FolderPathTextBox.KeyDown += new System.Windows.Input.KeyEventHandler(this.FolderPathTextBox_KeyDown);
            
            #line default
            #line hidden
            return;
            case 3:
            this.ScanOpenButton = ((System.Windows.Controls.Button)(target));
            
            #line 18 "..\..\ScanWindow.xaml"
            this.ScanOpenButton.Click += new System.Windows.RoutedEventHandler(this.ScanOpenButton_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.TraciLevelTextBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 22 "..\..\ScanWindow.xaml"
            this.TraciLevelTextBox.KeyDown += new System.Windows.Input.KeyEventHandler(this.TraciLevelTextBox_KeyDown);
            
            #line default
            #line hidden
            return;
            case 5:
            this.TraciScanType = ((System.Windows.Controls.ComboBox)(target));
            
            #line 23 "..\..\ScanWindow.xaml"
            this.TraciScanType.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.TraciScanType_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 6:
            this.UP = ((System.Windows.Controls.ComboBoxItem)(target));
            return;
            case 7:
            this.DOWN = ((System.Windows.Controls.ComboBoxItem)(target));
            return;
            case 8:
            this.ScanFileGridList = ((System.Windows.Controls.DataGrid)(target));
            
            #line 33 "..\..\ScanWindow.xaml"
            this.ScanFileGridList.SelectedCellsChanged += new System.Windows.Controls.SelectedCellsChangedEventHandler(this.ScanFileGridList_SelectedCellsChanged);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

