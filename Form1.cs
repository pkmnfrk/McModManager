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
            lstMods.BeginUpdate();
            foreach (Mod m in AppData.Mods.Values)
            {
                var lvi = new ListViewItem(m.ToString());
                lvi.Tag = m;

                if (m.IsMinecraftJar)
                {
                    lvi.Checked = true;
                    lvi.ToolTipText = "This isn't a mod, it's Minecraft itself. Thus, it cannot be deselected";
                }

                lstMods.Items.Add(lvi);
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
        }
    }
}
