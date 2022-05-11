using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CryptoTrading.TQClass
{
    public class DataGridMapping
    {
        public DataGrid TQMainDataGrid { get; set; }

        public DataGridType DataGridType { get; set; }

        public List<ColumnSettingItem> ColumnSettingList { get; set; }
    }
}
