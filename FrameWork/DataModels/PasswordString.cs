using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace FrameWork.DataModels
{
    public class PasswordString: object
    {
        private SecureString _password;

        public SecureString Password
        {
            get
            {
                if(_password == null)
                {
                    _password = new SecureString();
                }
                return _password;
            }
            set
            {
                if (_password != null)
                {
                    _password.Dispose();
                }
                _password = value;
            }
        }

        public int Length
        {
            get
            {
                return _password.Length;
            }
        }

        public bool isEmpty
        {
            get
            {
                return Password.Length > 0;
            }
        }

        public PasswordString(): this(new SecureString()) { }

        public PasswordString(SecureString password)
        {
            _password = password;
        }

        public PasswordString(string unsecureString)
        {
            _password = new SecureString();
            foreach(char c in unsecureString.ToCharArray())
                _password.AppendChar(c);
        }

        public override unsafe bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if(_password.Equals(obj))
                return true;

            var compareTo = (obj as PasswordString).Password;
            if(_password.Length != compareTo.Length)
                return false;

            IntPtr unsecurePointerThis = IntPtr.Zero;
            IntPtr unsecurePointerTo = IntPtr.Zero;
            GCHandle gch1 = new GCHandle();
            GCHandle gch2 = new GCHandle();
            bool result = false;
            RuntimeHelpers.ExecuteCodeWithGuaranteedCleanup(
                delegate
                {
                    string unsecurePasswordThis = new string('\0', _password.Length);
                    string unsecurePasswordTo = new string('\0', compareTo.Length);
                    gch1 = GCHandle.Alloc(unsecurePasswordThis, GCHandleType.Pinned);
                    gch2 = GCHandle.Alloc(unsecurePasswordTo, GCHandleType.Pinned);
                    try
                    {
                        unsecurePointerThis = Marshal.SecureStringToGlobalAllocUnicode(_password);
                        unsecurePointerTo = Marshal.SecureStringToGlobalAllocUnicode(compareTo);
                        unsecurePasswordThis = Marshal.PtrToStringUni(unsecurePointerThis);
                        unsecurePasswordTo = Marshal.PtrToStringUni(unsecurePointerTo);
                        result = unsecurePasswordThis.Equals(unsecurePasswordTo);
                    }
                    finally
                    {
                        Marshal.ZeroFreeGlobalAllocUnicode(unsecurePointerThis);
                        Marshal.ZeroFreeGlobalAllocUnicode(unsecurePointerTo);
                        var pInsecurePassword = (char*)gch1.AddrOfPinnedObject();
                        for (int index = 0; index < _password.Length; index++)
                        {
                            pInsecurePassword[index] = '\0';
                        }
                        var pInsecurePassword1 = (char*)gch2.AddrOfPinnedObject();
                        for (int index = 0; index < _password.Length; index++)
                        {
                            pInsecurePassword1[index] = '\0';
                        }
                        gch1.Free();
                        gch2.Free();
                    }
                }, delegate
                {
                    if (gch1.IsAllocated)
                    {
                        var pInsecurePassword = (char*)gch1.AddrOfPinnedObject();
                        for (int index = 0; index < _password.Length; index++)
                        {
                            pInsecurePassword[index] = '\0';
                        }
                        gch1.Free();
                    }
                    if (gch2.IsAllocated)
                    {
                        var pInsecurePassword = (char*)gch2.AddrOfPinnedObject();
                        for (int index = 0; index < _password.Length; index++)
                        {
                            pInsecurePassword[index] = '\0';
                        }
                        gch2.Free();
                    }
                }, null
            );
            return result;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
