using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Rucker.Tests
{
    public class Alphabet
    {
        #region Properties
        public string A { get; set; }
        public string B { get; set; }
        public string C { get; set; }
        public string D { get; set; }
        public string E { get; set; }
        public string F { get; set; }
        public string G { get; set; }
        public string H { get; set; }
        public string I { get; set; }
        public string J { get; set; }
        public string K { get; set; }
        public string L { get; set; }
        public string M { get; set; }
        public string N { get; set; }
        public string O { get; set; }
        public string P { get; set; }
        public string Q { get; set; }
        public string R { get; set; }
        public string S { get; set; }
        public string T { get; set; }
        public string U { get; set; }
        public string V { get; set; }
        public string W { get; set; }
        public string X { get; set; }
        public string Y { get; set; }
        public string Z { get; set; }
        #endregion

        public Alphabet()
        {
            foreach (var property in GetType().GetProperties().Where(p => p.Name != "Id"))
            {
                property.SetValue(this, property.Name);
            }
        }
    }

    public static class AlphabetExtensions
    {
        public static DataTable ToDataTable(this IEnumerable<Alphabet> alphabets)
        {
            var dt = CreateAlphabetDataTable();

            foreach (var alphabet in alphabets)
            {
                dt.Rows.Add(CreateAlphabetDataRow(alphabet, dt));
            }

            return dt;
        }

        private static DataTable CreateAlphabetDataTable()
        {
            var dt = new DataTable();

            dt.Columns.Add("A", typeof (string));
            dt.Columns.Add("B", typeof (string));
            dt.Columns.Add("C", typeof (string));
            dt.Columns.Add("D", typeof (string));
            dt.Columns.Add("E", typeof (string));
            dt.Columns.Add("F", typeof (string));
            dt.Columns.Add("G", typeof (string));
            dt.Columns.Add("H", typeof (string));
            dt.Columns.Add("I", typeof (string));
            dt.Columns.Add("J", typeof (string));
            dt.Columns.Add("K", typeof (string));
            dt.Columns.Add("L", typeof (string));
            dt.Columns.Add("M", typeof (string));
            dt.Columns.Add("N", typeof (string));
            dt.Columns.Add("O", typeof (string));
            dt.Columns.Add("P", typeof (string));
            dt.Columns.Add("Q", typeof (string));
            dt.Columns.Add("R", typeof (string));
            dt.Columns.Add("S", typeof (string));
            dt.Columns.Add("T", typeof (string));
            dt.Columns.Add("U", typeof (string));
            dt.Columns.Add("V", typeof (string));
            dt.Columns.Add("W", typeof (string));
            dt.Columns.Add("X", typeof (string));
            dt.Columns.Add("Y", typeof (string));
            dt.Columns.Add("Z", typeof (string));

            return dt;
        }

        private static DataRow CreateAlphabetDataRow(Alphabet a, DataTable dt)
        {
            var dr = dt.NewRow();

            dr["A"] = a.A;
            dr["B"] = a.B;
            dr["C"] = a.C;
            dr["D"] = a.D;
            dr["E"] = a.E;
            dr["F"] = a.F;
            dr["G"] = a.G;
            dr["H"] = a.H;
            dr["I"] = a.I;
            dr["J"] = a.J;
            dr["K"] = a.K;
            dr["L"] = a.L;
            dr["M"] = a.M;
            dr["N"] = a.N;
            dr["O"] = a.O;
            dr["P"] = a.P;
            dr["Q"] = a.Q;
            dr["R"] = a.R;
            dr["S"] = a.S;
            dr["T"] = a.T;
            dr["U"] = a.U;
            dr["V"] = a.V;
            dr["W"] = a.W;
            dr["X"] = a.X;
            dr["Y"] = a.Y;
            dr["Z"] = a.Z;

            return dr;
        }
    }
}