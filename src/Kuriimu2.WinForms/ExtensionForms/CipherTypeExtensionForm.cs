﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Kryptography;
using Kryptography.AES;
using Kuriimu2.WinForms.ExtensionForms.Models;
using Kuriimu2.WinForms.Extensions;

namespace Kuriimu2.WinForms.ExtensionForms
{
    class CipherStreamFactory
    {
        private Func<Stream, ExtensionType, Stream> _createStreamDelegate;
        private ExtensionType _extensionType;

        public CipherStreamFactory(ExtensionType extensionType, Func<Stream, ExtensionType, Stream> createStreamAction)
        {
            _createStreamDelegate = createStreamAction;
            _extensionType = extensionType;
        }

        public Stream CreateCipherStream(Stream input)
        {
            return _createStreamDelegate(input, _extensionType);
        }
    }

    abstract class CipherTypeExtensionForm : TypeExtensionForm<CipherStreamFactory, bool>
    {
        protected abstract void ProcessCipher(CipherStreamFactory cipherStreamFactory, Stream input, Stream output);

        protected override string TypeExtensionName => "Cipher";

        protected override bool ProcessFile(CipherStreamFactory extensionType, string filePath)
        {
            Stream fileStream = null;
            Stream newFileStream = null;

            try
            {
                fileStream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite);
                newFileStream = File.Create(filePath + ".out");

                ProcessCipher(extensionType, fileStream, newFileStream);
            }
            catch
            {
                return false;
            }
            finally
            {
                fileStream?.Close();
                newFileStream?.Close();
            }

            return true;
        }

        protected override void FinalizeProcess(IList<(string, bool)> results, string rootDir)
        {
            var reportFilePath = Path.Combine(rootDir, "cipherReport.txt");

            // Write hashes to text file
            var reportFile = File.CreateText(reportFilePath);
            foreach (var result in results)
                reportFile.WriteLine($"{result.Item1}: {result.Item2}");

            reportFile.Close();

            // Report finish
            MessageBox.Show($"The results are written to '{reportFilePath}'.", "Done", MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        protected override IList<ExtensionType> LoadExtensionTypes()
        {
            return new List<ExtensionType>
            {
                new ExtensionType("Xor",true,
                    new ExtensionTypeParameter("Key",typeof(string))),
                new ExtensionType("Rot",true,
                    new ExtensionTypeParameter("Rotation",typeof(byte))),
                new ExtensionType("AES ECB",true,
                    new ExtensionTypeParameter("Key", typeof(string))),
                new ExtensionType("AES CBC",true,
                    new ExtensionTypeParameter("Key", typeof(string)),
                    new ExtensionTypeParameter("IV", typeof(string))),
                new ExtensionType("AES CTR",true,
                    new ExtensionTypeParameter("Key", typeof(string)),
                    new ExtensionTypeParameter("Ctr", typeof(string)),
                    new ExtensionTypeParameter("LECtr", typeof(bool))),
                new ExtensionType("AES XTS",true,
                    new ExtensionTypeParameter("Key", typeof(string)),
                    new ExtensionTypeParameter("SectorId", typeof(string)),
                    new ExtensionTypeParameter("AdvanceSector", typeof(bool)),
                    new ExtensionTypeParameter("LESectorId", typeof(bool)),
                    new ExtensionTypeParameter("SectorSize", typeof(int)))
            };
        }

        protected override CipherStreamFactory CreateExtensionType(ExtensionType selectedExtension)
        {
            return new CipherStreamFactory(selectedExtension, CreateExtensionTypeInternal);
        }

        private Stream CreateExtensionTypeInternal(Stream input, ExtensionType selectedExtension)
        {
            switch (selectedExtension.Name)
            {
                case "Xor":
                    return new XorStream(input,
                        selectedExtension.GetParameterValue<string>("Key").Hexlify());

                case "Rot":
                    return new RotStream(input,
                        selectedExtension.GetParameterValue<byte>("Rotation"));

                case "AES ECB":
                    return new EcbStream(input,
                        selectedExtension.GetParameterValue<string>("Key").Hexlify());

                case "AES CBC":
                    return new CbcStream(input,
                        selectedExtension.GetParameterValue<string>("Key").Hexlify(),
                        selectedExtension.GetParameterValue<string>("IV").Hexlify());

                case "AES CTR":
                    return new CtrStream(input,
                        selectedExtension.GetParameterValue<string>("Key").Hexlify(),
                        selectedExtension.GetParameterValue<string>("Ctr").Hexlify(),
                        selectedExtension.GetParameterValue<bool>("LECtr"));

                case "AES XTS":
                    return new XtsStream(input,
                        selectedExtension.GetParameterValue<string>("Key").Hexlify(),
                        selectedExtension.GetParameterValue<string>("SectorId").Hexlify(),
                        selectedExtension.GetParameterValue<bool>("AdvanceSector"),
                        selectedExtension.GetParameterValue<bool>("LESectorId"),
                        selectedExtension.GetParameterValue<int>("SectorSize"));

                // TODO: Plugin extensibility?
                // TODO: Add nintendo NCA stream stuff
                default:
                    return null;
            }
        }
    }
}
