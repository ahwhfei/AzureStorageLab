using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureStorageLab
{
    public class BlobOperations
    {
        string storageAccountName = "whf1122";

        string storageAccountKey =
            "1YvorjJB5bpeDBzn3N6UutKl2etXDUPZeComvrYC2srFYwsHaQ+XBRk8zUUEbuBtLG9cK5tK+03aXB1lALRMyQ==";

        string containerName = "blob-ops";
        string localPicsToUploadPath = @"F:\OpsgilityTraining\Images";
        string localDownloadPath;
        string pseudoFolder = "images/";

        string image1Name = "sb_1.jpg";
        string image2Name = "sb_2.jpg";
        string image3Name = "sb_3.jpg";
        string image4Name = "sb_4.jpg";
        string textFileName = "testtextfile.txt";

        string ConnectionString { get; set; }
        CloudStorageAccount cloudStorageAccount { get; set; }
        CloudBlobClient cloudBlobClient { get; set; }
        CloudBlobContainer cloudBlobContainer { get; set; }
        CloudBlockBlob cloudBlockBlob { get; set; }

        public void SetUpObjects()
        {
            //make the container name unique so if you run this over and over, you won't
            // have any problems with the latency of deleting a container
            containerName = containerName + "-" + System.Guid.NewGuid().ToString().Substring(0, 12);
            //set the connection string
            ConnectionString =
                string.Format(@"DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}",
                    storageAccountName, storageAccountKey);
            //get reference to storage account
            cloudStorageAccount = CloudStorageAccount.Parse(ConnectionString);
            //get reference to the cloud blob client
            //this is used to access the storage account blobs and containers
            cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            //get reference to the container
            cloudBlobContainer = cloudBlobClient.GetContainerReference(containerName);
            //create the container if it doesn't exist
            cloudBlobContainer.CreateIfNotExists();
            //set the permissions so the blobs are public
            BlobContainerPermissions permissions = new BlobContainerPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Blob;
            cloudBlobContainer.SetPermissions(permissions);
        }

        public void BasicBlobOps()
        {
            //set up the objects
            SetUpObjects();
            UploadBlobs();

            GetListOfBlobs();
            Console.WriteLine("Press Any Key to exit.");
            Console.ReadLine();
        }

        public void UploadBlobs()
        {
            //get a reference to where the block blob will go, then upload the local file
            cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(image1Name);
            cloudBlockBlob.UploadFromFile(Path.Combine(localPicsToUploadPath, image1Name));
            cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(image2Name);
            cloudBlockBlob.UploadFromFile(Path.Combine(localPicsToUploadPath, image2Name));
            cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(image3Name);
            cloudBlockBlob.UploadFromFile(Path.Combine(localPicsToUploadPath, image3Name));

            //let's upload a blob with a pseudofolder in the name
            cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(pseudoFolder + image4Name);
            cloudBlockBlob.UploadFromFile(Path.Combine(localPicsToUploadPath, image4Name));

            //let's upload a text blob
            string textToUpload = "Opsgility makes training fun!";
            cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(pseudoFolder + textFileName);
            cloudBlockBlob.UploadText(textToUpload);
        }

        private string GetFileNameFromBlobURI(Uri theUri, string containerName)
        {
            string theFile = theUri.ToString();
            int dirIndex = theFile.IndexOf(containerName);
            string oneFile = theFile.Substring(dirIndex + containerName.Length + 1,
                theFile.Length - (dirIndex + containerName.Length + 1));
            return oneFile;
        }

        public void GetListOfBlobs()
        {
            Console.WriteLine(string.Empty);
            Console.WriteLine("before list");
            foreach (IListBlobItem blobItem in
                cloudBlobContainer.ListBlobs(null, true, BlobListingDetails.None))
            {
                string oneFile = GetFileNameFromBlobURI(blobItem.Uri, containerName);
                Console.WriteLine("blob name = {0}", oneFile);
            }
            Console.WriteLine("after list");
        }
    }
}
