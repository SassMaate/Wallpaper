using SharpCompress.Archives;
using SharpCompress.Common;
using System.IO;
using SSDECT = Sucrose.Shared.Dependency.Enum.CompatibilityType;
using SSSHF = Sucrose.Shared.Space.Helper.Filing;

namespace Sucrose.Shared.SevenZip.Extension
{
    internal static class Zip
    {
        public static SSDECT Extract(string Archive, string Destination)
        {
            try
            {
                using IArchive Archiver = ArchiveFactory.OpenArchive(Archive);

                foreach (IArchiveEntry Entry in Archiver.Entries)
                {
                    if (!Entry.IsDirectory)
                    {
                        Entry.WriteToDirectory(Destination, new ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                }

                return SSDECT.Pass;
            }
            catch
            {
                return SSDECT.UnforeseenConsequences;
            }
        }

        public static SSDECT Compress(string Source, string Destination)
        {
            try
            {
                if (File.Exists(Destination))
                {
                    SSSHF.Delete(Destination);
                }

                using IWritableArchive Archiver = ArchiveFactory.CreateArchive(ArchiveType.SevenZip);

                foreach (string Record in Directory.GetFiles(Source))
                {
                    Archiver.AddEntry(Path.GetFileName(Record), Record);
                }

                Archiver.SaveTo(Destination, CompressionType.LZMA);

                return SSDECT.Pass;
            }
            catch
            {
                return SSDECT.UnforeseenConsequences;
            }
        }
    }
}