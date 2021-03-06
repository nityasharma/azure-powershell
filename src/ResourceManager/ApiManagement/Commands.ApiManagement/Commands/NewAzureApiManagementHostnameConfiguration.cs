﻿//  
// Copyright (c) Microsoft.  All rights reserved.
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

namespace Microsoft.Azure.Commands.ApiManagement.Commands
{
    using System.Management.Automation;
    using Microsoft.Azure.Commands.ApiManagement.Models;
    using ResourceManager.Common;
    using Microsoft.WindowsAzure.Commands.Utilities.Common;

    [Cmdlet(VerbsCommon.New, "AzureRMApiManagementHostnameConfiguration"), OutputType(typeof (PsApiManagementHostnameConfiguration))]
    public class NewAzureApiManagementHostnameConfiguration : AzureRMCmdlet
    {
        [Parameter(
            ValueFromPipelineByPropertyName = false,
            Mandatory = true,
            HelpMessage = "Certificagte thumbprint. The certificate must be first imported with Import-ApiManagementCertificate command.")]
        [ValidateNotNullOrEmpty]
        public string CertificateThumbprint { get; set; }
        
        [Parameter(
            ValueFromPipelineByPropertyName = false,
            Mandatory = true,
            HelpMessage = "Custom hostaname.")]
        [ValidateNotNullOrEmpty]
        public string Hostname { get; set; }

        protected override void ProcessRecord()
        {
            WriteObject(
                new PsApiManagementHostnameConfiguration
                {
                    Hostname = Hostname,
                    HostnameCertificate = new PsApiManagementHostnameCertificate
                    {
                        Thumbprint = CertificateThumbprint
                    }
                });
        }
    }
}