using System.IO;
using System;
using System.Text;
using UnityEngine;

namespace Natrium
{
    public class CustomStream : MemoryStream
    {
        #region Writers
        public void Write(Events value)
        {
            byte[] bytes = BitConverter.GetBytes((int)value);
            Write(bytes, 0, bytes.Length);
        }

        public void Write(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Write(bytes, 0, bytes.Length);
        }

        public void Write(long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Write(bytes, 0, bytes.Length);
        }

        public void Write(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Write(bytes, 0, bytes.Length);
        }

        public void Write(double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Write(bytes, 0, bytes.Length);
        }

        public void Write(bool value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Write(bytes, 0, bytes.Length);
        }

        public void Write(string value)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(value + '\0');

            int stringLength = GetStringLength(bytes) + 1;

            if (stringLength < bytes.Length)
            {
                Debug.LogWarning("String truncated.");
                byte[] newBytes = new byte[stringLength];
                Array.Copy(bytes, 0, newBytes, 0, stringLength);
                Write(newBytes, 0, newBytes.Length);
            }
            else
                Write(bytes, 0, bytes.Length);
        }

        #endregion Writers

        #region Readers

        public bool Read(out Events value)
        {
            byte[] bytes = new byte[sizeof(Events)];
            int result = Read(bytes, 0, bytes.Length);

            value = (Events)BitConverter.ToInt32(bytes);

            bool boolResult = (result != 0) ? true : false;
            return boolResult;
        }

        public bool Read(out int value)
        {
            byte[] bytes = new byte[sizeof(int)];
            int result = Read(bytes, 0, bytes.Length);

            value = BitConverter.ToInt32(bytes);

            bool boolResult = (result != 0) ? true : false;
            return boolResult;
        }

        public bool Read(out long value)
        {
            byte[] bytes = new byte[sizeof(long)];
            int result = Read(bytes, 0, bytes.Length);

            value = BitConverter.ToInt64(bytes);

            bool boolResult = (result != 0) ? true : false;
            return boolResult;
        }

        public bool Read(out float value)
        {
            byte[] bytes = new byte[sizeof(float)];
            int result = Read(bytes, 0, bytes.Length);

            value = BitConverter.ToSingle(bytes);

            bool boolResult = (result != 0) ? true : false;
            return boolResult;
        }

        public bool Read(out double value)
        {
            byte[] bytes = new byte[sizeof(double)];
            int result = Read(bytes, 0, bytes.Length);

            value = BitConverter.ToDouble(bytes);

            bool boolResult = (result != 0) ? true : false;
            return boolResult;
        }

        public bool Read(out bool value)
        {
            byte[] bytes = new byte[sizeof(bool)];
            int result = Read(bytes, 0, bytes.Length);

            value = BitConverter.ToBoolean(bytes);

            bool boolResult = (result != 0) ? true : false;
            return boolResult;
        }

        public bool Read(out string value)
        {
            byte[] streamBytes = new byte[50];
            int result = Read(streamBytes, 0, streamBytes.Length);

            int stringLength = GetStringLength(streamBytes);

            byte[] bytes = new byte[stringLength];

            Array.Copy(streamBytes, 0, bytes, 0, stringLength);

            value = Encoding.ASCII.GetString(bytes);

            Position -= result;
            Position += stringLength + 1;

            bool boolResult = (result != 0) ? true : false;
            return boolResult;
        }

        #endregion Readers

        int GetStringLength(Byte[] bytes)
        {
            return Array.IndexOf(bytes, Convert.ToByte('\0'));
        }
    }

}