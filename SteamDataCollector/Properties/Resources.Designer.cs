﻿//------------------------------------------------------------------------------
// <auto-generated>
//     このコードはツールによって生成されました。
//     ランタイム バージョン:4.0.30319.42000
//
//     このファイルへの変更は、以下の状況下で不正な動作の原因になったり、
//     コードが再生成されるときに損失したりします。
// </auto-generated>
//------------------------------------------------------------------------------

namespace SteamDataCollector.Properties {
    using System;
    
    
    /// <summary>
    ///   ローカライズされた文字列などを検索するための、厳密に型指定されたリソース クラスです。
    /// </summary>
    // このクラスは StronglyTypedResourceBuilder クラスが ResGen
    // または Visual Studio のようなツールを使用して自動生成されました。
    // メンバーを追加または削除するには、.ResX ファイルを編集して、/str オプションと共に
    // ResGen を実行し直すか、または VS プロジェクトをビルドし直します。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   このクラスで使用されているキャッシュされた ResourceManager インスタンスを返します。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("SteamDataCollector.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   すべてについて、現在のスレッドの CurrentUICulture プロパティをオーバーライドします
        ///   現在のスレッドの CurrentUICulture プロパティをオーバーライドします。
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
        ///   steamdata に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string DataBase {
            get {
                return ResourceManager.GetString("DataBase", resourceCulture);
            }
        }
        
        /// <summary>
        ///   YOUR_PASSWORD に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string Password {
            get {
                return ResourceManager.GetString("Password", resourceCulture);
            }
        }
        
        /// <summary>
        ///   63306 に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string Port {
            get {
                return ResourceManager.GetString("Port", resourceCulture);
            }
        }
        
        /// <summary>
        ///   localhost に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string Server {
            get {
                return ResourceManager.GetString("Server", resourceCulture);
            }
        }
        
        /// <summary>
        ///   1500 に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string SleepMilSec {
            get {
                return ResourceManager.GetString("SleepMilSec", resourceCulture);
            }
        }
        
        /// <summary>
        ///   YOUR_ID に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string UserID {
            get {
                return ResourceManager.GetString("UserID", resourceCulture);
            }
        }
        
        /// <summary>
        ///   YOUR_URL に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string WebHook {
            get {
                return ResourceManager.GetString("WebHook", resourceCulture);
            }
        }
    }
}
