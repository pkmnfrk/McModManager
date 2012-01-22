using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MCModManager {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            treeView1.BeginUpdate();
            foreach (var mod in AppData.Mods.Values) {
                var tvwMod = new TreeNode(mod.Name, new [] {
                        new TreeNode("ID: " + mod.Id),
                        new TreeNode("Url: " + mod.Url),
                });
                var tvwModVer = new TreeNode("Versions:");
                tvwMod.Nodes.Add(tvwModVer);

                foreach (var ver in mod.Versions) {
                    var tvwVer = new TreeNode(ver.Ver, new [] {
                        new TreeNode("Url: " + ver.Url),
                        new TreeNode("Packing: " + ver.Packing),
                    });
                    var tvwVerDep = new TreeNode("Dependencies:");
                    tvwVer.Nodes.Add(tvwVerDep);

                    foreach (var dep in ver.Dependencies) {
                        var tvwDep = new TreeNode(dep.ToString());
                        tvwVerDep.Nodes.Add(tvwDep);
                    }

                    if (tvwVerDep.Nodes.Count == 0) {
                        tvwVerDep.Nodes.Add(new TreeNode("None") { NodeFont = new Font(treeView1.Font, FontStyle.Italic) });
                    }

                    tvwModVer.Nodes.Add(tvwVer);
                }

                treeView1.Nodes.Add(tvwMod);
            }
            treeView1.EndUpdate();
        }
    }
}
