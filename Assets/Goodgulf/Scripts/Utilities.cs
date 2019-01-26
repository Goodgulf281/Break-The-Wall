using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Goodgulf
{
    public class Utilities
    {
        // Use this static method to write to text a log file.

        static public void WriteDebugString(int playerID, string line)
        {
            string path = "C:/Temp/log.txt";

            File.AppendAllText(path, DateTime.Now.ToString() + "["+playerID+"]: " + line + Environment.NewLine);
        }
    }

}
