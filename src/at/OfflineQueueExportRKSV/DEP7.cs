﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OfflineQueueExportRKSV
{
    public class DEP7
    {

        [JsonProperty("Belege-Gruppe")]
        public List<ReceiptGroup> ReceiptGroups { get; set; } = new List<ReceiptGroup>();


        public class ReceiptGroup
        {

            private string ReceiptsFilename { get; set; } = null;
            private string WarningsFilename { get; set; } = null;
            private string PayloadsFilename { get; set; } = null;

            private ReceiptGroup()
            {
                ReceiptsFilename = System.IO.Path.GetTempFileName();
                WarningsFilename = System.IO.Path.GetTempFileName();
                PayloadsFilename = System.IO.Path.GetTempFileName();
            }

            ~ReceiptGroup()
            {
                if (!string.IsNullOrWhiteSpace(ReceiptsFilename))
                {
                    System.IO.File.Delete(ReceiptsFilename);
                }

                if (!string.IsNullOrWhiteSpace(WarningsFilename))
                {
                    System.IO.File.Delete(WarningsFilename);
                }

                if (!string.IsNullOrWhiteSpace(PayloadsFilename))
                {
                    System.IO.File.Delete(PayloadsFilename);
                }
            }

            private void Append(Stream stream, string text, System.Text.Encoding encoder = null)
            {
                if (encoder == null) encoder = System.Text.Encoding.UTF8;

                var arr = encoder.GetBytes(text);
                stream.Write(arr, 0, arr.Length);
            }

            private void Load(IEnumerable<string> jwsLines)
            {

                //jwt-chain-check
                string lastJWS = null;
                string lastHash = null;
                string lastReceiptNumber = null;

                using (var ReceiptsStream =  new System.IO.FileStream(ReceiptsFilename, FileMode.Append, FileAccess.Write, FileShare.None))
                using (var WarningsStream = new System.IO.FileStream(WarningsFilename, FileMode.Append, FileAccess.Write, FileShare.None))
                using (var PayloadsStream = new System.IO.FileStream(PayloadsFilename, FileMode.Append, FileAccess.Write, FileShare.None))
                {
                    foreach (var jws in jwsLines)
                    {
                        try
                        {

                            string qr = fiskaltrust.ifPOS.Utilities.AT_RKSV_Signature_ToDEP(jws, true);
                            if (string.IsNullOrWhiteSpace(qr))
                            {
                                Append(WarningsStream, $"JWS {jws} cannot be converted to QR{Environment.NewLine}");
                                continue;
                            }

                            Append(PayloadsStream, $"{qr}{Environment.NewLine}");

                            string Revision = null;
                            string ZDA = null;
                            string CashBoxIdentification = null;
                            string ReceiptIdentification = null;
                            DateTime DateTimeIso;
                            decimal TurnOverNormal = 0.0m;
                            decimal TurnOverReduced1 = 0.0m;
                            decimal TurnOverReduced2 = 0.0m;
                            decimal TurnOverZero = 0.0m;
                            decimal TurnOverSpecial = 0.0m;
                            string TurnOverCounterBase64 = null;
                            string CertificateSerialNumberHex = null;
                            string PrevReceiptHashBase64 = null;
                            string ReceiptSignatureBase64 = null;

                            if (!fiskaltrust.ifPOS.Utilities.AT_RKSV_SplitReceipt(qr, out Revision, out ZDA, out CashBoxIdentification, out ReceiptIdentification, out DateTimeIso, out TurnOverNormal, out TurnOverReduced1, out TurnOverReduced2, out TurnOverZero, out TurnOverSpecial, out TurnOverCounterBase64, out CertificateSerialNumberHex, out PrevReceiptHashBase64, out ReceiptSignatureBase64))
                            {
                                Append(WarningsStream, $"QR {qr} cannot be splitted{Environment.NewLine}");
                                continue;
                            }

                            if (lastJWS == jws)
                            {
                                //already processed entry
                                continue;
                            }

                            if (CashBoxIdentification != CashboxIdentification)
                            {
                                Append(WarningsStream, $"Cashboxidentification {CashBoxIdentification} doesnt match queue {CashboxIdentification}{Environment.NewLine}");
                                continue;
                            }

                            //TODO turnover counter
                            //TODO checksignature

                            if (lastHash != null)
                            {
                                //check for last hash
                                if (lastHash != Convert.ToBase64String(fiskaltrust.ifPOS.Utilities.FromBase64urlString(PrevReceiptHashBase64)))
                                {
                                    Append(WarningsStream, $"Chain broken between {lastReceiptNumber} and {ReceiptIdentification}{Environment.NewLine}");
                                }
                            }
                            else
                            {
                                Append(WarningsStream, $"First receipt {ReceiptIdentification}{Environment.NewLine}");
                            }
                            lastHash = fiskaltrust.ifPOS.Utilities.AT_RKSV_CreateReceiptHashBase64(jws);

                            lastJWS = jws;
                            lastReceiptNumber = ReceiptIdentification;

                            Append(ReceiptsStream, $"{jws}{Environment.NewLine}");
                        }
                        catch (Exception x)
                        {
                            if (string.IsNullOrWhiteSpace(lastReceiptNumber))
                            {
                                Append(WarningsStream, $"Exception before first line: {x.Message}{Environment.NewLine}");
                            }
                            else
                            {
                                Append(WarningsStream, $"Exception after {lastReceiptNumber}: {x.Message}{Environment.NewLine}");
                            }
                        }

                    }

                    Append(WarningsStream, $"Last receipt {lastReceiptNumber}{Environment.NewLine}");

                    ReceiptsStream.Flush();
                    WarningsStream.Flush();
                    PayloadsStream.Flush();
                }
            }


            public static ReceiptGroup Create(Guid queueId, string cashboxIdentification, string cashboxKeyBase64, string certificateBase64, IEnumerable<string> jwsLines)
            {
                var receiptGroup = new ReceiptGroup
                {
                    QueueId = queueId,
                    CashboxIdentification = cashboxIdentification,
                    CashboxKeyBase64 = cashboxKeyBase64,
                    CertificateBase64 = certificateBase64
                };

                receiptGroup.Load(jwsLines);
                return receiptGroup;
            }

            public static ReceiptGroup Create(Guid queueId, string certificateBase64)
            {
                return new ReceiptGroup
                {
                    QueueId = queueId,
                    CertificateBase64 = certificateBase64
                };
            }

            [JsonIgnore]
            internal Guid QueueId { get; set; }

            [JsonProperty("Kassenidentifikationsnummer")]
            public string CashboxIdentification { get; set; }

            [JsonProperty("AES-Key")]
            public string CashboxKeyBase64 { get; set; }

            [JsonProperty("Signaturzertifikat")]
            public string CertificateBase64 { get; private set; }

            [JsonProperty("Zertifizierungsstellen")]
            public string[] CertificateAuthority { get; private set; }

            [JsonProperty("Belege-kompakt")]
            public IEnumerable<string> Receipts => System.IO.File.ReadLines(ReceiptsFilename);

            [JsonProperty("Belege-warnungen")]
            public IEnumerable<string> Warnings => System.IO.File.ReadLines(WarningsFilename);

            [JsonProperty("Belege-payload")]
            public IEnumerable<string> Payloads => System.IO.File.ReadLines(PayloadsFilename);
        }

    }

}
