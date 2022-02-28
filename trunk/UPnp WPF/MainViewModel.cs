
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanoDLP_Browser
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Windows;
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
        public bool visibleStop { get; set; }

        [XmlIgnore] 
        public bool visibleMove { get; set; }
        [XmlIgnore]
        public bool Enable { get; set; }
        [XmlIgnore]
        public bool ManualAdd { get; set; }
        [XmlIgnore]
        public Visibility getMoveVisibility
        {
            get{
                if (!visibleMove)
                {
                    return Visibility.Hidden;
                }
                else {
                    return Visibility.Visible;
                }
            }
        }
        [XmlIgnore]
        public int getMoveSize
        {
            get
            {
                if (!visibleMove)
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
        public Visibility getStopVisibility
        {
            get
            {
                if (!visibleStop)
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
        public int getStopSize
        {
            get
            {
                if (!visibleStop)
                {
                    return 0;
                }
                else
                {
                    return 100;
                }
            }
        }
    }

}
