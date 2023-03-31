using System.Security.Cryptography;
using System.Management;
using System.Text;

namespace Technologai.Identity
{
    internal class MachineIdentity
    {
        private string _fingerPrint = string.Empty;

        internal string Value
        {
            get
            {
                if (string.IsNullOrEmpty(_fingerPrint))
                {
                    _fingerPrint = GetHash(
                            "CPU >> " + cpuId() +
                            "\nBIOS >> " + biosId() +
                            "\nBASE >> " + baseId() +
                            "\nDISK >> " + diskId() +
                            "\nVIDEO >> " + videoId() +
                            "\nMAC >> " + macId()
                    );
                }
                return _fingerPrint;
            }
        }

        private string GetHash(string text)
        {   
            return GetHexString(MD5.Create().ComputeHash(new ASCIIEncoding().GetBytes(text)));
        }

        private string GetHexString(byte[] bt)
        {
            string s = string.Empty;
            for (int i = 0; i < bt.Length; i++)
            {
                byte b = bt[i];
                int n, n1, n2;
                n = (int)b;
                n1 = n & 15;
                n2 = (n >> 4) & 15;
                if (n2 > 9)
                    s += ((char)(n2 - 10 + (int)'A')).ToString();
                else
                    s += n2.ToString();
                if (n1 > 9)
                    s += ((char)(n1 - 10 + (int)'A')).ToString();
                else
                    s += n1.ToString();
                if ((i + 1) != bt.Length && (i + 1) % 2 == 0) s += "-";
            }
            return s;
        }

        //TODO: This is inefficient
        
        private string GetIdentifier(string wmiClass, string wmiProperty, string wmiMustBeTrue)
        {
            ManagementClass mc = new ManagementClass(wmiClass);
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                if (mo[wmiMustBeTrue].ToString() == "True")
                {
                    return Convert.ToString(mo[wmiProperty]) ?? String.Empty;
                }
            }
            return String.Empty;
        }
                
        private string GetIdentifier(string wmiClass, string wmiProperty)
        {

            ManagementClass mc = new ManagementClass(wmiClass);
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                return Convert.ToString(mo[wmiProperty]) ?? String.Empty;
            }
            return String.Empty;
        }

        private string cpuId()
        {
            //Uses first CPU identifier available in order of preference
            //Don't get all identifiers, as it is very time consuming
            string retVal = GetIdentifier("Win32_Processor", "UniqueId");
            if (retVal == "") //If no UniqueID, use ProcessorID
            {
                retVal = GetIdentifier("Win32_Processor", "ProcessorId");
                if (retVal == "") //If no ProcessorId, use Name
                {
                    retVal = GetIdentifier("Win32_Processor", "Name");
                    if (retVal == "") //If no Name, use Manufacturer
                    {
                        retVal = GetIdentifier("Win32_Processor", "Manufacturer");
                    }
                    //Add clock speed for extra security
                    retVal += GetIdentifier("Win32_Processor", "MaxClockSpeed");
                }
            }
            return retVal;
        }

        //BIOS Identifier
        private string biosId()
        {
            return GetIdentifier("Win32_BIOS", "Manufacturer")
            + GetIdentifier("Win32_BIOS", "SMBIOSBIOSVersion")
            + GetIdentifier("Win32_BIOS", "IdentificationCode")
            + GetIdentifier("Win32_BIOS", "SerialNumber")
            + GetIdentifier("Win32_BIOS", "ReleaseDate")
            + GetIdentifier("Win32_BIOS", "Version");
        }

        //Main physical hard drive ID
        private string diskId()
        {
            return GetIdentifier("Win32_DiskDrive", "Model")
            + GetIdentifier("Win32_DiskDrive", "Manufacturer")
            + GetIdentifier("Win32_DiskDrive", "Signature")
            + GetIdentifier("Win32_DiskDrive", "TotalHeads");
        }

        //Motherboard ID
        private string baseId()
        {
            return GetIdentifier("Win32_BaseBoard", "Model")
            + GetIdentifier("Win32_BaseBoard", "Manufacturer")
            + GetIdentifier("Win32_BaseBoard", "Name")
            + GetIdentifier("Win32_BaseBoard", "SerialNumber");
        }

        //Primary video controller ID
        private string videoId()
        {
            return GetIdentifier("Win32_VideoController", "DriverVersion")
            + GetIdentifier("Win32_VideoController", "Name");
        }

        //First enabled network card ID
        private string macId()
        {
            return GetIdentifier("Win32_NetworkAdapterConfiguration",
                    "MACAddress", "IPEnabled");
        }
    }

}

