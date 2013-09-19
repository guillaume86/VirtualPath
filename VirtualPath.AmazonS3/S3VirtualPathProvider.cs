using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.IO;
using VirtualPath.Common;

namespace VirtualPath.AmazonS3
{
    [Export("AmazonS3", typeof(IVirtualPathProvider))]
    [Export("S3", typeof(IVirtualPathProvider))]
    public class S3VirtualPathProvider : AbstractVirtualPathProviderBase
    {
        private AmazonS3Client Client;
        private string BucketName;

        public S3VirtualPathProvider(AmazonS3Client client, string bucket) : base()
        {
            this.Client = client;
            this.BucketName = bucket;
        }

        public S3VirtualPathProvider(string accessKeyId, string secretAccessKey, string bucket)
            : this(GetClient(accessKeyId, secretAccessKey), bucket)
        {

        }

        private static AmazonS3Client GetClient(string awsAccessKeyId, string awsSecretAccessKey)
        {
            var config = new AmazonS3Config { };
            return new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey, config);
        }

        private IVirtualDirectory _rootDirectory;
        public override IVirtualDirectory RootDirectory
        {
            get 
            { 
                return _rootDirectory =  (_rootDirectory 
                    ?? new S3VirtualDirectory(this, null, new Amazon.S3.IO.S3DirectoryInfo(Client, BucketName))); 
            }
        }

        public override string VirtualPathSeparator
        {
            get { return "/"; }
        }

        public override string RealPathSeparator
        {
            get { return "/"; }
        }

        internal S3FileInfo CreateFile(string virtualPath, byte[] contents)
        {
            var fInfo = new S3FileInfo(Client, BucketName, virtualPath);
            using (var stream = fInfo.Create())
            {
                stream.Write(contents, 0, contents.Length);
            }
            return fInfo;
        }

        internal System.IO.Stream CreateFile(string virtualPath)
        {
            var fInfo = new S3FileInfo(Client, BucketName, virtualPath);
            return fInfo.Create();
        }
    }
}