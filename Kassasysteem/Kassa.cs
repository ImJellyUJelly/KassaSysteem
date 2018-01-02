﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Kassasysteem;
using Logic;
using Logic.Classes;

namespace GUI
{
    public partial class Kassa : Form
    {
        public KassaApp App { get; }
        private AfrekenScherm _afrekenScherm;
        private ListViewItem _item;
        private VoorraadScherm _voorraadScherm;
        private PenningmeesterScherm _omzetScherm;
        private KasInUitScherm _kasinuitScherm;

        private bool _contanteVerkoop;
        private bool _contanteVerkoopLid;

        public Kassa(KassaApp app)
        {
            try
            {
                InitializeComponent();
                _contanteVerkoop = false;
                _contanteVerkoopLid = false;
                App = app;
                timer.Start();
                if (App.CheckDbConnection())
                {
                    UpdateKassaGegevens();
                    UpdateBestellingen();
                }
                else
                {
                    lbKassaNaam.Text = App.Lokatie;
                    lbDagDatum.Text = DateTime.Now.ToShortDateString();
                    lbOpenstaandeRekeningen.Text = @"0";
                    lbLoginnaam.Text = @"Admin";
                    lbKlantnaam.Text = "";
                    lbDatumklant.Text = "";
                    lbTotaalPrijs.Text = "";
                    lbLedenprijs.Text = "";
                    tbKlantnaam.Text = "";
                    cbLidNaam.Enabled = false;
                    lbConnectie.Text = @"mislukt";
                    lbConnectie.ForeColor = Color.Red;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(@"Een error is opgetreden!" + Environment.NewLine + Environment.NewLine +
                                exception.Message);
            }
        }

        private void btMaakBestelling_Click(object sender, EventArgs e)
        {
            try
            {
                if (tbKlantnaam.Text != "" && cbLidNaam.Text != "")
                {
                    throw new Exception("Je kunt geen 2 namen tegelijk invoeren voor een klant.");
                }
                if (tbKlantnaam.Text != "")
                {
                    App.AddBestelling(tbKlantnaam.Text, DateTime.Now);
                    UpdateBestellingen();
                    UpdateKassaGegevens();
                }
                if (cbLidNaam.Text != "")
                {
                    Lid lid = App.GetLidByName(cbLidNaam.Text);
                    App.AddBestelling(lid, DateTime.Now);
                    UpdateBestellingen();
                    UpdateKassaGegevens();
                }
                if (tbKlantnaam.Text == "" && cbLidNaam.Text == "")
                {
                    // Do nothing
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(@"Een error is opgetreden!" + Environment.NewLine + Environment.NewLine +
                                exception.Message);
            }
        }

        private void UpdateAllesNaBetalen()
        {
            UpdateBestellingen();
            UpdateKassaGegevens();
            lbKlantnaam.Text = "";
            lbDatumklant.Text = "";
            lbTotaalPrijs.Text = "";
            lbLedenprijs.Text = "";
            lvProductenInBestelling.Items.Clear();
        }

        private void UpdateBestellingen()
        {
            lvBestellingen.Items.Clear();
            foreach (Bestelling b in App.GetBestellingen())
            {
                if (b.Betaald == false)
                {
                    _item = new ListViewItem
                    {
                        Text = b.GetBesteller(),
                        Tag = b
                    };
                    lvBestellingen.Items.Add(_item);
                }
            }
        }

        private void UpdateLeden()
        {
            List<Lid> leden = App.GetLeden();
            cbLidNaam.Items.Clear();
            if (leden != null)
            {
                foreach (Lid lid in leden)
                {
                    cbLidNaam.Items.Add(lid.GetLidNaam());
                }
            }
        }

        private void UpdateKassaGegevens()
        {
            try
            {
                if (App.Database.GetIsConnected())
                {
                    UpdateLeden();
                }
                lbKassaNaam.Text = App.Lokatie;
                lbOpenstaandeRekeningen.Text = App.GetBestellingen().Count.ToString();
                lbDagDatum.Text = DateTime.Now.DayOfWeek + " " + DateTime.Now.ToShortDateString();
                lbLoginnaam.Text = App.Authentication.GetFullName();
                lbKlantnaam.Text = "";
                lbDatumklant.Text = "";
                lbTotaalPrijs.Text = "";
                lbLedenprijs.Text = "";
                tbKlantnaam.Text = "";
                cbLidNaam.Text = "";
                if (App.Database.GetIsConnected())
                {
                    lbConnectie.Text = @"gelukt";
                    lbConnectie.ForeColor = Color.Green;
                }
                gpDrinken.Visible = false;
                gpEten.Visible = false;
                gpMenu.Visible = true;
            }
            catch (Exception exception)
            {
                MessageBox.Show(@"Een error is opgetreden!" + Environment.NewLine + Environment.NewLine + exception.Message);
            }
        }

        private void UpdateKlantBestelling(Bestelling bestelling)
        {
            lvProductenInBestelling.Items.Clear();
            if (bestelling != null)
            {
                foreach (Product p in bestelling.GetProducten())
                {
                    ListViewItem lvi = new ListViewItem(p.Naam);
                    lvi.SubItems.Add("€" + p.Prijs);
                    lvi.SubItems.Add("€" + p.Ledenprijs.ToString(CultureInfo.InvariantCulture));
                    lvi.Tag = p;
                    lvProductenInBestelling.Items.Add(lvi);
                }
                lbTotaalPrijs.Text = @"€" + bestelling.TotaalPrijs.ToString(CultureInfo.InvariantCulture);
                lbLedenprijs.Text = @"€" + bestelling.TotaalLedenPrijs.ToString(CultureInfo.InvariantCulture);
                lbDatumklant.Text =
                    bestelling.Datum.ToShortDateString() + @" - " + bestelling.Datum.ToShortTimeString();
            }
            else
            {
                foreach (Product p in App.GetLosseVerkopen())
                {
                    ListViewItem lvi = new ListViewItem(p.Naam);
                    lvi.SubItems.Add("€" + p.Prijs);
                    lvi.SubItems.Add("€" + p.Ledenprijs.ToString(CultureInfo.InvariantCulture));
                    lvi.Tag = p;
                    lvProductenInBestelling.Items.Add(lvi);
                }
                // Eerst de prijs uit de losse verkopen bepalen en displayen
                decimal prijs = 0.00m;
                foreach (Product p in App.GetLosseVerkopen())
                {
                    prijs += p.Prijs;
                }
                lbTotaalPrijs.Text = @"€" + prijs.ToString(CultureInfo.InvariantCulture);
                // Dan de ledenprijs uit de losse verkopen bepalen en displayen
                prijs = 0.00m;
                foreach (Product p in App.GetLosseVerkopen())
                {
                    prijs += p.Ledenprijs;
                }
                lbLedenprijs.Text = @"€" + prijs.ToString(CultureInfo.InvariantCulture);
                lbDatumklant.Text = DateTime.Today.ToShortDateString();
            }
        }

        private void AddProductToBestelling(string productnaam)
        {
            // Als er een product meteen verkocht wordt, zonder bestelling, wordt de if uitgevoerd.
            // Bij het toevogen van en product aan een bestelling, wordt de else uitgevoerd.
            if (_contanteVerkoop || _contanteVerkoopLid)
            {
                if (_contanteVerkoop) // Losse verkoop als niet lid
                {
                    Product product = App.VindProduct(productnaam);
                    App.AddLosseVerkoop(product, false);
                    UpdateKlantBestelling(null);
                }
                else // Losse verkoop als lid
                {
                    Product product = App.VindProduct(productnaam);
                    App.AddLosseVerkoop(product, true);
                    UpdateKlantBestelling(null);
                }
            }
            else
            {
                try
                {
                    var bestelling = lvBestellingen.SelectedItems[0].Tag as Bestelling;
                    if (bestelling != null)
                    {
                        foreach (Bestelling b in App.GetBestellingen())
                        {
                            if (b.Id == bestelling.Id)
                            {
                                lvProductenInBestelling.Items.Clear();
                                Product product = App.VindProduct(productnaam);
                                if (product != null)
                                {
                                    App.AddProductToBestelling(b, product);
                                    UpdateKlantBestelling(b);
                                }
                                else
                                {
                                    MessageBox.Show(
                                        @"Het systeem kent dit product niet, raadpleeg uw nerd voor verdere hulp.");
                                }
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show(@"U doet een losse verkoop");
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(@"Een error is opgetreden! Mogelijke oplossing: selecteer een bestelling. "
                                    + Environment.NewLine + Environment.NewLine + exception.Message);
                }
            }
        }

        private void btAfrekenen_Click(object sender, EventArgs e)
        {
            try
            {
                var bestelling = lvBestellingen.SelectedItems[0].Tag as Bestelling;
                if (bestelling != null)
                {
                    _afrekenScherm = new AfrekenScherm(App, bestelling);
                    _afrekenScherm.ShowDialog();
                    UpdateAllesNaBetalen();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(@"Een error is opgetreden!" + Environment.NewLine + Environment.NewLine + exception.Message);
            }
        }

        private void lvBestellingen_Click(object sender, EventArgs e)
        {
            var bestelling = lvBestellingen.SelectedItems[0].Tag as Bestelling;
            if (bestelling != null)
            {
                lbKlantnaam.Text = bestelling.GetBesteller();
                lvProductenInBestelling.Items.Clear();
                UpdateKlantBestelling(bestelling);
                btVerwijderProduct.Enabled = true;
                btAfrekenen.Enabled = true;
            }
            else
            {
                MessageBox.Show(@"Er is geen bestelling geselecteerd.");
            }
        }
        #region buttonclicks_producten
        private void btKoffie_Click(object sender, EventArgs e)
        {
            AddProductToBestelling("Koffie");
        }

        private void btThee_Click(object sender, EventArgs e)
        {
            AddProductToBestelling("Thee");
        }

        private void btCocaCola_Click(object sender, EventArgs e)
        {
            AddProductToBestelling("Coca Cola");
        }

        private void btCocaColaZero_Click(object sender, EventArgs e)
        {
            AddProductToBestelling("Coca Cola Zero");
        }

        private void btFantaSinas_Click(object sender, EventArgs e)
        {
            AddProductToBestelling("Fanta");
        }

        private void btFantaCassis_Click(object sender, EventArgs e)
        {
            AddProductToBestelling("Fanta Cassis");
        }

        private void btSpaRood_Click(object sender, EventArgs e)
        {
            AddProductToBestelling("Spa Rood");
        }

        private void btBitterLemon_Click(object sender, EventArgs e)
        {
            AddProductToBestelling("Bitter Lemon");
        }

        private void btIceTea_Click(object sender, EventArgs e)
        {
            AddProductToBestelling("Ice Tea");
        }

        private void btChocomel_Click(object sender, EventArgs e)
        {
            AddProductToBestelling("Chocomel");
        }

        private void btAaDrink_Click(object sender, EventArgs e)
        {
            AddProductToBestelling("AA-Drink");
        }

        private void btAquarius_Click(object sender, EventArgs e)
        {
            AddProductToBestelling("Aquarius");
        }

        private void btHertogJan_Click(object sender, EventArgs e)
        {
            AddProductToBestelling("Hertog-Jan");
        }

        private void btJupiler_Click(object sender, EventArgs e)
        {
            AddProductToBestelling("Jupiler Alcoholvrij");
        }

        private void btAmstelRadler_Click(object sender, EventArgs e)
        {
            AddProductToBestelling("Amstel Radler");
        }

        private void btLeffeBruin_Click(object sender, EventArgs e)
        {
            AddProductToBestelling("Leffe Bruin");
        }

        private void btWisselendSpeciaalBier_Click(object sender, EventArgs e)
        {
            AddProductToBestelling("Barbar Blond");
        }

        private void btRodeWijn_Click(object sender, EventArgs e)
        {
            AddProductToBestelling("Rode Wijn");
        }

        private void btWitteWijn_Click(object sender, EventArgs e)
        {
            AddProductToBestelling("Witte Wijn");
        }

        private void btMars_Click(object sender, EventArgs e)
        {
            AddProductToBestelling("Mars");
        }

        private void btSnickers_Click(object sender, EventArgs e)
        {
            AddProductToBestelling("Snickers");
        }

        private void btCrokyNaturel_Click(object sender, EventArgs e)
        {
            AddProductToBestelling("Naturel Chips");
        }

        private void btCrokyPaprika_Click(object sender, EventArgs e)
        {
            AddProductToBestelling("Paprika Chips");
        }

        private void btBiFi_Click(object sender, EventArgs e)
        {
            AddProductToBestelling("BiFi");
        }

        private void btBonnenkaart_Click(object sender, EventArgs e)
        {
            AddProductToBestelling("Munten");
        }

        private void btSchrobbeler_Click(object sender, EventArgs e)
        {
            AddProductToBestelling("Schrobbeler");
        }
        #endregion

        private void btVerwijderProduct_Click(object sender, EventArgs e)
        {
            try
            {
                var bestelling = lvBestellingen.SelectedItems[0].Tag as Bestelling;

                if (bestelling != null)
                {
                    var product = lvProductenInBestelling.SelectedItems[0].Tag as Product;
                    if (product != null)
                    {
                        MessageBox.Show($@"{product.Naam} wordt uit de bestelling van {bestelling.GetBesteller()} verwijderd.");
                        App.RemoveProductVanBestelling(bestelling, product);
                        UpdateKlantBestelling(bestelling);
                    }
                    else
                    {
                        MessageBox.Show(@"Product is niet uit bestelling verwijderd.");
                    }
                }
                else
                {
                    MessageBox.Show(@"Een error is opgetreden!");
                }
                UpdateBestellingen();
            }
            catch (Exception exception)
            {
                MessageBox.Show(@"Een error is opgetreden!" + Environment.NewLine + Environment.NewLine +
                                exception.Message);
            }
        }

        private void afmeldenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Afmelden();
        }

        private void gegevensWijzigenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                InlogScherm login = new InlogScherm(this, App);
                login.ShowDialog();
                UpdateBestellingen();
                UpdateKassaGegevens();
                UpdateKlantBestelling(null);
            }
            catch (Exception exception)
            {
                MessageBox.Show(@"Een error is opgetreden!" + Environment.NewLine + Environment.NewLine +
                                exception.Message);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _voorraadScherm = new VoorraadScherm(App);
            _voorraadScherm.ShowDialog();
        }

        private void omzetInzienToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // TODO: Omzet zichbaar maken vanuit db
                _omzetScherm = new PenningmeesterScherm(App);
                _omzetScherm.ShowDialog();
            }
            catch (Exception exception)
            {
                MessageBox.Show(@"Een error is opgetreden!" + Environment.NewLine + Environment.NewLine +
                                exception.Message);
            }
        }

        private void bestelgeschiedenisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                BestelgeschiedenisScherm geschiedenis = new BestelgeschiedenisScherm(App);
                geschiedenis.ShowDialog();
            }
            catch (Exception exception)
            {
                MessageBox.Show(@"Een error is opgetreden!" + Environment.NewLine + Environment.NewLine +
                                exception.Message);
            }
        }

        private void ledenlijstToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                LedenScherm leden = new LedenScherm(App);
                leden.ShowDialog();
                UpdateLeden();
            }
            catch (Exception exception)
            {
                MessageBox.Show(@"Een error is opgetreden!" + Environment.NewLine + Environment.NewLine +
                                exception.Message);
            }
        }

        private void btEten_Click(object sender, EventArgs e)
        {
            gpEten.Visible = true;
            gpMenu.Visible = false;
        }

        private void btDrinken_Click(object sender, EventArgs e)
        {
            gpDrinken.Visible = true;
            gpMenu.Visible = false;
        }

        private void btTerug1_Click(object sender, EventArgs e)
        {
            gpMenu.Visible = true;
            gpDrinken.Visible = false;
        }

        private void btTerug2_Click(object sender, EventArgs e)
        {
            gpMenu.Visible = true;
            gpEten.Visible = false;
        }

        private void afmeldenToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Afmelden();
        }

        private void Afmelden()
        {
            try
            {
                DialogResult dialogResult = MessageBox.Show(@"Weet u zeker dat u wilt afmelden?", @"Afmelden",
                    MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    Close();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(@"Een error is opgetreden!" + Environment.NewLine + Environment.NewLine +
                                exception.Message);
            }
        }

        private void verkoopgeschiedenisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: losse verkopen weergave 
        }

        private void sUKToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (App.GetIsGemachtigd())
                {
                    _kasinuitScherm = new KasInUitScherm(App);
                    _kasinuitScherm.Show();
                }
                else
                {
                    throw new Exception("U heeft geen toegang tot deze pagina.");
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(@"Een error is opgetreden!" + Environment.NewLine + Environment.NewLine +
                    exception.Message);
            }
        }

        private void cbSoortVerkoop_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string selected = cbSoortVerkoop.SelectedItem.ToString();
                if (selected == null)
                {
                    throw new Exception("Er is niets geselecteerd.");
                }
                if (selected == "Losse verkoop wel lid")
                {
                    BestellingSoortEnum soort = BestellingSoortEnum.Losse_verkoop_wel_lid;
                    SelectCorrectSellMethod(soort);
                }
                else
                {
                    if (selected == "Losse verkoop niet lid")
                    {
                        BestellingSoortEnum soort = BestellingSoortEnum.Losse_verkoop_niet_lid;
                        SelectCorrectSellMethod(soort);
                    }
                    else
                    {
                        BestellingSoortEnum soort = (BestellingSoortEnum)Enum.Parse(typeof(BestellingSoortEnum), selected);
                        SelectCorrectSellMethod(soort);
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(@"Een error is opgetreden!" + Environment.NewLine + Environment.NewLine + exception.Message);
            }
        }

        private void SelectCorrectSellMethod(BestellingSoortEnum sellMethod)
        {
            switch (sellMethod)
            {
                case BestellingSoortEnum.Bestelling:
                    SelectSellMethod(true, true);
                    break;
                case BestellingSoortEnum.Losse_verkoop_niet_lid:
                    SelectSellMethod(false, false);
                    break;
                case BestellingSoortEnum.Losse_verkoop_wel_lid:
                    SelectSellMethod(false, true);
                    break;
            }
        }

        private void SelectSellMethod(bool isBestelling, bool isLid)
        {
            if (isBestelling)
            {
                groupBox6.Visible = true;
                _contanteVerkoop = false;
                _contanteVerkoopLid = false;
            }
            else
            {
                lvProductenInBestelling.Items.Clear();
                foreach (Product p in App.GetLosseVerkopen())
                {
                    ListViewItem lvi = new ListViewItem(p.Naam);
                    lvi.SubItems.Add("€" + p.Prijs);
                    lvi.SubItems.Add("€" + p.Ledenprijs.ToString(CultureInfo.InvariantCulture));
                    lvi.Tag = p;
                    lvProductenInBestelling.Items.Add(lvi);
                }
                if (isLid)
                {
                    groupBox6.Visible = false;
                    _contanteVerkoop = true;
                    _contanteVerkoopLid = true;
                }
                else
                {
                    groupBox6.Visible = false;
                    _contanteVerkoop = true;
                    _contanteVerkoopLid = false;
                }
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            SelectSellMethod(true, true);
            cbSoortVerkoop.Text = "Bestelling";
        }

        private void voorraadToevoegenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (App.GetIsGemachtigd())
                {
                    LeveringScherm scherm = new LeveringScherm(App);
                    scherm.Show();
                }
                else
                {
                    throw new Exception("U heeft geen toegang tot deze pagina.");
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(@"Een error is opgetreden!" + Environment.NewLine + Environment.NewLine + exception.Message);
            }
        }
    }
}