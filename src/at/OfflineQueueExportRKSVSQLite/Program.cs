using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OfflineQueueExportRKSV
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = ProgramOptions.GetOptionsFromCommandLine(args);

            Dictionary<string, object> configuration = new Dictionary<string, object>();
            //encrypted connection string => connectionstring
            configuration.Add("servicefolder", options.ServiceFolder);
            

            var storage = new fiskaltrust.service.storage(options.QueueId, configuration);

            var dep7 = new DEP7();
            dep7.ReceiptGroups.Add(DEP7.ReceiptGroup.Create(options.QueueId, options.CashboxIdentification, options.CashBoxKeyBase64, options.CertificateBase64, storage.JournalATTableByTimeStamp().OrderBy(j => j.TimeStamp).Select(j => string.Concat(j.JWSHeaderBase64url, ".", j.JWSPayloadBase64url, ".", j.JWSSignatureBase64url))));

            using (var fs = new System.IO.FileStream(options.OutputFilename, System.IO.FileMode.Create))
            {
                using (var sw = new System.IO.StreamWriter(fs))
                using (var jw = new JsonTextWriter(sw))
                {

                    var serializer = new JsonSerializer();
                    serializer.Serialize(jw, dep7);
                }
            }

            decimal TurnoverTotal = 0.0m;
            byte[] CashBoxKeyBytes = Convert.FromBase64String(options.CashBoxKeyBase64);

            using (var fs = new System.IO.FileStream($"{options.OutputFilename}.turnover", System.IO.FileMode.Create))
            using (var sw = new System.IO.StreamWriter(fs, Encoding.UTF8))
            {
                sw.WriteLine("ReceiptIdentification;LastTotal;Normal;Reduced1;Reduced2;Zero;Special;Sum(Parts);LastTotal+Sum(Parts);DecodedTotal;Error");

                foreach (var item in dep7.ReceiptGroups[0].Receipts)
                {
                    string Revision = null;
                    string ZDA = null;
                    string CashBoxIdentification = null;
                    string ReceiptIdentification = null;
                    DateTime DateTimeIso = DateTime.MinValue;
                    decimal TurnoverNormal = 0.0m;
                    decimal TurnoverReduced1 = 0.0m;
                    decimal TurnoverReduced2 = 0.0m;
                    decimal TurnoverZero = 0.0m;
                    decimal TurnoverSpecial = 0.0m;
                    //decimal TurnoverTotalSum = 0.0m;

                    decimal TurnoverTotalDecoded = 0.0m;
                    string TurnoverTotalBase64 = null;

                    string CertificateSerialnumberHex = null;
                    string PrevReceiptHashBase64 = null;
                    string ReceiptSignatureBase64 = null;

                    string qr = fiskaltrust.ifPOS.Utilities.AT_RKSV_Signature_ToDEP(item, true);
                    if (!string.IsNullOrWhiteSpace(item) && fiskaltrust.ifPOS.Utilities.AT_RKSV_SplitReceipt(qr, out Revision, out ZDA, out CashBoxIdentification, out ReceiptIdentification, out DateTimeIso, out TurnoverNormal, out TurnoverReduced1, out TurnoverReduced2, out TurnoverZero, out TurnoverSpecial, out TurnoverTotalBase64, out CertificateSerialnumberHex, out PrevReceiptHashBase64, out ReceiptSignatureBase64))
                    {
                        TurnoverTotalDecoded = fiskaltrust.ifPOS.Utilities.AT_RKSV_DecryptTurnoverSum(CashBoxIdentification, ReceiptIdentification, CashBoxKeyBytes, Convert.FromBase64String(TurnoverTotalBase64));
                        decimal PartsSum = TurnoverNormal + TurnoverReduced1 + TurnoverReduced2 + TurnoverZero + TurnoverSpecial;
                        sw.WriteLine($"{ReceiptIdentification};{TurnoverTotal};{TurnoverNormal};{TurnoverReduced1};{TurnoverReduced2};{TurnoverZero};{TurnoverSpecial};{TurnoverNormal};{PartsSum};{TurnoverTotal + PartsSum};{TurnoverTotalDecoded};{TurnoverTotalDecoded - (TurnoverTotal + PartsSum)}");
                        //Console.WriteLine($"{ReceiptIdentification};{TurnoverTotal};{TurnoverNormal};{TurnoverReduced1};{TurnoverReduced2};{TurnoverZero};{TurnoverSpecial};{TurnoverNormal};{PartsSum};{TurnoverTotal + PartsSum};{TurnoverTotalDecoded};{TurnoverTotalDecoded - (TurnoverTotal + PartsSum)}");
                        TurnoverTotal = TurnoverTotalDecoded;
                    }
                    else
                    {
                        throw new ArgumentException();
                    }

                }


            }


        }
    }
}
