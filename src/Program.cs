using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System;
using System.IO;
using System.Xml;

namespace updeps
{
    class Reference
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string ProjectFilePath { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Require at least two arguments");
                Environment.Exit(2);
            }

            FileInfo[] fileInfos = new FileInfo[args.Length];

            for (int i = 0; i < args.Length; i++)
            {
                FileInfo fileInfo = new FileInfo(args[i]);

                if (!fileInfo.Exists)
                {
                    Console.WriteLine(
                        "File {0} does not exists", fileInfo.FullName);

                    Environment.Exit(2);
                }

                fileInfos[i] = fileInfo;
            }

            string[] csprojFilePathes = fileInfos
                .Take(fileInfos.Length - 1)
                .Select(s =>
                {
                    if (s.Extension != ".csproj")
                    {
                        Console
                            .WriteLine("Only *.csproj files are supported.");

                        Environment.Exit(2);
                    }

                    return s.FullName;
                })
                .ToArray();


            string nuspecFilePath = fileInfos
                .Skip(fileInfos.Length - 1)
                .Take(1)
                .Select(s =>
                {
                    if (s.Extension != ".nuspec")
                    {
                        Console
                            .WriteLine("Last argument must be a .nuspec file.");

                        Environment.Exit(2);
                    }

                    return s.FullName;
                }).First();

          
            IEnumerable<Reference> references =
                GetReferences(csprojFilePathes);

            UpdateNuspecFile(nuspecFilePath, references);
            
            Environment.Exit(0);
        }

        private static void UpdateNuspecFile(
			string nuspecFilePath, 
			IEnumerable<Reference> references)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(nuspecFilePath);

            // TODO: work something out if you require targeting different frameworks 
            // Mc'Giver would be proud!
            var node = doc.SelectNodes("//*[local-name()='package']").Item(0);
            node = node.SelectNodes("//*[local-name()='metadata']").Item(0);
            node = node.SelectNodes("//*[local-name()='dependencies']").Item(0);
            node = node.SelectNodes("//*[local-name()='group']").Item(0);

            while (node.HasChildNodes)
            {
                node.RemoveChild(node.FirstChild);
            }

            foreach (var reference in references)
            {
                XmlElement elem = doc.CreateElement("dependency",
                    "http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd");

                elem.SetAttribute("id", reference.Name);
                elem.SetAttribute("version", reference.Version);
                elem.SetAttribute("exclude", "Build,Analyzers");
                node.AppendChild(elem);
            }

            doc.Save(nuspecFilePath);
        }

        static IEnumerable<Reference> GetReferences(
            string[] csprojFilePathes)
        {
            Dictionary<string, Reference> references =
                            new Dictionary<string, Reference>();

            foreach (string csprojFilePath in csprojFilePathes)
            {
                Console.WriteLine("Reading {0}", csprojFilePath);

                foreach (Reference reference in GetReferences(csprojFilePath))
                {
                    if (references.ContainsKey(reference.Name))
                    {
                        Reference referenceAdded = references[reference.Name];

                        if (referenceAdded.Version != reference.Version)
                        {
                            Console.WriteLine(
                                "  Failed {0} {1}",
                                reference.Version,
                                reference.Name);

                            Console.WriteLine(
                                "    Different versions of the same package reference.");

                            Console.WriteLine(
                                "    {0} in {1}",
                                referenceAdded.Version,
                                referenceAdded.ProjectFilePath);

                            Environment.Exit(1);
                        }
                        else
                        {
                            Console.WriteLine("  Skipped {0} {1}",
                               reference.Version, reference.Name);
                        }
                    }
                    else
                    {
                        Console.WriteLine("  Added {0} {1}",
                            reference.Version, reference.Name);

                        references.Add(reference.Name, reference);
                    }
                }
            }

            return references.Values;
        }

        static IEnumerable<Reference>
            GetReferences(string csprojFilePath)
        {
            XDocument doc = XDocument.Load(csprojFilePath);
            XElement project = doc.Element("Project");

            foreach (var itemGroup in project.Elements("ItemGroup"))
            {
                foreach (var packageReference in itemGroup
                    .Elements("PackageReference"))
                {
                    yield return new Reference
                    {
                        Name = packageReference.Attribute("Include").Value,
                        Version = packageReference.Attribute("Version").Value,
                        ProjectFilePath = csprojFilePath
                    };
                }
            }
        }
    }
}
