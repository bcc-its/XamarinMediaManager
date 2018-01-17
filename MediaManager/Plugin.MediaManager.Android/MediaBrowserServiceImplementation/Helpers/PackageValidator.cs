using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Util;
using Java.IO;
using Java.Lang;
using Org.XmlPull.V1;
using Plugin.MediaManager.Abstractions;
using Process = Android.OS.Process;

namespace Plugin.MediaManager.MediaBrowserServiceImplementation.Helpers
{
    public class PackageValidator
    {
        private ILoggingService log;
        private Dictionary<string, List<CallerInfo>> validCertificates;

        public PackageValidator(Context ctx)
        {
            //validCertificates =
                ReadValidCertificates(
                    ctx.Resources.GetXml(Android.Resource.Xml.allowed_media_browser_callers)); //TODO: Set allowed media browser callers;
        }

        private Dictionary<string, List<CallerInfo>> ReadValidCertificates(IXmlResourceParser parser)
        {
            var validCertificates = new Dictionary<string, List<CallerInfo>>();
            try
            {
                var eventType = parser.Next();
                while (eventType != XmlPullParserNode.EndDocument)
                {
                    if (eventType == XmlPullParserNode.StartTag
                        && parser.Name.Equals("signing_certificate"))
                    {

                        var name = parser.GetAttributeValue(null, "name");
                        var packageName = parser.GetAttributeValue(null, "package");
                        var isRelease = parser.GetAttributeBooleanValue(null, "release", false);
                        var certificate = parser.NextText().Replace("\\s|\\n", "");

                        var info = new CallerInfo(name, packageName, isRelease);
                        var infos = new List<CallerInfo>();
                        if (!validCertificates.TryGetValue(certificate, out infos))
                        {
                            infos = new List<CallerInfo>();
                            validCertificates.Add(certificate, infos);
                        }
                        log.Debug(
                            $"Adding allowed caller: {info.Name}, package={info.PackageName}release={info.Release}certificate={certificate}");
                        infos.Add(info);
                    }
                    eventType = parser.Next();
                }
            }
            catch (XmlPullParserException ex) 
            {
                log.Error("Could not read allowed callers from XML.", ex);
            }
            catch (IOException ex)
            {
                log.Error("Could not read allowed callers from XML.", ex);
            }

            return validCertificates;
        }

        /**
         * @return false if the caller is not authorized to get data from this MediaBrowserService
         */
        public bool IsCallerAllowed(Context context, string callingPackage, int callingUid)
        {
            // Always allow calls from the framework, self app or development environment.
            if (Process.SystemUid == callingUid || Process.MyUid() == callingUid)
                return true;
            

            if (IsPlatformSigned(context, callingPackage))
                return true;
            

            var packageInfo = GetPackageInfo(context, callingPackage);
            if (packageInfo == null)
                return false;

            if (packageInfo.Signatures.Count != 1)
            {
                log.Warn("Caller does not have exactly one signature certificate!");
                return false;
            }

            var signature = Base64.EncodeTostring(
                packageInfo.Signatures[0].ToByteArray(), Base64.NoWrap);

            // Test for known signatures:
            var validCallers = validCertificates.GetValueOrDefault(signature);
            if (validCallers == null)
            {
                log.Debug($"Signature for caller {callingPackage} is not valid: \n {signature}");
                if (validCertificates.Any())
                {
                    log.Warn(
                        $"The list of valid certificates is empty. " +
                        $"Either your file res/xml/allowed_media_browser_callers.xml is empty or there was an error while reading it. Check previous log messages.");
                }
                return false;
            }

            // Check if the package name is valid for the certificate:
            var expectedPackages = new stringBuffer();
            foreach (var info in validCallers) {
                if (callingPackage.Equals(info.PackageName))
                {
                    log.Debug($"Valid caller: {info.Name} package={info.PackageName}release={info.Release}");
                    return true;
                }
                expectedPackages.Append(info.PackageName).Append(' ');
            }

            log.Info($"Caller has a valid certificate, but its package doesn't match any " +
                     $"expected package for the given certificate. Caller's package is {callingPackage}" +
                     $" Expected packages as defined in res/xml/allowed_media_browser_callers.xml are ({expectedPackages})." +
                     $" This caller's certificate is: \n {signature}");

            return false;
        }

        /**
         * @return true if the installed package signature matches the platform signature.
         */
        private bool IsPlatformSigned(Context context, string pkgName)
        {
            var platformPackageInfo = GetPackageInfo(context, "android");

            // Should never happen.
            if (platformPackageInfo == null || platformPackageInfo.Signatures == null
                || platformPackageInfo.Signatures.Count == 0)
            {
                return false;
            }

            var clientPackageInfo = GetPackageInfo(context, pkgName);

            return (clientPackageInfo != null && clientPackageInfo.Signatures != null
                    && clientPackageInfo.Signatures.Count > 0 &&
                    platformPackageInfo.Signatures[0].Equals(clientPackageInfo.Signatures[0]));
        }

        /**
         * @return {@link PackageInfo} for the package name or null if it's not found.
         */
        private PackageInfo GetPackageInfo(Context context, string pkgName)
        {
            try
            {
                var pm = context.PackageManager;
                return pm.GetPackageInfo(pkgName, PackageInfoFlags.Signatures);
            }
            catch (PackageManager.NameNotFoundException e)
            {
               log.Warn("Package manager can't find package: ", e);
            }
            return null;
        }

        internal class CallerInfo
        {
            public string Name { get; set; }
            public string PackageName { get; set; }
            public bool Release { get; set; }

            internal CallerInfo(string name, string packageName, bool release)
            {
                Name = name;
                PackageName = packageName;
                Release = release;
            }
        }
    }
}