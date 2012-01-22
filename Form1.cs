//-----------------------------------------------------------------------
// <copyright file="Form1.cs" company="Mike Caron">
//     using System.Standard.Disclaimer;
// </copyright>
//-----------------------------------------------------------------------

namespace MCModManager
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;

    /// <summary>
    /// The main window
    /// </summary>
    public partial class Form1 : Form
    {
        /// <summary>
        /// Initializes a new instance of the Form1 class
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            treeView1.BeginUpdate();
            foreach (var mod in AppData.Mods.Values)
            {
                var tvwModSubnodes = new[]
                {
                    new TreeNode("ID: " + mod.Id),
                    new TreeNode("Url: " + mod.Url),
                };

                var tvwMod = new TreeNode(mod.Name, tvwModSubnodes);
                var tvwModVer = new TreeNode("Versions:");
                tvwMod.Nodes.Add(tvwModVer);

                foreach (var ver in mod.Versions)
                {
                    var tvwVerSubnodes = new[]
                    {
                        new TreeNode("Url: " + ver.Url),
                        new TreeNode("Packing: " + ver.Packing),
                        new TreeNode("Hash: " + ver.Hash),
                        new TreeNode("Is Downloaded: " + ver.IsDownloaded),
                        new TreeNode("Cache Path: " + ver.CachePath),
                    };

                    var tvwVer = new TreeNode(ver.Ver, tvwVerSubnodes);
                    var tvwVerDep = new TreeNode("Dependencies:");
                    tvwVer.Nodes.Add(tvwVerDep);

                    foreach (var dep in ver.Dependencies)
                    {
                        var tvwDep = new TreeNode(dep.ToString());
                        tvwVerDep.Nodes.Add(tvwDep);
                    }

                    if (tvwVerDep.Nodes.Count == 0)
                    {
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
