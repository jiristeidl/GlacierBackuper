﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Glacier;
using Amazon.Glacier.Transfer;
using Amazon.Glacier.Model;

namespace GlacierBackuper
{
    internal class Program
    {
        private static Glacier client;
        private static List<DescribeVaultOutput> vaultList;

        private static Glacier GlacierClient
        {
            get
            {
                if (client == null)
                {
                    client = new Glacier();
                }
                return client;
            }
        }

        private static string SelectedVault { get; set; }

        private static List<DescribeVaultOutput> VaultList
        {
            get
            {
                if (vaultList == null)
                {
                    vaultList = GlacierClient.GetVaultsList();
                }
                return vaultList;
            }
        }

        public static void Main(string[] args)
        {
            WriteBasicInfo();
            SelectedVault = WriteVaultPicker();

            Console.Read();
        }

        private static string GetServiceOutput()
        {
            StringBuilder sb = new StringBuilder(1024);
            using (StringWriter sr = new StringWriter(sb))
            {
                sr.WriteLine("===========================================");
                sr.WriteLine("Welcome to the AWS .NET SDK!");
                sr.WriteLine("===========================================");

                // Print the number of Amazon EC2 instances.
                IAmazonEC2 ec2 = AWSClientFactory.CreateAmazonEC2Client();
                DescribeInstancesRequest ec2Request = new DescribeInstancesRequest();

                try
                {
                    DescribeInstancesResponse ec2Response = ec2.DescribeInstances(ec2Request);
                    int numInstances = 0;
                    numInstances = ec2Response.Reservations.Count;
                    sr.WriteLine(string.Format("You have {0} Amazon EC2 instance(s) running in the {1} region.",
                                               numInstances, ConfigurationManager.AppSettings["AWSRegion"]));
                }
                catch (AmazonEC2Exception ex)
                {
                    if (ex.ErrorCode != null && ex.ErrorCode.Equals("AuthFailure"))
                    {
                        sr.WriteLine("The account you are using is not signed up for Amazon EC2.");
                        sr.WriteLine("You can sign up for Amazon EC2 at http://aws.amazon.com/ec2");
                    }
                    else
                    {
                        sr.WriteLine("Caught Exception: " + ex.Message);
                        sr.WriteLine("Response Status Code: " + ex.StatusCode);
                        sr.WriteLine("Error Code: " + ex.ErrorCode);
                        sr.WriteLine("Error Type: " + ex.ErrorType);
                        sr.WriteLine("Request ID: " + ex.RequestId);
                    }
                }
                sr.WriteLine();

                // Print the number of Amazon SimpleDB domains.
                IAmazonSimpleDB sdb = AWSClientFactory.CreateAmazonSimpleDBClient();
                ListDomainsRequest sdbRequest = new ListDomainsRequest();

                try
                {
                    ListDomainsResponse sdbResponse = sdb.ListDomains(sdbRequest);

                    int numDomains = 0;
                    numDomains = sdbResponse.DomainNames.Count;
                    sr.WriteLine(string.Format("You have {0} Amazon SimpleDB domain(s) in the {1} region.",
                                               numDomains, ConfigurationManager.AppSettings["AWSRegion"]));
                }
                catch (AmazonSimpleDBException ex)
                {
                    if (ex.ErrorCode != null && ex.ErrorCode.Equals("AuthFailure"))
                    {
                        sr.WriteLine("The account you are using is not signed up for Amazon SimpleDB.");
                        sr.WriteLine("You can sign up for Amazon SimpleDB at http://aws.amazon.com/simpledb");
                    }
                    else
                    {
                        sr.WriteLine("Caught Exception: " + ex.Message);
                        sr.WriteLine("Response Status Code: " + ex.StatusCode);
                        sr.WriteLine("Error Code: " + ex.ErrorCode);
                        sr.WriteLine("Error Type: " + ex.ErrorType);
                        sr.WriteLine("Request ID: " + ex.RequestId);
                    }
                }
                sr.WriteLine();

                // Print the number of Amazon S3 Buckets.
                IAmazonS3 s3Client = AWSClientFactory.CreateAmazonS3Client();

                try
                {
                    ListBucketsResponse response = s3Client.ListBuckets();
                    int numBuckets = 0;
                    if (response.Buckets != null &&
                        response.Buckets.Count > 0)
                    {
                        numBuckets = response.Buckets.Count;
                    }
                    sr.WriteLine("You have " + numBuckets + " Amazon S3 bucket(s).");
                }
                catch (AmazonS3Exception ex)
                {
                    if (ex.ErrorCode != null && (ex.ErrorCode.Equals("InvalidAccessKeyId") ||
                        ex.ErrorCode.Equals("InvalidSecurity")))
                    {
                        sr.WriteLine("Please check the provided AWS Credentials.");
                        sr.WriteLine("If you haven't signed up for Amazon S3, please visit http://aws.amazon.com/s3");
                    }
                    else
                    {
                        sr.WriteLine("Caught Exception: " + ex.Message);
                        sr.WriteLine("Response Status Code: " + ex.StatusCode);
                        sr.WriteLine("Error Code: " + ex.ErrorCode);
                        sr.WriteLine("Request ID: " + ex.RequestId);
                    }
                }
                sr.WriteLine("Press any key to continue...");
            }
            return sb.ToString();
        }

        private static string ReadVaultName(List<DescribeVaultOutput> vaultList)
        {
            bool firstTry = true;
            string input;
            do
            {
                if (!firstTry)
                {
                    Console.WriteLine("Thats not correct, try again");
                }
                input = Console.ReadLine();
                firstTry = false;
            } while (!vaultList.Any(v => v.VaultName.ToLower().Equals(input.ToLower())));

            return input;
        }

        private static void WriteBasicInfo()
        {
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine("Name\tLast Upload\t\tNumber of Archives\tSize");
            foreach (var vault in VaultList)
            {
                Console.WriteLine("{0}\t{1}\t{2}\t\t\t{3}MB", vault.VaultName, vault.LastInventoryDate,vault.NumberOfArchives, vault.SizeInBytes / 1024 / 1024);
            }
            Console.WriteLine("-------------------------------------------------------------");
        }

        private static string WriteVaultPicker()
        {
            Console.WriteLine("Which Vault you want to upload to?");
            return ReadVaultName(VaultList);
        }
    }
}