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

using Microsoft.Azure.Common.Authentication;
using Microsoft.Azure.Management.BackupServices;
using Microsoft.Azure.Test;
using Microsoft.Azure.Test.HttpRecorder;
using Microsoft.WindowsAzure.Commands.ScenarioTest;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Reflection;
using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;

namespace Microsoft.Azure.Commands.AzureBackup.Test.ScenarioTests
{
    public abstract class AzureBackupTestsBase : RMTestBase
    {
        private CSMTestEnvironmentFactory csmTestFactory;
        private EnvironmentSetupHelper helper;

        public static string ResourceGroupName;
        public static string ResourceName;

        public BackupVaultServicesManagementClient BackupVaultServicesMgmtClient { get; private set; }

        public BackupServicesManagementClient BackupServicesMgmtClient { get; private set; }

        protected AzureBackupTestsBase()
        {
            AzureBackupTestsBase.ResourceName = ConfigurationManager.AppSettings["ResourceName"];
            AzureBackupTestsBase.ResourceGroupName = ConfigurationManager.AppSettings["ResourceGroupName"];

            this.helper = new EnvironmentSetupHelper();
            this.csmTestFactory = new CSMTestEnvironmentFactory();
        }

        protected void SetupManagementClients()
        {
            BackupVaultServicesMgmtClient = GetBackupVaultServicesManagementClient();
            BackupServicesMgmtClient = GetBackupServicesManagementClient();

            helper.SetupSomeOfManagementClients(BackupVaultServicesMgmtClient, BackupServicesMgmtClient);
        }

        protected void RunPowerShellTest(params string[] scripts)
        {
            using (UndoContext context = UndoContext.Current)
            {
                context.Start(TestUtilities.GetCallingClass(2), TestUtilities.GetCurrentMethodName(2));

                SetupManagementClients();

                helper.SetupEnvironment(AzureModule.AzureResourceManager);
                helper.SetupModules(AzureModule.AzureResourceManager, "ScenarioTests\\" + this.GetType().Name + ".ps1");

                helper.RunPowerShellTest(scripts);
            }
        }

        private BackupVaultServicesManagementClient GetBackupVaultServicesManagementClient()
        {
            return GetServiceClient<BackupVaultServicesManagementClient>();
        }

        private BackupServicesManagementClient GetBackupServicesManagementClient()
        {
            return GetServiceClient<BackupServicesManagementClient>();
        }

        public static T GetServiceClient<T>() where T : class
        {
            var factory = (TestEnvironmentFactory)new CSMTestEnvironmentFactory();

            var testEnvironment = factory.GetTestEnvironment();

            ServicePointManager.ServerCertificateValidationCallback = IgnoreCertificateErrorHandler;

            if (typeof(T) == typeof(BackupVaultServicesManagementClient))
            {
                BackupVaultServicesManagementClient client;

                if (testEnvironment.UsesCustomUri())
                {
                    client = new BackupVaultServicesManagementClient(
                        testEnvironment.Credentials as SubscriptionCloudCredentials,
                        testEnvironment.BaseUri);
                }

                else
                {
                    client = new BackupVaultServicesManagementClient(
                        testEnvironment.Credentials as SubscriptionCloudCredentials);
                }

                return GetServiceClient<T>(factory, client);
            }
            else
            {
                BackupServicesManagementClient client;

                if (testEnvironment.UsesCustomUri())
                {
                    client = new BackupServicesManagementClient(
                        testEnvironment.Credentials as SubscriptionCloudCredentials,
                        testEnvironment.BaseUri);
                }

                else
                {
                    client = new BackupServicesManagementClient(
                        testEnvironment.Credentials as SubscriptionCloudCredentials);
                }

                return GetVaultServiceClient<T>(factory, client);
            }
        }

        public static T GetServiceClient<T>(TestEnvironmentFactory factory, BackupVaultServicesManagementClient client) where T : class
        {
            TestEnvironment testEnvironment = factory.GetTestEnvironment();

            HttpMockServer instance;
            try
            {
                instance = HttpMockServer.CreateInstance();
            }
            catch (ApplicationException)
            {
                HttpMockServer.Initialize("TestEnvironment", "InitialCreation");
                instance = HttpMockServer.CreateInstance();
            }
            T obj2 = typeof(T).GetMethod("WithHandler", new Type[1]
            {
                typeof (DelegatingHandler)
            }).Invoke((object)client, new object[1]
            {
                (object) instance
            }) as T;

            if (HttpMockServer.Mode == HttpRecorderMode.Record)
            {
                HttpMockServer.Variables[TestEnvironment.SubscriptionIdKey] = testEnvironment.SubscriptionId;
            }

            if (HttpMockServer.Mode == HttpRecorderMode.Playback)
            {
                PropertyInfo property1 = typeof(T).GetProperty("LongRunningOperationInitialTimeout", typeof(int));
                PropertyInfo property2 = typeof(T).GetProperty("LongRunningOperationRetryTimeout", typeof(int));
                if (property1 != (PropertyInfo)null && property2 != (PropertyInfo)null)
                {
                    property1.SetValue((object)obj2, (object)0);
                    property2.SetValue((object)obj2, (object)0);
                }
            }
            return obj2;
        }

        public static T GetVaultServiceClient<T>(TestEnvironmentFactory factory, BackupServicesManagementClient client) where T : class
        {
            TestEnvironment testEnvironment = factory.GetTestEnvironment();

            HttpMockServer instance;
            try
            {
                instance = HttpMockServer.CreateInstance();
            }
            catch (ApplicationException)
            {
                HttpMockServer.Initialize("TestEnvironment", "InitialCreation");
                instance = HttpMockServer.CreateInstance();
            }
            T obj2 = typeof(T).GetMethod("WithHandler", new Type[1]
            {
                typeof (DelegatingHandler)
            }).Invoke((object)client, new object[1]
            {
                (object) instance
            }) as T;

            if (HttpMockServer.Mode == HttpRecorderMode.Record)
            {
                HttpMockServer.Variables[TestEnvironment.SubscriptionIdKey] = testEnvironment.SubscriptionId;
            }

            if (HttpMockServer.Mode == HttpRecorderMode.Playback)
            {
                PropertyInfo property1 = typeof(T).GetProperty("LongRunningOperationInitialTimeout", typeof(int));
                PropertyInfo property2 = typeof(T).GetProperty("LongRunningOperationRetryTimeout", typeof(int));
                if (property1 != (PropertyInfo)null && property2 != (PropertyInfo)null)
                {
                    property1.SetValue((object)obj2, (object)0);
                    property2.SetValue((object)obj2, (object)0);
                }
            }
            return obj2;
        }

        private static bool IgnoreCertificateErrorHandler
           (object sender,
           System.Security.Cryptography.X509Certificates.X509Certificate certificate,
           System.Security.Cryptography.X509Certificates.X509Chain chain,
           SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}