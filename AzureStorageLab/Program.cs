﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageLab
{
    class Program
    {
        static void Main(string[] args)
        {
            BlobOperations blobOps = new BlobOperations();
            blobOps.BasicBlobOps();
        }
    }
}
