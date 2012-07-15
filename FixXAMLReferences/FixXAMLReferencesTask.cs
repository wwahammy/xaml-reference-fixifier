using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;



using System.Xml.Linq;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using ProcessorArchitecture = System.Reflection.ProcessorArchitecture;

namespace FixXAMLReferences
{
    public class FixXAMLReferencesTask : Task
    {
        
        [Required]
        public ITaskItem[] Pages { get; set; }

        [Required]
        public ITaskItem[] References { get; set; }

        [Required]
        public string[] AssemblySearchPaths { get; set; }

        private AssemblyName[] ActualRefs { get; set; }

        public override bool Execute()
        {
            ActualRefs = GetProperReferences().ToArray();

            foreach (var p in Pages)
            {
              HandlePage(p);
            }

            return true;
        }

        private bool HandlePage(ITaskItem p)
        {
            var fullPath = p.GetMetadata("FullPath");
            var l = XElement.Load(fullPath);

            foreach (var rd in l.Descendants("{http://schemas.microsoft.com/winfx/2006/xaml/presentation}ResourceDictionary")
                .Where(e => e.Attribute("{http://schemas.microsoft.com/winfx/2006/xaml/presentation}Source") != null))
            {
                HandleResourceDictionary(rd);
            }

            return true;
        }

        private bool HandleResourceDictionary(XElement rd)
        {

            var currentSource = rd.Attribute("{http://schemas.microsoft.com/winfx/2006/xaml/presentation}Source");

            var p = GetValidPackUri(currentSource.Value);
            
            if (p.PackUriAuthority == PackUriAuthority.Application)
            {
                var firstAssm = ActualRefs.FirstOrDefault(i => AreWeReferringToTheSameAssembly(p.ReferencedAssembly, i));
                if (firstAssm != null)
                {
                    p.ReferencedAssembly = firstAssm;
                    currentSource.Value = p.ToString();
                }
            }

            return false;
        }

  

        

        private PackUri GetValidPackUri(string uriInput)
        {
            try
            {
                return new PackUri(uriInput);
            }
            catch (UriFormatException)
            {
                //we're going to add the beginning to it
                return new PackUri("pack://application:,,," + uriInput);
            }
            
        }


        private IEnumerable<AssemblyName> GetProperReferences()
        {

            return References.Select(GetAssemblyFromTaskItem);
        }



        /// <summary>
        /// Given a task item, we try to find the assembly in the following order:
        /// <list type="number">
        /// <item>
        /// <description>use the hint path</description>
        /// </item>
        /// <item>
        /// <description>in the AssemblySearchPaths</description>
        /// </item>
        /// <item>
        /// <description>the GAC</description>
        /// </item>
        /// </list>
        /// If multiple items are returned from the GAC, we get the latest one
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private AssemblyName GetAssemblyFromTaskItem(ITaskItem i)
        {
       


            //assembly we want
            var assmWeWant = new AssemblyName(i.ItemSpec);
            
            var ret = AssemblyName.GetAssemblyName(Path.Combine(BuildEngine3.ProjectFileOfTaskNode, i.GetMetadata("HintPath"))); 
            
            if (ret != null && AreWeReferringToTheSameAssembly(assmWeWant, ret))
                return ret;

            // find it in the search paths
            ret =
                AssemblySearchPaths.SelectMany(asp => Directory.EnumerateFiles(asp, "*.dll", SearchOption.AllDirectories)).Select(
                    GetAssemblyNameForPath).FirstOrDefault(an => an != null && AreWeReferringToTheSameAssembly(assmWeWant, ret));

            
                return ret;

            //To The GAC!!!
           // return AssemblyCacheEnum.GetAssemblyStrongNames(assmWeWant).Select(s => new AssemblyName(s)).OrderBy(n => n.Version).FirstOrDefault();
           
            

        }


        private static AssemblyName GetAssemblyNameForPath(string path)
        {
            try
            {
                return AssemblyName.GetAssemblyName(path);
            }
            catch (Exception)
            {

                return null;
            }
        }

        private bool AreWeReferringToTheSameAssembly(AssemblyName simple, AssemblyName fullyQualified)
        {
            var result = simple.Name == fullyQualified.Name;
            if (simple.Version != null)
                result &= simple.Version <= fullyQualified.Version;
            if (simple.ProcessorArchitecture != ProcessorArchitecture.None)
            {
                result &= simple.ProcessorArchitecture == fullyQualified.ProcessorArchitecture;
            }

            if (simple.CultureInfo != null)
            {
                result &= simple.CultureInfo.Equals(fullyQualified.CultureInfo);
            }

            if (simple.GetPublicKeyToken() != null)
            {
                result &= simple.GetPublicKeyToken().SequenceEqual(fullyQualified.GetPublicKeyToken());
            }


            return result;
        }

    }
}
