using System;
using System.Collections.Generic;

namespace DeMol.Model
{
    internal class ScoresData
    {
        public List<Scores> Scores { get; set; } = new List<Scores>();
        public int GroepsPot { get; set; }
        public int MaxTeVerdienen { get; set; }
    }

    public struct Scores
    {
        public string Naam;
        public decimal totaalPercentage;

        public int aantalPasVragenVerdiend;
        public decimal aantalVragenBeantwoord;
        public int aantalVragenJuistBeantwoord;
        public decimal percentage;

        // public decimal finaleAantalVragenBeantwoord;
        // public int finaleAantalVragenJuistBeantwoord;
        // public decimal finalePercentage;

        // public int aantalKeerMolGeweest;
        // public int verdiendAlsMol;

        public TimeSpan totaleTijd;
        public int aantalKeerMolJuistGeraden;
        public int aantalKeerMolletjeJuistGeraden;

        public bool MolGeraden;
        public bool MolletjeGeraden;

        public void WistWieDeMolWas(string opdrachtDataOpdracht)
        {
            if (opdrachtDataOpdracht.SafeEqual("a"))
            {
                MolGeraden = true;
            }

            if (opdrachtDataOpdracht.SafeEqual("b"))
            {
                MolletjeGeraden = true;
            }
        }
    }
}