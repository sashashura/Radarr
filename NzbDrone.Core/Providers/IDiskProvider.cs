﻿using System;
using System.IO;

namespace NzbDrone.Core.Providers
{
    public interface IDiskProvider
    {
        bool Exists(string path);
        string[] GetDirectories(string path);
        String CreateDirectory(string path);
        string[] GetFiles(string path, string pattern, SearchOption searchOption);
    }
}