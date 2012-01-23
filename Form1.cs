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
        private Dictionary<Mod, ListViewItem> listCache = new Dictionary<Mod, ListViewItem>();
        private Dictionary<Mod, int> autoChecked = new Dictionary<Mod, int>();

        /// <summary>
        /// Initializes a new instance of the Form1 class
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lstMods.BeginUpdate();
            foreach (Mod m in AppData.Mods.Values)
            {
                var lvi = new ListViewItem(m.ToString());
                lvi.Tag = m;

                if (m.IsMinecraftJar)
                {
                    lvi.Checked = true;
                    lvi.ToolTipText = "This isn't a mod, it's Minecraft itself. Thus, it cannot be deselected";
                    this.autoChecked[m] = 1;
                }
                else
                {
                    this.autoChecked[m] = 0;
                }

                lstMods.Items.Add(lvi);
                this.listCache[m] = lvi;
            }

            lstMods.EndUpdate();
        }

        private void LstMods_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            Mod m = lstMods.Items[e.Index].Tag as Mod;

            if (m.IsMinecraftJar)
            {
                e.NewValue = CheckState.Checked;
            }
            else if (e.NewValue == CheckState.Checked)
            {
                if (e.CurrentValue == CheckState.Unchecked)
                {
                    this.autoChecked[m]++;
                }

                foreach (var dep in m.Versions.Last().Dependencies.Select(d => d.OnlyValue()))
                {
                    // locate the mod this represents
                    if (AppData.Mods.ContainsKey(dep))
                    {
                        var tmod = AppData.Mods[dep];

                        var lvi = this.listCache[tmod];
                        lvi.Checked = true;
                        if (!tmod.IsMinecraftJar)
                        {
                            lvi.ToolTipText = "This mod is required by another mod and cannot be unselected while that other mod is selected";
                        }

                        this.autoChecked[tmod]++;
                    }
                }
            }
            else if (e.NewValue == CheckState.Unchecked)
            {
                if (e.CurrentValue == CheckState.Checked)
                {
                    this.autoChecked[m]--;
                }

                foreach (var dep in m.Versions.Last().Dependencies.Select(d => d.OnlyValue()))
                {
                    // locate the mod this represents
                    if (AppData.Mods.ContainsKey(dep))
                    {
                        var tmod = AppData.Mods[dep.OnlyValue()];

                        this.autoChecked[tmod]--;

                        if (this.autoChecked[tmod] == 0)
                        {
                            var lvi = this.listCache[tmod];
                            lvi.Checked = false;
                            lvi.ToolTipText = null;
                        }
                    }
                }
            }
        }
    }
}
