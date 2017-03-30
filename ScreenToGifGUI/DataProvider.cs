using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ScreenToGifGUI
{
    public class DataProvider
    {
        public Keys[] GetValidValuesFromKeys()
        {
            Array a = Enum.GetValues(typeof(Keys));
            Keys[] result = (from item in a.Cast<Keys>()
                 where Regex.IsMatch(item.ToString(), "^[a-zA-Z]$")
                 select item).ToArray();
            return result;
        }
    }
}
