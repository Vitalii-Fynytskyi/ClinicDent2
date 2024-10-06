using ClinicDentClientCommon;
using ClinicDentClientCommon.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicDent2.MultiSelectComboBox
{
    public class MultiSelectComboBoxItemTooth:MultiSelectComboBoxItem
    {
        public static List<MultiSelectComboBoxItemTooth> AllTeeth { get; set; }
        static MultiSelectComboBoxItemTooth()
        {
            AllTeeth = SharedData.AllTeeth.Select(t => new MultiSelectComboBoxItemTooth(t)).ToList();
        }
        public Tooth Tooth { get; set; }
        public MultiSelectComboBoxItemTooth(Tooth toothToSet):base()
        {
            Tooth = toothToSet;
            Name=toothToSet.Id.ToString();
        }
    }
}
