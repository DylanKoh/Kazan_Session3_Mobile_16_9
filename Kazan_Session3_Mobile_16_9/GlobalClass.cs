using System;
using System.Collections.Generic;
using System.Text;

namespace Kazan_Session3_Mobile_16_9
{
    public class GlobalClass
    {
        public class Asset
        {
            public long ID { get; set; }
            public string AssetSN { get; set; }
            public string AssetName { get; set; }
            public long DepartmentLocationID { get; set; }
            public long EmployeeID { get; set; }
            public long AssetGroupID { get; set; }
            public string Description { get; set; }
            public Nullable<System.DateTime> WarrantyDate { get; set; }
        }

        public partial class AssetOdometer
        {
            public long ID { get; set; }
            public long AssetID { get; set; }
            public System.DateTime ReadDate { get; set; }
            public long OdometerAmount { get; set; }

        }

        public class PMScheduleModel
        {
            public long ID { get; set; }
            public string Name { get; set; }
            public Nullable<long> PMScheduleTypeID { get; set; }

        }

        public class PMScheduleType
        {
            public long ID { get; set; }
            public string Name { get; set; }
        }

        public class PMTask
        {
            public long ID { get; set; }
            public long AssetID { get; set; }
            public long TaskID { get; set; }
            public long PMScheduleTypeID { get; set; }
            public Nullable<System.DateTime> ScheduleDate { get; set; }
            public Nullable<long> ScheduleKilometer { get; set; }
            public Nullable<bool> TaskDone { get; set; }
        }

        public partial class Task1
        {
            public long ID { get; set; }
            public string Name { get; set; }
        }


        public class CustomView
        {
            public int PMTaskID { get; set; }
            public string Asset { get; set; }
            public string TaskName { get; set; }
            public string TaskTypeAndValue { get; set; }
            public bool TaskDone { get; set; }
        }

    }
}
