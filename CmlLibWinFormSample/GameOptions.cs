﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CmlLib.Utils;

namespace CmlLibWinFormSample
{
    public partial class GameOptions : Form
    {
        public string Path;

        public GameOptions(string path)
        {
            this.Path = path;
            InitializeComponent();
        }

        private void GameOptions_Load(object sender, EventArgs e)
        {
            txtPath.Text = Path;
            btnLoad_Click(null, null);
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            this.Path = txtPath.Text;

            var options = GameOptionsFile.ReadFile(this.Path);
            foreach (var item in options)
            {
                listView1.Items.Add(new ListViewItem(new string[]
                {
                    item.Key,
                    item.Value
                }));
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            OpenPanel("", "", true);
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;
            var key = listView1.SelectedItems[0].Text;
            listView1.Items.RemoveByKey(key);
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;
            var key = listView1.SelectedItems[0].Text;
            var value = listView1.SelectedItems[0].SubItems[1].Text;

            OpenPanel(key, value, false);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var dict = new Dictionary<string, string>();
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                var item = listView1.Items[i];
                dict.Add(item.Text, item.SubItems[1].Text);
            }

            GameOptionsFile.WriteFile(Path, dict);
        }

        private void OpenPanel(string key, string value, bool enableKey)
        {
            pKeyValue.Visible = true;
            txtKey.Text = key;
            txtValue.Text = value;
            txtKey.Enabled = enableKey;
        }

        string oKey = "";
        string oValue = "";

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (txtKey.Enabled)
                listView1.Items.Add(new ListViewItem(new string[] { txtKey.Text, txtValue.Text } ));
            else
                listView1.Items.Find(oKey, false)[0].SubItems[1].Text = txtValue.Text;
            pKeyValue.Visible = false;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            pKeyValue.Visible = false;
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            txtKey.Text = oKey;
            txtValue.Text = oValue;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}