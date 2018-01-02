﻿using System;
using System.Windows.Forms;
using Logic;
using Logic.Classes;

namespace Kassasysteem
{
    public partial class VoorraadScherm : Form
    {
        ListViewItem _item;
        private Product _product;
        public KassaApp App { get; private set; }
        public VoorraadScherm(KassaApp app)
        {
            InitializeComponent();
            App = app;
            SetAble(App.GetIsGemachtigd(), App.GetIsGemachtigd());
            RefreshGegevens();
        }

        private void SetAble(bool ableChange, bool ableNew)
        {
            groupBox2.Enabled = ableChange;
            groupBox3.Enabled = ableNew;
            btSluiten.Enabled = true;
        }

        private void RefreshGegevens()
        {
            lvProducten.Items.Clear();
            foreach (Product product in App.Voorraad.GetProducten())
            {
                _item = new ListViewItem(product.Id.ToString());
                _item.SubItems.Add(product.Naam);
                _item.SubItems.Add(product.Soort);
                _item.SubItems.Add(product.Voorraad.ToString());
                _item.SubItems.Add("€" + product.Prijs);
                _item.SubItems.Add("€" + product.Ledenprijs);
                lvProducten.Items.Add(_item);
            }
        }

        private void lvProducten_Click(object sender, EventArgs e)
        {
            var selected = Convert.ToInt32(lvProducten.SelectedItems[0].Text);
            foreach (Product product in App.Voorraad.GetProducten())
            {
                if (product.Id == selected)
                {
                    _product = product;
                    SetAble(App.GetIsGemachtigd(), App.GetIsGemachtigd());
                    btOpslaan.Enabled = App.Authentication.Lid.GetBestuursfunctie();
                    tbNaam.Text = product.Naam;
                    tbSoort.Text = product.Soort;
                    nudVoorraad.Value = product.Voorraad;
                    nudPrijs.Value = product.Prijs;
                    nudLedenprijs.Value = product.Ledenprijs;
                }
            }
        }

        private void btOpslaan_Click(object sender, EventArgs e)
        {
            try
            {
                if (tbNaam.Text == "")
                {
                    throw new Exception(@"U moet een naam invullen.");
                }
                if (tbSoort.Text == "")
                {
                    throw new Exception(@"U moet een soort invullen.");
                }
                if (nudVoorraad.Value == 0)
                {
                    throw new Exception(@"U moet een voorraad invullen.");
                }
                if (nudPrijs.Value == 0)
                {
                    throw new Exception(@"U moet een prijs invullen.");
                }
                if (nudLedenprijs.Value == 0)
                {
                    throw new Exception(@"U moet een ledenprijs invullen.");
                }
                _product.SetNaam(tbNaam.Text);
                _product.SetSoort(tbSoort.Text);
                _product.SetVoorraad(Convert.ToInt32(nudVoorraad.Value));
                _product.SetPrijs(nudPrijs.Value);
                _product.SetLedenprijs(nudLedenprijs.Value);
                App.EditProduct(_product);
                RefreshGegevens();
            }
            catch (Exception exception)
            {
                MessageBox.Show(@"Een error is opgetreden!" + Environment.NewLine + Environment.NewLine + exception.Message);
            }
        }

        private void btSluiten_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btNOpslaan_Click(object sender, EventArgs e)
        {
            try
            {
                if (tbNProductnaam.Text == "")
                {
                    throw new Exception(@"U moet een naam invullen.");
                }
                if (tbNSoort.Text == "")
                {
                    throw new Exception(@"U moet een soort invullen.");
                }
                if (nudNVoorraad.Value == 0)
                {
                    throw new Exception(@"U moet een voorraad invullen.");
                }
                if (nudNPrijs.Value == 0)
                {
                    throw new Exception(@"U moet een prijs invullen.");
                }
                if (nudNLedenprijs.Value == 0)
                {
                    throw new Exception(@"U moet een ledenprijs invullen.");
                }
                int id = lvProducten.Items.Count + 1;
                _item = new ListViewItem(id.ToString());
                _item.SubItems.Add(tbNProductnaam.Text);
                _item.SubItems.Add(tbNSoort.Text);
                _item.SubItems.Add(nudNVoorraad.Value.ToString());
                _item.SubItems.Add("€" + nudNPrijs.Value);
                _item.SubItems.Add("€" + nudNLedenprijs.Value);
                lvProducten.Items.Add(_item);
                App.AddNewProduct(id, tbNaam.Text, tbSoort.Text, Convert.ToInt32(nudVoorraad.Value), nudLedenprijs.Value, nudPrijs.Value);
            }
            catch (Exception exception)
            {
                MessageBox.Show(@"Een error is opgetreden!" + Environment.NewLine + Environment.NewLine + exception.Message);
            }
        }
    }
}
