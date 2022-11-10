using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.WpfGraphControl;

namespace MinWpfApp {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        DockPanel Panel = new DockPanel();

        public MainWindow() {
            InitializeComponent();
            this.Content = Panel;
            Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            GraphViewer graphViewer = new GraphViewer();
            graphViewer.BindToPanel(Panel);
            Graph graph = new Graph();

            var rooModule = "c:\\dev\\trunk\\Monthly.FullBuild.module";
            graph.AddEdge("root", rooModule);

            Dictionary<string, bool> modules = new Dictionary<string, bool>();

            GetModulesV2(
                rooModule, (current, next) => {

                    //File.AppendAllText("C:\\test.txt", $"{current}, {next}{Environment.NewLine}");
                    if (!modules.ContainsKey(next)) {

                        graph.AddEdge(current, next);
                        modules.Add(next, true);
                    }
                });

            graph.Attr.LayerDirection = LayerDirection.LR;
            graphViewer.Graph = graph; // throws exception
        }

        private XmlNamespaceManager _nm;

        private XmlNamespaceManager GetNamespaceManager() {
            if (_nm == null) {
                _nm = new XmlNamespaceManager(new NameTable());
                _nm.AddNamespace("pfx", "http://schemas.microsoft.com/developer/msbuild/2003");
            }

            return _nm;
        }

        private void GetModulesV2(string current, Action<string, string> handler) {
            if (!File.Exists(current)) {
                return;
            }

            var xml = XDocument.Load(current);

            var modules = xml.XPathSelectElements(
                "//pfx:Module", GetNamespaceManager());

            var dir = Path.GetDirectoryName(current);

            foreach (var x in modules) {
                string module = x.Attribute("Include").Value;
                string next = Path.Combine(dir, module);

                handler(current, next);
                GetModulesV2(next, handler);
            }
        }
    }
}
