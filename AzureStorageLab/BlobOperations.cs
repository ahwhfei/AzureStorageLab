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

        string localFile = string.Empty;

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
            DownloadBlobs();

            CopyBlob();
            GetListOfBlobs();

            BlobProperties();

            DeleteOneBlob();
            GetListOfBlobs();

            CleanUp();

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

        public void DownloadBlobs()
        {
            //create a subfolder for the downloaded files
            // under the folder with the images that we were using for the upload
            localDownloadPath = Path.Combine(localPicsToUploadPath, "downloaded");
            if (!Directory.Exists(localDownloadPath))
            {
                Directory.CreateDirectory(localDownloadPath);
            }

            cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(image1Name);
            if (cloudBlockBlob.Exists())
            {
                localFile = Path.Combine(localDownloadPath, image1Name);
                cloudBlockBlob.DownloadToFile(localFile, FileMode.Create);
            }
            cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(image2Name);
            if (cloudBlockBlob.Exists())
            {
                localFile = Path.Combine(localDownloadPath, image2Name);
                cloudBlockBlob.DownloadToFile(localFile, FileMode.Create);
            }
        }

        public void CopyBlob()
        {
            //copy from image1name
            cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(image1Name);
            //get a reference to the destination blob.
            CloudBlockBlob destBlob = cloudBlobContainer.GetBlockBlobReference("copyof_" + image1Name);
            //start the copy from the source blob (cloudBlockBlob) to the destination blob.
            destBlob.StartCopy(cloudBlockBlob);
        }

        public void BlobProperties()
        {
            //let's look at snowy cabin, which is image4 and is in the pseudo-folder
            cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(pseudoFolder + image4Name);

            Console.WriteLine(string.Empty);
            //display some of the blob properties
            Console.WriteLine("blob type = " + cloudBlockBlob.BlobType);
            Console.WriteLine("blob name = " + cloudBlockBlob.Name);
            Console.WriteLine("blob URI = " + cloudBlockBlob.Uri);

            //update the system properties on the object and display a couple of them
            cloudBlockBlob.FetchAttributes();
            Console.WriteLine("content type = " + cloudBlockBlob.Properties.ContentType);
            Console.WriteLine("size = " + cloudBlockBlob.Properties.Length);

            //change the content type from 'application/octet stream' to 'image/jpg'
            cloudBlockBlob.Properties.ContentType = "image/jpg";
            cloudBlockBlob.SetProperties();
            //refresh the attributes and write out the content type again
            cloudBlockBlob.FetchAttributes();
            Console.WriteLine("content type = " + cloudBlockBlob.Properties.ContentType);

            //print the current values
            PrintMetadata();
            //set some metadata and save it
            cloudBlockBlob.Metadata["First"] = "number one";
            cloudBlockBlob.Metadata["Second"] = "number two";
            cloudBlockBlob.Metadata["Three"] = "number three";
            cloudBlockBlob.SetMetadata();
            //print it out again
            PrintMetadata();

            //now clear the metadata, save the change,
            // and then print it again (empty list)
            cloudBlockBlob.Metadata.Clear();
            cloudBlockBlob.SetMetadata();
            PrintMetadata();
        }

        public void PrintMetadata()
        {
            //fetch the attributes of the blob to make sure they are current
            cloudBlockBlob.FetchAttributes();
            //if there is metaata, loop throught he dictionary and print it out
            int index = 0;
            if (cloudBlockBlob.Metadata.Count > 0)
            {
                IDictionary<string, string> metadata = cloudBlockBlob.Metadata;
                foreach (KeyValuePair<string, string> oneMetadata in metadata)
                {
                    index++;
                    Console.WriteLine("metadata {0} = {1}, {2}", index,
                    oneMetadata.Key.ToString(), oneMetadata.Value.ToString());
                }
            }
            else
            {
                Console.WriteLine("No metadata found.");
            }
        }

        public void DeleteOneBlob()
        {
            //delete one blob
            string blobName = image3Name;
            cloudBlockBlob =
            cloudBlobContainer.GetBlockBlobReference(pseudoFolder + textFileName);
            cloudBlockBlob.Delete();
        }

        public void DeleteAllBlobs()
        {
            List<string> listOBlobs = new List<string>();
            foreach (IListBlobItem blobItem in
            cloudBlobContainer.ListBlobs(null, true, BlobListingDetails.None))
            {
                string oneFile = GetFileNameFromBlobURI(blobItem.Uri, containerName);
                cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(oneFile);
                cloudBlockBlob.Delete();
            }
        }

        public void CleanUp()
        {
            DeleteAllBlobs();
            GetListOfBlobs();
            cloudBlobContainer.Delete();
            if (!string.IsNullOrEmpty(localDownloadPath))
            {
                Directory.Delete(localDownloadPath, true);
            }
        }
    }
}
