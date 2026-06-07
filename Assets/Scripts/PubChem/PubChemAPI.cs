using System;
using System.Collections.Generic;
using System.Text;

namespace MoleculeViewer.PubChem
{
    public static class URIs
    {
        public enum PubChemFormat
        {
            JSON,
            XML,
            ASN,
            ASNT,
            SDF,
            MOL,
            PNG
        }

        public static string SanitizeSMILES(string smiles) => Uri.EscapeDataString(smiles);
        
        public static string SmilesProperties(string smiles, IEnumerable<string> properties, PubChemFormat format = PubChemFormat.JSON)
        {
            smiles = SanitizeSMILES(smiles);

            var sb = new StringBuilder()
                .Append("https://pubchem.ncbi.nlm.nih.gov/rest/pug/compound/smiles/")
                .Append(smiles)
                .Append("/property/");

            bool isFirst = true;
            foreach (string property in properties)
            {
                if (isFirst)
                    isFirst = false;
                else
                    sb.Append(',');

                sb.Append(property);
            }

            return sb.Append("/").Append(format).ToString();
        }

        public static string SmilesProperties(string smiles, params string[] properties)
            => SmilesProperties(smiles, (IEnumerable<string>)properties);

        public static string SmilesDescription(string smiles, PubChemFormat format = PubChemFormat.JSON)
        {
            smiles = SanitizeSMILES(smiles);

            return new StringBuilder()
                .Append("https://pubchem.ncbi.nlm.nih.gov/rest/pug/compound/smiles/")
                .Append(smiles)
                .Append("/description/")
                .Append(format)
                .ToString();
        }
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

    [Serializable]
    public class PubChemDescriptionResponse
    {
        public InformationList InformationList;
    }

    [Serializable]
    public class InformationList
    {
        public List<Information> Information;
    }

    [Serializable]
    public class Information
    {
        public int CID;
        public string Title;
    }
}
