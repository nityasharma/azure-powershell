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

using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.Azure.Management.Network;
using Microsoft.Azure.Commands.Network.Models;
using MNM = Microsoft.Azure.Management.Network.Models;

namespace Microsoft.Azure.Commands.Network
{
     [Cmdlet(VerbsCommon.Get, "AzureRMNetworkSecurityGroup"), OutputType(typeof(PSNetworkSecurityGroup))]
    public class GetAzureNetworkSecurityGroupCommand : NetworkSecurityGroupBaseCmdlet
    {
        [Alias("ResourceName")]
        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The resource name.")]
        [ValidateNotNullOrEmpty]
        public virtual string Name { get; set; }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The resource group name.")]
        [ValidateNotNullOrEmpty]
        public virtual string ResourceGroupName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            if (!string.IsNullOrEmpty(this.Name))
            {
                var nsg = this.GetNetworkSecurityGroup(this.ResourceGroupName, this.Name);

                WriteObject(nsg);
            }
            else if (!string.IsNullOrEmpty(this.ResourceGroupName))
            {
                var nsgGetResponse = this.NetworkSecurityGroupClient.List(this.ResourceGroupName);

                var psNsgs = new List<PSNetworkSecurityGroup>();
                
                foreach (var networkSecurityGroup in nsgGetResponse.NetworkSecurityGroups)
                {
                    var psNsg = this.ToPsNetworkSecurityGroup(networkSecurityGroup);
                    psNsg.ResourceGroupName = this.ResourceGroupName;
                    psNsgs.Add(psNsg);
                }

                WriteObject(psNsgs, true);
            }
            else
            {
                var nsgGetResponse = this.NetworkSecurityGroupClient.ListAll();

                var psNsgs = new List<PSNetworkSecurityGroup>();

                foreach (var networkSecurityGroup in nsgGetResponse.NetworkSecurityGroups)
                {
                    var psNsg = this.ToPsNetworkSecurityGroup(networkSecurityGroup);
                    psNsg.ResourceGroupName = NetworkBaseCmdlet.GetResourceGroup(networkSecurityGroup.Id);
                    psNsgs.Add(psNsg);
                }

                WriteObject(psNsgs, true);
            }
        }
    }
}
