
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanoDLP_Browser
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Drawing;
    using System.Windows;
    using System.Windows.Media;
    using System.Xml.Serialization;
    using UPNPLib;

    [Serializable]
    [XmlRoot("Dto")]
    public class Dto
    {
        public Dto() {
            LayerMax = 100;
            LayerNow = 0;
            Printing = false;
        }
        [XmlElement("name")]
        public string Name { get; set; }
        [XmlElement("URI")]
        public string URI { get; set; }
        [XmlElement("UUID")]
        public string UUID { get; set; }
        [XmlElement("Discription")]
        public string Discription { get; set; }
        [XmlIgnore]
        public string ETA { get; set; }

        [XmlIgnore] 
        public string Remaining { get; set; }
        [XmlIgnore]
        public string Height { get; set; }
        [XmlIgnore]
        public string Layer { get; set; }
        [XmlIgnore] 
        public string Plate { get; set; }
        [XmlIgnore] 
        public bool Printing { get; set; }
        [XmlIgnore]
        public int LayerNow { get; set; }
        [XmlIgnore]
        public int LayerMax { get; set; }
        //public UPnPDevice Device { get; set; }

        [XmlIgnore]
        public bool Enable { get; set; }
        [XmlIgnore]
        public bool ManualAdd { get; set; }
        [XmlIgnore]

        public bool visibleStop {
            get {
                if (Printing)
                {
                    return true;
                }
                else {
                    return false;
                }
             }
        }

        [XmlIgnore]
        public bool visibleMove
        {
            get
            {
                if (!Printing)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [XmlIgnore]
        public SolidColorBrush getBgColor
        {
            get
            {
                if (Printing)
                {
                    return new SolidColorBrush(Colors.Lavender);
                }
                else if (ManualAdd) {
                    return new SolidColorBrush(Colors.Ivory);
                }
                else
                {
                    return new SolidColorBrush(Colors.White);
                }
            }
        }

        [XmlIgnore]
        public Visibility getIdleButtonVisibility
        {
            get{
                if (Printing)
                {
                    return Visibility.Hidden;
                }
                else {
                    return Visibility.Visible;
                }
            }
        }
        [XmlIgnore]
        public int getIdleButtonSize
        {
            get
            {
                if (Printing)
                {
                    return 0;
                }
                else
                {
                    return 100;
                }
            }
        }
        [XmlIgnore]
        public Visibility getPrintingButtonVisibility
        {
            get
            {
                if (!Printing)
                {
                    return Visibility.Hidden;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
        }
        [XmlIgnore]
        public int getPrintingButtonSize
        {
            get
            {
                if (!Printing)
                {
                    return 0;
                }
                else
                {
                    return 100;
                }
            }
        }
        public Visibility getEditVisibility 
        { 
            get {
                if (ManualAdd)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Hidden;
                }
            } 
        }
    }

}
