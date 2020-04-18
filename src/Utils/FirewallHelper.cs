using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetFwTypeLib;

namespace PowerPing.Utils {
    /// 

    /// Allows basic access to the windows firewall API.
    /// This can be used to add an exception to the windows firewall
    /// exceptions list, so that our programs can continue to run merrily
    /// even when nasty windows firewall is running.
    ///
    /// Please note: It is not enforced here, but it might be a good idea
    /// to actually prompt the user before messing with their firewall settings,
    /// just as a matter of politeness.
    /// 

    /// 
    /// To allow the installers to authorize idiom products to work through
    /// the Windows Firewall.
    /// 
    public class FirewallHelper {
        #region Variables
        /// 

        /// Hooray! Singleton access.
        /// 

        private static FirewallHelper instance = null;

        /// 

        /// Interface to the firewall manager COM object
        /// 

        private INetFwMgr fwMgr = null;
        #endregion
        #region Properties
        /// 

        /// Singleton access to the firewallhelper object.
        /// Threadsafe.
        /// 

        public static FirewallHelper Instance {
            get {
                lock (typeof(FirewallHelper)) {
                    if (instance == null)
                        instance = new FirewallHelper();
                    return instance;
                }
            }
        }
        #endregion
        #region Constructivat0r
        /// 

        /// Private Constructor.  If this fails, HasFirewall will return
        /// false;
        /// 

        private FirewallHelper()
        {
            // Get the type of HNetCfg.FwMgr, or null if an error occurred
            Type fwMgrType = Type.GetTypeFromProgID("HNetCfg.FwMgr", false);

            // Assume failed.
            fwMgr = null;

            if (fwMgrType != null) {
                try {
                    fwMgr = (INetFwMgr)Activator.CreateInstance(fwMgrType);
                }
                // In all other circumnstances, fwMgr is null.
                catch (ArgumentException) { }
                catch (NotSupportedException) { }
                catch (System.Reflection.TargetInvocationException) { }
                catch (MissingMethodException) { }
                catch (MethodAccessException) { }
                catch (MemberAccessException) { }
                catch (TypeLoadException) { }
            }
        }
        #endregion
        #region Helper Methods
        /// 

        /// Gets whether or not the firewall is installed on this computer.
        /// 

        /// 
        public bool IsFirewallInstalled {
            get {
                if (fwMgr != null &&
                      fwMgr.LocalPolicy != null &&
                      fwMgr.LocalPolicy.CurrentProfile != null)
                    return true;
                else
                    return false;
            }
        }

        /// 

        /// Returns whether or not the firewall is enabled.
        /// If the firewall is not installed, this returns false.
        /// 

        public bool IsFirewallEnabled {
            get {
                if (IsFirewallInstalled && fwMgr.LocalPolicy.CurrentProfile.FirewallEnabled)
                    return true;
                else
                    return false;
            }
        }

        /// 

        /// Returns whether or not the firewall allows Application "Exceptions".
        /// If the firewall is not installed, this returns false.
        /// 

        /// 
        /// Added to allow access to this metho
        /// 
        public bool AppAuthorizationsAllowed {
            get {
                if (IsFirewallInstalled && !fwMgr.LocalPolicy.CurrentProfile.ExceptionsNotAllowed)
                    return true;
                else
                    return false;
            }
        }

        /// 

        /// Adds an application to the list of authorized applications.
        /// If the application is already authorized, does nothing.
        /// 

        /// 
        ///         The full path to the application executable.  This cannot
        ///         be blank, and cannot be a relative path.
        /// 
        /// 
        ///         This is the name of the application, purely for display
        ///         puposes in the Microsoft Security Center.
        /// 
        /// 
        ///         When applicationFullPath is null OR
        ///         When appName is null.
        /// 
        /// 
        ///         When applicationFullPath is blank OR
        ///         When appName is blank OR
        ///         applicationFullPath contains invalid path characters OR
        ///         applicationFullPath is not an absolute path
        /// 
        /// 
        ///         If the firewall is not installed OR
        ///         If the firewall does not allow specific application 'exceptions' OR
        ///         Due to an exception in COM this method could not create the
        ///         necessary COM types
        /// 
        /// 
        ///         If no file exists at the given applicationFullPath
        /// 
        public void GrantAuthorization(string applicationFullPath, string appName)
        {
            #region  Parameter checking
            if (applicationFullPath == null)
                throw new ArgumentNullException("applicationFullPath");
            if (appName == null)
                throw new ArgumentNullException("appName");
            if (applicationFullPath.Trim().Length == 0)
                throw new ArgumentException("applicationFullPath must not be blank");
            if (applicationFullPath.Trim().Length == 0)
                throw new ArgumentException("appName must not be blank");
            if (applicationFullPath.IndexOfAny(Path.InvalidPathChars) >= 0)
                throw new ArgumentException("applicationFullPath must not contain invalid path characters");
            if (!Path.IsPathRooted(applicationFullPath))
                throw new ArgumentException("applicationFullPath is not an absolute path");
            if (!File.Exists(applicationFullPath))
                throw new FileNotFoundException("File does not exist", applicationFullPath);
            // State checking
            if (!IsFirewallInstalled)
                throw new FirewallHelperException("Cannot grant authorization: Firewall is not installed.");
            if (!AppAuthorizationsAllowed)
                throw new FirewallHelperException("Application exemptions are not allowed.");
            #endregion

            if (!HasAuthorization(applicationFullPath)) {
                // Get the type of HNetCfg.FwMgr, or null if an error occurred
                Type authAppType = Type.GetTypeFromProgID("HNetCfg.FwAuthorizedApplication", false);

                // Assume failed.
                INetFwAuthorizedApplication appInfo = null;

                if (authAppType != null) {
                    try {
                        appInfo = (INetFwAuthorizedApplication)Activator.CreateInstance(authAppType);
                    }
                    // In all other circumnstances, appInfo is null.
                    catch (ArgumentException) { }
                    catch (NotSupportedException) { }
                    catch (System.Reflection.TargetInvocationException) { }
                    catch (MissingMethodException) { }
                    catch (MethodAccessException) { }
                    catch (MemberAccessException) { }
                    catch (TypeLoadException) { }
                }

                if (appInfo == null)
                    throw new FirewallHelperException("Could not grant authorization: can't create INetFwAuthorizedApplication instance.");

                appInfo.Name = appName;
                appInfo.ProcessImageFileName = applicationFullPath;
                // ...
                // Use defaults for other properties of the AuthorizedApplication COM object

                // Authorize this application
                fwMgr.LocalPolicy.CurrentProfile.AuthorizedApplications.Add(appInfo);
            }
            // otherwise it already has authorization so do nothing
        }
        /// 

        /// Removes an application to the list of authorized applications.
        /// Note that the specified application must exist or a FileNotFound
        /// exception will be thrown.
        /// If the specified application exists but does not current have
        /// authorization, this method will do nothing.
        /// 

        /// 
        ///         The full path to the application executable.  This cannot
        ///         be blank, and cannot be a relative path.
        /// 
        /// 
        ///         When applicationFullPath is null
        /// 
        /// 
        ///         When applicationFullPath is blank OR
        ///         applicationFullPath contains invalid path characters OR
        ///         applicationFullPath is not an absolute path
        /// 
        /// 
        ///         If the firewall is not installed.
        /// 
        /// 
        ///         If the specified application does not exist.
        /// 
        public void RemoveAuthorization(string applicationFullPath)
        {

            #region  Parameter checking
            if (applicationFullPath == null)
                throw new ArgumentNullException("applicationFullPath");
            if (applicationFullPath.Trim().Length == 0)
                throw new ArgumentException("applicationFullPath must not be blank");
            if (applicationFullPath.IndexOfAny(Path.InvalidPathChars) >= 0)
                throw new ArgumentException("applicationFullPath must not contain invalid path characters");
            if (!Path.IsPathRooted(applicationFullPath))
                throw new ArgumentException("applicationFullPath is not an absolute path");
            if (!File.Exists(applicationFullPath))
                throw new System.IO.FileNotFoundException("File does not exist", applicationFullPath);
            // State checking
            if (!IsFirewallInstalled)
                throw new FirewallHelperException("Cannot remove authorization: Firewall is not installed.");
            #endregion

            if (HasAuthorization(applicationFullPath)) {
                // Remove Authorization for this application
                fwMgr.LocalPolicy.CurrentProfile.AuthorizedApplications.Remove(applicationFullPath);
            }
            // otherwise it does not have authorization so do nothing
        }
        /// 

        /// Returns whether an application is in the list of authorized applications.
        /// Note if the file does not exist, this throws a FileNotFound exception.
        /// 

        /// 
        ///         The full path to the application executable.  This cannot
        ///         be blank, and cannot be a relative path.
        /// 
        /// 
        ///         The full path to the application executable.  This cannot
        ///         be blank, and cannot be a relative path.
        /// 
        /// 
        ///         When applicationFullPath is null
        /// 
        /// 
        ///         When applicationFullPath is blank OR
        ///         applicationFullPath contains invalid path characters OR
        ///         applicationFullPath is not an absolute path
        /// 
        /// 
        ///         If the firewall is not installed.
        /// 
        /// 
        ///         If the specified application does not exist.
        /// 
        public bool HasAuthorization(string applicationFullPath)
        {
            #region  Parameter checking
            if (applicationFullPath == null)
                throw new ArgumentNullException("applicationFullPath");
            if (applicationFullPath.Trim().Length == 0)
                throw new ArgumentException("applicationFullPath must not be blank");
            if (applicationFullPath.IndexOfAny(Path.InvalidPathChars) >= 0)
                throw new ArgumentException("applicationFullPath must not contain invalid path characters");
            if (!Path.IsPathRooted(applicationFullPath))
                throw new ArgumentException("applicationFullPath is not an absolute path");
            if (!File.Exists(applicationFullPath))
                throw new FileNotFoundException("File does not exist.", applicationFullPath);
            // State checking
            if (!IsFirewallInstalled)
                throw new FirewallHelperException("Cannot remove authorization: Firewall is not installed.");

            #endregion

            // Locate Authorization for this application
            foreach (string appName in GetAuthorizedAppPaths()) {
                // Paths on windows file systems are not case sensitive.
                if (appName.ToLower() == applicationFullPath.ToLower())
                    return true;
            }

            // Failed to locate the given app.
            return false;

        }

        /// 

        /// Retrieves a collection of paths to applications that are authorized.
        /// 

        /// 
        /// 
        ///         If the Firewall is not installed.
        ///   
        public ICollection GetAuthorizedAppPaths()
        {
            // State checking
            if (!IsFirewallInstalled)
                throw new FirewallHelperException("Cannot remove authorization: Firewall is not installed.");

            ArrayList list = new ArrayList();
            //  Collect the paths of all authorized applications
            foreach (INetFwAuthorizedApplication app in fwMgr.LocalPolicy.CurrentProfile.AuthorizedApplications)
                list.Add(app.ProcessImageFileName);

            return list;
        }
        #endregion
    }

    /// 

    /// Describes a FirewallHelperException.
    /// 

    /// 
    ///
    /// 
    public class FirewallHelperException : System.Exception {
        /// 

        /// Construct a new FirewallHelperException
        /// 

        /// 
        public FirewallHelperException(string message)
          : base(message)
        { }
    }
}
