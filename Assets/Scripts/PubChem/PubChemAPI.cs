using System;
using System.Collections.Generic;
using System.Text;

namespace MoleculeViewer.PubChem
{
    public static class URIs
    {
        public static string SmilesProperties(string smiles, IEnumerable<string> properties, string format = "json")
        {
            StringBuilder sb = new();
            sb.Append("https://pubchem.ncbi.nlm.nih.gov/rest/pug/compound/smiles/");
            sb.Append(smiles);
            sb.Append("/property/");

            bool isFirst = true;
            foreach (string property in properties)
            {
                if (isFirst)
                    isFirst = false;
                else
                    sb.Append(',');

                sb.Append(property);
            }

            sb.Append("/");
            sb.Append(format);
            return sb.ToString();
        }

        public static string SmilesProperties(string smiles, params string[] properties)
            => SmilesProperties(smiles, (IEnumerable<string>)properties);
    }

    [Serializable]
    public class PubChemResponse
    {
        public PropertyTable PropertyTable;
    }

    [Serializable]
    public class PropertyTable
    {
        public List<Properties> Properties;
    }

    [Serializable]
    public class Properties
    {
        public int CID;
        public string IUPACName;
        public string MolecularFormula;
    }
}
