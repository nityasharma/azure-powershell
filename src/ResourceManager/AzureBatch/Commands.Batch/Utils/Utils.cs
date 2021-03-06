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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Batch;
using Microsoft.Azure.Commands.Batch.Models;

namespace Microsoft.Azure.Commands.Batch.Utils
{
    /// <summary>
    /// Helper class
    /// </summary>
    internal static class Utils
    {
        /// <summary>
        /// Syncs the collections on a PSJobSpecification with its wrapped OM object
        /// </summary>
        internal static void JobSpecificationSyncCollections(PSJobSpecification specification)
        {
            if (specification != null)
            {
                specification.omObject.CommonEnvironmentSettings = CreateSyncedList(specification.CommonEnvironmentSettings,
                (e) =>
                {
                    EnvironmentSetting envSetting = new EnvironmentSetting(e.Name, e.Value);
                    return envSetting;
                });

                if (specification.JobManagerTask != null)
                {
                    JobManagerTaskSyncCollections(specification.JobManagerTask);   
                }

                if (specification.JobPreparationTask != null)
                {
                    JobPreparationTaskSyncCollections(specification.JobPreparationTask);
                }

                if (specification.JobReleaseTask != null)
                {
                    JobReleaseTaskSyncCollections(specification.JobReleaseTask);
                }

                specification.omObject.Metadata = CreateSyncedList(specification.Metadata,
                (m) =>
                {
                    MetadataItem metadata = new MetadataItem(m.Name, m.Value);
                    return metadata;
                });
                
                if (specification.PoolInformation != null)
                {
                    PoolInformationSyncCollections(specification.PoolInformation);
                }
            }
        }

        /// <summary>
        /// Syncs the collections on a PSJobManagerTask with its wrapped OM object
        /// </summary>
        internal static void JobManagerTaskSyncCollections(PSJobManagerTask jobManager)
        {
            if (jobManager != null)
            {
                jobManager.omObject.EnvironmentSettings = CreateSyncedList(jobManager.EnvironmentSettings, 
                    (e) =>
                    {
                        EnvironmentSetting envSetting = new EnvironmentSetting(e.Name, e.Value);
                        return envSetting;
                    });

                jobManager.omObject.ResourceFiles = CreateSyncedList(jobManager.ResourceFiles,
                    (r) =>
                    {
                        ResourceFile resourceFile = new ResourceFile(r.BlobSource, r.FilePath);
                        return resourceFile;
                    });
            }
        }

        /// <summary>
        /// Syncs the collections on a PSJobPreparationTask with its wrapped OM object
        /// </summary>
        internal static void JobPreparationTaskSyncCollections(PSJobPreparationTask jobPrepTask)
        {
            if (jobPrepTask != null)
            {
                jobPrepTask.omObject.EnvironmentSettings = CreateSyncedList(jobPrepTask.EnvironmentSettings,
                    (e) =>
                    {
                        EnvironmentSetting envSetting = new EnvironmentSetting(e.Name, e.Value);
                        return envSetting;
                    });

                jobPrepTask.omObject.ResourceFiles = CreateSyncedList(jobPrepTask.ResourceFiles,
                    (r) =>
                    {
                        ResourceFile resourceFile = new ResourceFile(r.BlobSource, r.FilePath);
                        return resourceFile;
                    });
            }
        }

        /// <summary>
        /// Syncs the collections on a PSJobReleaseTask with its wrapped OM object
        /// </summary>
        internal static void JobReleaseTaskSyncCollections(PSJobReleaseTask jobReleaseTask)
        {
            if (jobReleaseTask != null)
            {
                jobReleaseTask.omObject.EnvironmentSettings = CreateSyncedList(jobReleaseTask.EnvironmentSettings,
                    (e) =>
                    {
                        EnvironmentSetting envSetting = new EnvironmentSetting(e.Name, e.Value);
                        return envSetting;
                    });

                jobReleaseTask.omObject.ResourceFiles = CreateSyncedList(jobReleaseTask.ResourceFiles,
                    (r) =>
                    {
                        ResourceFile resourceFile = new ResourceFile(r.BlobSource, r.FilePath);
                        return resourceFile;
                    });
            }
        }

        /// <summary>
        /// Syncs the collections on a PSPoolInformation with its wrapped OM object
        /// </summary>
        internal static void PoolInformationSyncCollections(PSPoolInformation poolInfo)
        {
            if (poolInfo != null)
            {
                if (poolInfo.AutoPoolSpecification != null)
                {
                    AutoPoolSpecificationSyncCollections(poolInfo.AutoPoolSpecification);
                }
            }
        }

        /// <summary>
        /// Syncs the collections on a PSAutoPoolSpecification with its wrapped OM object
        /// </summary>
        internal static void AutoPoolSpecificationSyncCollections(PSAutoPoolSpecification spec)
        {
            if (spec != null)
            {
                if (spec.PoolSpecification != null)
                {
                    PoolSpecificationSyncCollections(spec.PoolSpecification);
                }
            }
        }

        /// <summary>
        /// Syncs the collections on a PSPoolSpecification with its wrapped OM object
        /// </summary>
        internal static void PoolSpecificationSyncCollections(PSPoolSpecification spec)
        {
            if (spec != null)
            {
                spec.omObject.CertificateReferences = CreateSyncedList(spec.CertificateReferences,
                    (c) =>
                    {
                        CertificateReference certReference = new CertificateReference();
                        certReference.StoreLocation = c.StoreLocation;
                        certReference.StoreName = c.StoreName;
                        certReference.Thumbprint = c.Thumbprint;
                        certReference.ThumbprintAlgorithm = c.ThumbprintAlgorithm;
                        certReference.Visibility = c.Visibility;
                        return certReference;
                    });

                spec.omObject.Metadata = CreateSyncedList(spec.Metadata, 
                    (m) =>
                    {
                        MetadataItem metadata = new MetadataItem(m.Name, m.Value);
                        return metadata;
                    });

                if (spec.StartTask != null)
                {
                    StartTaskSyncCollections(spec.StartTask);
                }
            }
        }

        /// <summary>
        /// Syncs the collections on a PSStartTask with its wrapped OM object
        /// </summary>
        internal static void StartTaskSyncCollections(PSStartTask startTask)
        {
            if (startTask != null)
            {
                startTask.omObject.EnvironmentSettings = CreateSyncedList(startTask.EnvironmentSettings,
                    (e) =>
                    {
                        EnvironmentSetting envSetting = new EnvironmentSetting(e.Name, e.Value);
                        return envSetting;
                    });

                startTask.omObject.ResourceFiles = CreateSyncedList(startTask.ResourceFiles,
                    (r) =>
                    {
                        ResourceFile resourceFile = new ResourceFile(r.BlobSource, r.FilePath);
                        return resourceFile;
                    });
            }
        }

        /// <summary>
        /// Creates a list of OM objects matching a list of PowerShell objects
        /// </summary>
        /// <typeparam name="Tps">The type of the PowerShell class</typeparam>
        /// <typeparam name="Tom">The type of the OM class</typeparam>
        /// <param name="psList">The list of PowerShell items</param>
        /// <param name="mappingFunction">The function to create a matching OM item</param>
        /// <returns>A list of OM objects matching a list of PowerShell objects</returns>
        private static IList<Tom> CreateSyncedList<Tps, Tom>(IEnumerable<Tps> psList, Func<Tps, Tom> mappingFunction)
        {
            if (psList == null)
            {
                return null;
            }

            List<Tom> omList = new List<Tom>();
            foreach (Tps item in psList)
            {
                Tom omItem = mappingFunction(item);
                omList.Add(omItem);
            }
            return omList;
        }
    }
}
