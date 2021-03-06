﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using AutoMapper;
using Microsoft.Azure.Management.Network;
using Microsoft.Azure.Commands.Network.Models;
using Microsoft.Azure.Commands.ResourceManager.Common.Tags;
using MNM = Microsoft.Azure.Management.Network.Models;
using Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters;

namespace Microsoft.Azure.Commands.Network
{
    [Cmdlet(VerbsCommon.New, "AzureRmVirtualNetwork"), OutputType(typeof(PSVirtualNetwork))]
    public class NewAzureVirtualNetworkCommand : VirtualNetworkBaseCmdlet
    {
        [Alias("ResourceName")]
        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The resource name.")]
        [ValidateNotNullOrEmpty]
        public virtual string Name { get; set; }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The resource group name.")]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public virtual string ResourceGroupName { get; set; }

        [Parameter(
         Mandatory = true,
         ValueFromPipelineByPropertyName = true,
         HelpMessage = "location.")]
        [ValidateNotNullOrEmpty]
        public virtual string Location { get; set; }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The address prefixes of the virtual network")]
        [ValidateNotNullOrEmpty]
        public List<string> AddressPrefix { get; set; }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The list of Dns Servers")]
        public List<string> DnsServer { get; set; }

        [Parameter(
             Mandatory = false,
             ValueFromPipelineByPropertyName = true,
             HelpMessage = "The list of subnets")]
        public List<PSSubnet> Subnet { get; set; }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "An array of hashtables which represents resource tags.")]
        public Hashtable Tag { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "Do not ask for confirmation if you want to overrite a resource")]
        public SwitchParameter Force { get; set; }

        public override void ExecuteCmdlet()
        {
            base.ExecuteCmdlet();

            if (this.IsVirtualNetworkPresent(this.ResourceGroupName, this.Name))
            {
                ConfirmAction(
                    Force.IsPresent,
                    string.Format(Microsoft.Azure.Commands.Network.Properties.Resources.OverwritingResource, Name),
                    Microsoft.Azure.Commands.Network.Properties.Resources.OverwritingResourceMessage,
                    Name,
                    () => CreateVirtualNetwork());

                WriteObject(this.GetVirtualNetwork(this.ResourceGroupName, this.Name));
            }
            else
            {
                var virtualNetwork = CreateVirtualNetwork();

                WriteObject(virtualNetwork);
            }
        }

        private PSVirtualNetwork CreateVirtualNetwork()
        {
            var vnet = new PSVirtualNetwork();
            vnet.Name = this.Name;
            vnet.ResourceGroupName = this.ResourceGroupName;
            vnet.Location = this.Location;
            vnet.AddressSpace = new PSAddressSpace();
            vnet.AddressSpace.AddressPrefixes = this.AddressPrefix;

            if (this.DnsServer != null)
            {
                vnet.DhcpOptions = new PSDhcpOptions();
                vnet.DhcpOptions.DnsServers = this.DnsServer;
            }

            vnet.Subnets = new List<PSSubnet>();
            vnet.Subnets = this.Subnet;

            // Map to the sdk object
            var vnetModel = Mapper.Map<MNM.VirtualNetwork>(vnet);
            vnetModel.Tags = TagsConversionHelper.CreateTagDictionary(this.Tag, validate: true);

            // Execute the Create VirtualNetwork call
            this.VirtualNetworkClient.CreateOrUpdate(this.ResourceGroupName, this.Name, vnetModel);

            var getVirtualNetwork = this.GetVirtualNetwork(this.ResourceGroupName, this.Name);

            return getVirtualNetwork;
        }
    }
}
