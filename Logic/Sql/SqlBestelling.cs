﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Logic.Classes;
using Logic.Interfaces;

namespace Logic.Sql
{
    public class SqlBestelling : IBestellingServices
    {
        private string connectie;
        private SqlLid sqlLid;

        public SqlBestelling(string connectie)
        {
            this.connectie = connectie;
            sqlLid = new SqlLid(connectie);
        }
        public Bestelling GetBestellingById(int bestellingId)
        {
            throw new NotImplementedException();
        }

        public List<Bestelling> GetBestellingenVanLid(Lid lid)
        {
            throw new NotImplementedException();
        }

        public List<Bestelling> GetAllBestellingen()
        {
            try
            {
                List<Bestelling> bestellingen = new List<Bestelling>();
                using (SqlConnection conn = new SqlConnection(connectie))
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();

                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.CommandText = "SELECT * " +
                                              "FROM Bestelling " +
                                              "WHERE BKassaId = @kassaId;";


                            cmd.Parameters.AddWithValue("@kassaId", 0);
                            cmd.Connection = conn;

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int id = reader.GetInt32(0);
                                    Lid persoon = null;
                                    string persoonNaam = "";
                                    if (!reader.IsDBNull(1))
                                    {
                                        persoon = sqlLid.GetLidFromId(reader.GetInt32(1));
                                    }
                                    if (!reader.IsDBNull(2))
                                    {
                                        persoonNaam = reader.GetString(2);
                                    }
                                    DateTime datum = reader.GetDateTime(3);
                                    bool betaald = reader.GetBoolean(4);
                                    int kassaId = reader.GetInt32(5);
                                    DateTime datumbetaald = DateTime.Parse("1-1-1900");
                                    if (betaald)
                                    {
                                        datumbetaald = reader.GetDateTime(6);
                                    }
                                    bool betaaldBonnen = false;
                                    if (!reader.IsDBNull(7))
                                    {
                                        betaaldBonnen = reader.GetBoolean(7);
                                    }
                                    decimal betaaldBedrag = reader.GetDecimal(8);
                                    Bestelling _bestelling;

                                    if (persoon != null)
                                    {
                                        _bestelling = new Bestelling(id, persoon, datum);
                                    }
                                    else
                                    {
                                        _bestelling = new Bestelling(id, persoonNaam, datum);
                                    }
                                    _bestelling.AddProductenToList(GetProductenInBestelling(id));
                                    _bestelling.SetBetaaldBedrag(betaaldBedrag);
                                    bestellingen.Add(SuppGetAllBestellingen(_bestelling, betaald, kassaId, datumbetaald, betaaldBonnen));
                                }
                            }
                            return bestellingen;
                        }
                    }
                }
                return null;
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }

        private Bestelling SuppGetAllBestellingen(Bestelling bestelling, bool betaald, int kassaid,
            DateTime datumbetaald, bool bonnen)
        {
            bestelling.SetBetaald(betaald);
            bestelling.SetKassaId(kassaid);
            bestelling.SetDatumBetaald(datumbetaald);
            bestelling.SetBetaaldMetBonnen(bonnen);
            return bestelling;
        }

        public List<Product> GetProductenInBestelling(int bestellingId)
        {
            try
            {
                List<Product> producten = new List<Product>();
                using (SqlConnection conn = new SqlConnection(connectie))
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();

                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.CommandText = "SELECT ProdId, ProdNaam, SoortNaam, ProdVoorraad, ProdLedenPrijs, ProdPrijs " +
                                              "FROM Productbestelling " +
                                              "LEFT JOIN Product ON ProdId = PbProductId " +
                                              "LEFT JOIN Soort ON SoortId = ProdSoortId " +
                                              "WHERE PbBestellingId = @bestellingId;";
                            cmd.Connection = conn;

                            cmd.Parameters.AddWithValue("@bestellingId", bestellingId);

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int id = reader.GetInt32(0);
                                    string naam = reader.GetString(1);
                                    string soort = reader.GetString(2);
                                    int voorraad = reader.GetInt32(3);
                                    decimal ledenprijs = reader.GetDecimal(4);
                                    decimal prijs = reader.GetDecimal(5);

                                    Product product = new Product(id, naam, soort, voorraad, ledenprijs, prijs);
                                    producten.Add(product);
                                }
                            }
                            return producten;
                        }
                    }
                }
                return null;
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }

        public int AddBestellingMetPersoon(Bestelling bestelling)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectie))
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();

                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.CommandText = "INSERT INTO Bestelling (BPersoonid, BDatum, BBetalld, BKassaId, BDatumBetaald, BBetaaldMetBonnen, BBetaaldBedrag) VALUES (@persoonId, @datum, @betaald, @kassaId, @datumbetaald, @bonnen, @betaaldbedrag);";
                            cmd.Connection = conn;

                            cmd.Parameters.AddWithValue("@persoonId", bestelling.Lid.Id);
                            cmd.Parameters.AddWithValue("@datum", bestelling.Datum);
                            cmd.Parameters.AddWithValue("@betaald", bestelling.Betaald);
                            cmd.Parameters.AddWithValue("@kassaId", bestelling.KassaId);
                            cmd.Parameters.AddWithValue("@datumbetaald", bestelling.DatumBetaald);
                            cmd.Parameters.AddWithValue("@bonnen", bestelling.BetaaldMetBonnen);
                            cmd.Parameters.AddWithValue("@betaaldbedrag", 0.00);

                            cmd.ExecuteNonQuery();

                            cmd.CommandText = "SELECT BId FROM Bestelling WHERE BPersoonid = @persoonId AND BDatum = @datum AND BBetalld = @betaald AND BKassaId = @kassaId AND BBetaaldMetBonnen = @bonnen AND BDatumBetaald = @datumbetaald AND BBetaaldBedrag = @betaaldbedrag ORDER BY BId DESC;";
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int id = reader.GetInt32(0);
                                    return id;
                                }
                            }
                        }
                    }
                }
                return -1;
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }

        public int AddBestellingMetNaam(Bestelling bestelling)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectie))
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();

                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.CommandText = "INSERT INTO Bestelling (BNaam, BDatum, BBetalld, BKassaId, BDatumBetaald, BBetaaldMetBonnen, BBetaaldBedrag) VALUES (@naam, @datum, @betaald, @kassaId, @datumbetaald, @bonnen, @betaaldbedrag);";
                            cmd.Connection = conn;

                            cmd.Parameters.AddWithValue("@naam", bestelling.Naam);
                            cmd.Parameters.AddWithValue("@datum", bestelling.Datum);
                            cmd.Parameters.AddWithValue("@betaald", bestelling.Betaald);
                            cmd.Parameters.AddWithValue("@kassaId", bestelling.KassaId);
                            cmd.Parameters.AddWithValue("@datumbetaald", bestelling.DatumBetaald);
                            cmd.Parameters.AddWithValue("@bonnen", bestelling.BetaaldMetBonnen);
                            cmd.Parameters.AddWithValue("@betaaldbedrag", 0.00);

                            cmd.ExecuteNonQuery();

                            cmd.CommandText = "SELECT BId FROM Bestelling WHERE BNaam = @naam AND BDatum = @datum AND BBetalld = @betaald AND BKassaId = @kassaId AND BBetaaldMetBonnen = @bonnen AND BDatumBetaald = @datumbetaald AND BBetaaldBedrag = @betaaldbedrag ORDER BY BId DESC;";
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int id = reader.GetInt32(0);
                                    return id;
                                }
                            }
                        }
                    }
                }
                return -1;
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }

        public void EditBestelling(Bestelling bestelling)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectie))
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();

                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.CommandText = "UPDATE Bestelling SET BPersoonId = @persoonId, BNaam = @naam, BDatum = @datum, " +
                                              "BBetaald = @betaald, BKassaId = @kassaId, BDatumBetaald = @datumbetaald, BBetaaldMetBonnen = @bonnen " +
                                              "WHERE BId = @bestellingId;";
                            cmd.Connection = conn;

                            cmd.Parameters.AddWithValue("@persoonId", bestelling.Lid.Id);
                            cmd.Parameters.AddWithValue("@naam", bestelling.Naam);
                            cmd.Parameters.AddWithValue("@datum", bestelling.Datum);
                            cmd.Parameters.AddWithValue("@betaald", bestelling.Betaald);
                            cmd.Parameters.AddWithValue("@kassaId", bestelling.KassaId);
                            cmd.Parameters.AddWithValue("@datumbetaald", bestelling.DatumBetaald);
                            cmd.Parameters.AddWithValue("@bonnen", bestelling.BetaaldMetBonnen);
                            cmd.Parameters.AddWithValue("@bestellingId", bestelling.Id);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }

        public void DeleteBestelling(Bestelling bestelling)
        {
            throw new NotImplementedException();
        }

        public void BetaalBestelling(Bestelling bestelling)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectie))
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();

                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.CommandText = "UPDATE Bestelling SET BBetalld = 1, BBetaaldMetBonnen = @bonnen, BBetaaldBedrag = @betaaldBedrag, BDatumBetaald = @betaaldatum WHERE BId = @bestellingId;";
                            cmd.Connection = conn;

                            cmd.Parameters.AddWithValue("@bestellingId", bestelling.Id);
                            cmd.Parameters.AddWithValue("@bonnen", bestelling.BetaaldMetBonnen);
                            cmd.Parameters.AddWithValue("@betaaldBedrag", bestelling.BetaaldBedrag);
                            cmd.Parameters.AddWithValue("@betaaldatum", bestelling.DatumBetaald);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }
    }
}
