
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanoDLP_Browser
{

    public class NanoDLPStatus
    {
        public bool AutoShutdown;
        public double Camera;
        public bool Cast;
        public bool Covered;
        public bool ForceStop;
        public bool Halted;
        public double LampHours;
        
        public double LayerID;
        public double LayerSinceStart;
        public double LayerStartTime;
        public double LayerTime;
        public double LayersCount;
        public double Layer;

        public bool Panicked;
        public double PanicRow;

        public string Path;

        public bool Paused;
        public double PlateHeight;
        public bool PlateID;
        public double PrevLayerTime;
        public bool Printing;
        public double ResumeID;

        public double SlicingPlateID;
        public double StartAfterSlice;
        public double State;

        public double Version;

        public string Wifi;
        public string disk;
        public string mem;
        public string proc;
        public string proc_numb;
        public string temp;
        public string uptime;

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
