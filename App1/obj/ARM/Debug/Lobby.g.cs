﻿

#pragma checksum "C:\Users\Anđelko Marčinko\Documents\Visual Studio 2013\Projects\App1\App1\Lobby.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "2098F1C9C019AECBD7F69C608914FE06"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace App1
{
    partial class Lobby : global::Windows.UI.Xaml.Controls.Page, global::Windows.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
 
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                #line 33 "..\..\..\Lobby.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.FindPeers_Tap;
                 #line default
                 #line hidden
                break;
            case 2:
                #line 34 "..\..\..\Lobby.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.btnBack_Click;
                 #line default
                 #line hidden
                break;
            case 3:
                #line 35 "..\..\..\Lobby.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.ConnectToSelected_Tap_1;
                 #line default
                 #line hidden
                break;
            case 4:
                #line 47 "..\..\..\Lobby.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.btnStart_Click;
                 #line default
                 #line hidden
                break;
            case 5:
                #line 50 "..\..\..\Lobby.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.SendMessage_Tap_1;
                 #line default
                 #line hidden
                break;
            }
            this._contentLoaded = true;
        }
    }
}


