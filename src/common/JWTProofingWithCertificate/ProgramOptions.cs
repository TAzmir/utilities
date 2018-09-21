﻿using CommandLine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace JWTProofingWithCertificate
{
    internal class ProgramOptions
    {
        [Option(longName: "certificatepath", Required = true, HelpText = "Path of the certificate file")]
        public string CertificatePath { get; set; }

        [Option(longName: "jwt", Required = true, HelpText = "JWT string to be proofed")]
        public string JWT { get; set; }

        public const int MaxParamValueLength = 8 * 1024;

        public static ProgramOptions GetOptionsFromCommandLine(string[] args)
        {

            var option = new ProgramOptions();
            Parser.Default.ParseArguments<ProgramOptions>(args)
              .WithParsed(opts => { option = opts; })
              .WithNotParsed((errs) =>
              {
                  option = GetProgramOptionsFromReadLineLoop();
              });
            return option;
        }

        private static string ReadLine()
        {
            Stream inputStream = Console.OpenStandardInput(MaxParamValueLength);
            byte[] bytes = new byte[MaxParamValueLength];
            int outputLength = inputStream.Read(bytes, 0, MaxParamValueLength);
            char[] chars = Encoding.UTF8.GetChars(bytes, 0, outputLength);
            var result = new string(chars);
            if (result.EndsWith(Environment.NewLine))
            {
                result = result.Substring(0, result.Length - Environment.NewLine.Length);
            }
            return result;
        }

        public static ProgramOptions GetProgramOptionsFromReadLineLoop()
        {
            var option = new ProgramOptions();
            var properties = typeof(ProgramOptions).GetProperties();

            foreach (var property in properties)
            {
                OptionAttribute attr = property.GetCustomAttributes(typeof(OptionAttribute), true).FirstOrDefault() as OptionAttribute;
                if (attr != null)
                {
                    string value = "";
                    do
                    {
                        if (attr.Default != null)
                            Console.Write($"{attr.LongName} ({attr.Default}):");
                        else
                            Console.Write($"{attr.LongName}:");

                        value = ReadLine();
                        if (string.IsNullOrWhiteSpace(value) && attr.Default != null)
                        {
                            Console.Error.WriteLine($"No value given for {attr.LongName}. Using Default Value: {attr.Default}");
                            value = attr.Default.ToString();
                        }

                        if (string.IsNullOrEmpty(value) && attr.Required)
                        {
                            Console.Error.WriteLine($"Error. Please provide a value for {attr.LongName}.");
                        }

                        if (!string.IsNullOrEmpty(value))
                        {
                            property.SetValue(option, TypeDescriptor.GetConverter(property.PropertyType).ConvertFromInvariantString(value));
                        }

                    } while (value == string.Empty && attr.Required);
                }
            }
            return option;
        }

    }
}
