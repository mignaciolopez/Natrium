using System.IO;
using System;
using System.Text;
using UnityEngine;

namespace CEG.Shared
{
    public abstract class Stream : MemoryStream
    {
        #region Writers
        public void Write(Events value)
        {
            var bytes = BitConverter.GetBytes((int)value);
            Write(bytes, 0, bytes.Length);
        }

        public void Write(int value)
        {
            var bytes = BitConverter.GetBytes(value);
            Write(bytes, 0, bytes.Length);
        }

        public void Write(long value)
        {
            var bytes = BitConverter.GetBytes(value);
            Write(bytes, 0, bytes.Length);
        }

        public void Write(float value)
        {
            var bytes = BitConverter.GetBytes(value);
            Write(bytes, 0, bytes.Length);
        }

        public void Write(double value)
        {
            var bytes = BitConverter.GetBytes(value);
            Write(bytes, 0, bytes.Length);
        }

        public void Write(bool value)
        {
            var bytes = BitConverter.GetBytes(value);
            Write(bytes, 0, bytes.Length);
        }

        public void Write(string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value + '\0');

            var stringLength = GetStringLength(bytes) + 1;

            if (stringLength < bytes.Length)
            {
                UnityEngine.Debug.LogWarning("String truncated.");
                var newBytes = new byte[stringLength];
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
            var bytes = new byte[sizeof(Events)];
            var result = Read(bytes, 0, bytes.Length);

            value = (Events)BitConverter.ToInt32(bytes);
            
            return result != 0;
        }

        public bool Read(out int value)
        {
            var bytes = new byte[sizeof(int)];
            var result = Read(bytes, 0, bytes.Length);

            value = BitConverter.ToInt32(bytes);

            return result != 0;
        }

        public bool Read(out long value)
        {
            var bytes = new byte[sizeof(long)];
            var result = Read(bytes, 0, bytes.Length);

            value = BitConverter.ToInt64(bytes);
            
            return result != 0;
        }

        public bool Read(out float value)
        {
            var bytes = new byte[sizeof(float)];
            var result = Read(bytes, 0, bytes.Length);

            value = BitConverter.ToSingle(bytes);
            
            return result != 0;
        }

        public bool Read(out double value)
        {
            var bytes = new byte[sizeof(double)];
            var result = Read(bytes, 0, bytes.Length);

            value = BitConverter.ToDouble(bytes);
            
            return result != 0;
        }

        public bool Read(out bool value)
        {
            var bytes = new byte[sizeof(bool)];
            var result = Read(bytes, 0, bytes.Length);

            value = BitConverter.ToBoolean(bytes);
            
            return result != 0;
        }

        public bool Read(out string value)
        {
            var streamBytes = new byte[50];
            var result = Read(streamBytes, 0, streamBytes.Length);

            var stringLength = GetStringLength(streamBytes);

            var bytes = new byte[stringLength];

            Array.Copy(streamBytes, 0, bytes, 0, stringLength);

            value = Encoding.ASCII.GetString(bytes);

            Position -= result;
            Position += stringLength + 1;
            
            return result != 0;
        }

        #endregion Readers

        private static int GetStringLength(byte[] bytes)
        {
            return Array.IndexOf(bytes, Convert.ToByte('\0'));
        }
    }

}