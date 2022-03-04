using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NanoDLP_Browser
{
    public class Dtos : ObservableCollection<Dto>
    {
        public Dtos()
        {
            if ((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue))
            {

                this.Add(new Dto()
                {
                    Name = "手動追加",
                    URI = "http://127.0.0.1/",
                    Discription = "hogehoge",
                    ManualAdd = true,
                    Printing = false,

                });
                this.Add(new Dto()
                {
                    Name = "手動追加 印刷中",
                    URI = "http://127.0.0.1/",
                    Discription = "hogehoge",
                    ManualAdd = true,
                    Printing = true,
                });
                this.Add(new Dto()
                {
                    Name = "UPnp",
                    URI = "http://127.0.0.1/",
                    Discription = "ホゲホゲ",
                    ManualAdd = false,
                    Printing = false,
                });
                this.Add(new Dto()
                {
                    Name = "UPnP印刷中",
                    URI = "http://127.0.0.1/",
                    Discription = "ほげほげー",
                    ManualAdd = false,
                    Printing = true,
                });
 

            }

        }
    }
}