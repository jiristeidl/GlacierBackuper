using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Amazon.Glacier;
using Amazon.Glacier.Transfer;
using Amazon.Glacier.Model;

namespace GlacierBackuper
{
    internal class Glacier
    {
        private AmazonGlacierClient client { get; set; }

        public DescribeVaultResponse GetVaultDescriptions(string vaultName)
        {
            DescribeVaultRequest request = new DescribeVaultRequest(vaultName);

            return client.DescribeVault(request);
        }

        public Glacier()
        {
            client = new AmazonGlacierClient(new AmazonGlacierConfig());
        }

        public List<DescribeVaultOutput> GetVaultsList()
        {
            return client.ListVaults().VaultList;
        }
    }
}