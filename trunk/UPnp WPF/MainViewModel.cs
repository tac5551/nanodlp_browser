
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPnp_WPF
{
    using System.Collections.Generic;
    using System.Windows;
    using UPNPLib;

    public class Dto
    {
        public string Name { get; set; }
        public string URI { get; set; }
        public string UUID { get; set; }
        public string Discription { get; set; }

        public string ETA { get; set; }
        public string Remaining { get; set; }
        public string Height { get; set; }
        public string Layer { get; set; }
        public string Plate { get; set; }

        //public UPnPDevice Device { get; set; }
        public bool visibleStop { get; set; }
        public bool visibleMove { get; set; }
        public bool Enable { get; set; }
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
