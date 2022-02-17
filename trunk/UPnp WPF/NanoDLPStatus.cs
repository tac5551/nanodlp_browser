
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPnp_WPF
{

    public class NanoDLPStatus
    {
        public bool Printing;
        public string Path;
        public double LayersCount;
        public double LayerID;
        public double LayerStartTime;
        public double LayerTime;
        public double Layer;
        public double PlateHeight;

        public double getLayerTime
        {
            get
            {
                return LayerTime / 1000000000;
            }
        }
        public double getRemainingTime
        {
            get
            {
                return Math.Round((this.LayersCount - this.LayerID) * this.getLayerTime / 60);
            }
        }
        public double getTotalTime
        {
            get
            {
                return Math.Round(this.LayersCount * this.getLayerTime / 60);
            }
        }

        public double getCurrentHeight
        {
            get
            {
                return this.PlateHeight / this.LayersCount * this.LayerID;
            }
        }
        public string getETA
        {
            get
            {
                DateTime est = DateTime.Now;
                return est.AddMinutes(this.getRemainingTime).ToString("HH:mm");

            }
        }

    }
}
