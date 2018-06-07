using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace OfflineQueueExportRKSV
{
    public class DEP7ReceiptsOnly
    {
        [JsonProperty("Belege-Gruppe")]
        public List<ReceiptGroup> ReceiptGroups { get; set; } = new List<ReceiptGroup>();

        public class ReceiptGroup
        {
            private string ReceiptsFilename { get; set; } = null;

            private ReceiptGroup()
            {
                ReceiptsFilename = System.IO.Path.GetTempFileName();
            }

            ~ReceiptGroup()
            {
                if (!string.IsNullOrWhiteSpace(ReceiptsFilename))
                {
                    System.IO.File.Delete(ReceiptsFilename);
                }

            }

            [JsonProperty("Belege-kompakt")]
            public List<string> Receipts { get; set; } = new List<string>();


        }


    }
}
