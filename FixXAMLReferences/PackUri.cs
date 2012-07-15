using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using CoApp.Toolkit.Extensions;

namespace FixXAMLReferences
{
    class PackUri : Uri
    {

        private AssemblyName _referencedAssembly;
        public PackUri(string uriString) : base(uriString)
        {
        }

        public PackUri(string uriString, bool dontEscape) : base(uriString, dontEscape)
        {
        }

        public PackUri(string uriString, UriKind uriKind) : base(uriString, uriKind)
        {
        }

        public PackUri(Uri baseUri, string relativeUri) : base(baseUri, relativeUri)
        {
        }

        public PackUri(Uri baseUri, string relativeUri, bool dontEscape) : base(baseUri, relativeUri, dontEscape)
        {
        }

        public PackUri(Uri baseUri, Uri relativeUri) : base(baseUri, relativeUri)
        {
        }

        protected PackUri(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }

        public AssemblyName ReferencedAssembly { 
            get { return _referencedAssembly ?? (_referencedAssembly = GetAssemblyNameFromLocalPath()); }

            set { _referencedAssembly = value; }
        }

        private AssemblyName GetAssemblyNameFromLocalPath()
        {
            var paths = LocalPath.Split(';');
            if (paths.Length == 1)
                return null;
            var theRefParts = paths.TakeAllBut(1).ToArray();
            //remove the / on the first one
            theRefParts[0] = theRefParts[0].TrimStart('/');


            var getOutput = String.Join(",", theRefParts);

            return new AssemblyName(getOutput);
        }

 


        public static PackUri Create(string input)
        {
            try
            {
                return new PackUri(input);
            }
            catch (UriFormatException)
            {

                return new PackUri(input, UriKind.Relative);
            }
        }

        public PackUriAuthority PackUriAuthority { 
            get
            {
                
                if (Authority == "application:,,," || String.IsNullOrEmpty(Authority))
                {
                    return PackUriAuthority.Application;
                    
                }
                else if (Authority == "siteoforigin:,,,")
                {
                    return PackUriAuthority.SiteOfOrigin;
                }
                else
                {
                    return PackUriAuthority.Invalid;
                }
            }
        }

        public override string ToString()
        {
            if (PackUriAuthority == PackUriAuthority.Invalid || _referencedAssembly == null)
                return base.ToString();

            return Scheme + Authority + CreatePackUriAssemblyNameToString() + LocalPath.Split(';').Last();
        }



        private string CreatePackUriAssemblyNameToString()
        {
            return _referencedAssembly.ToString().Replace(", ", ";");
        }


        
    }

    public enum PackUriAuthority
    {
        Invalid,
        Application,
        SiteOfOrigin
    }
    
    
}
