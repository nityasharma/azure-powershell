﻿<#
.SYNOPSIS
Tests API Management Create List Remove operations.
#>
function Test-CrudApiManagement
{
    # Setup
    $location = Get-ProviderLocation "Microsoft.ApiManagement/service"

    # Create resource group
    $resourceGroupName = Get-ResourceGroupName
    New-AzureRMResourceGroup -Name $resourceGroupName -Location $location -Force

    $apiManagementName = Get-ApiManagementServiceName
    $organization = "apimpowershellorg"
    $adminEmail = "apim@powershell.org"

    # Create API Management service
    $result = New-AzureRMApiManagement -ResourceGroupName $resourceGroupName -Location $location -Name $apiManagementName -Organization $organization -AdminEmail $adminEmail

    Assert-AreEqual $resourceGroupName $result.ResourceGroupName
    Assert-AreEqual $apiManagementName $result.Name
    Assert-AreEqual $location $result.Location
    Assert-AreEqual "Developer" $result.Sku
    Assert-AreEqual 1 $result.Capacity

    # Get SSO token
    $token = Get-AzureRMApiManagementSsoToken -ResourceGroupName $resourceGroupName -Name $apiManagementName
    Assert-NotNull $token

    # List services within the resource group
    $apimServicesInGroup = Get-AzureRMApiManagement -ResourceGroupName $resourceGroupName
    Assert-True {$apimServicesInGroup.Count -ge 1}

    $found = 0
    for ($i = 0; $i -lt $apimServicesInGroup.Count; $i++)
    {
        if ($apimServicesInGroup[$i].Name -eq $apiManagementName)
        {
            $found = 1
            Assert-AreEqual $location $apimServicesInGroup[$i].Location
            Assert-AreEqual $resourceGroupName $apimServicesInGroup[$i].ResourceGroupName
    
            Assert-AreEqual "Developer" $apimServicesInGroup[$i].Sku
            Assert-AreEqual 1 $apimServicesInGroup[$i].Capacity
            break
        }
    }
    Assert-True {$found -eq 1} "Api Management created earlier is not found."

    # Create on more group
    $secondResourceGroup = Get-ResourceGroupName
    New-AzureRMResourceGroup -Name $secondResourceGroup -Location $location -Force

    # Create one more service
    $secondApiManagementName = Get-ApiManagementServiceName
    $secondOrganization = "second.apimpowershellorg"
    $secondAdminEmail = "second.apim@powershell.org"
    $secondSku = "Standard"
    $secondSkuCapacity = 2

    $secondResult = New-AzureRMApiManagement -ResourceGroupName $secondResourceGroup -Location $location -Name $secondApiManagementName -Organization $secondOrganization -AdminEmail $secondAdminEmail -Sku $secondSku -Capacity $secondSkuCapacity
    Assert-AreEqual $secondResourceGroup $secondResult.ResourceGroupName
    Assert-AreEqual $secondApiManagementName $secondResult.Name
    Assert-AreEqual $location $secondResult.Location
    Assert-AreEqual $secondSku $secondResult.Sku
    Assert-AreEqual $secondSkuCapacity $secondResult.Capacity

    # Get SSO token
    $secondToken = Get-AzureRMApiManagementSsoToken -ResourceGroupName $secondResourceGroup -Name $secondApiManagementName
    Assert-NotNull $secondToken

    # List all services
    $allServices = Get-AzureRMApiManagement
    Assert-True {$allServices.Count -ge 2}

    $found = 0
    for ($i = 0; $i -lt $allServices.Count; $i++)
    {
        if ($allServices[$i].Name -eq $apiManagementName)
        {
            $found = $found + 1
            Assert-AreEqual $location $allServices[$i].Location
            Assert-AreEqual $resourceGroupName $allServices[$i].ResourceGroupName
    
            Assert-AreEqual "Developer" $allServices[$i].Sku
            Assert-AreEqual 1 $allServices[$i].Capacity
        }

        if ($allServices[$i].Name -eq $secondApiManagementName)
        {
            $found = $found + 1
            Assert-AreEqual $location $allServices[$i].Location
            Assert-AreEqual $secondResourceGroup $allServices[$i].ResourceGroupName
    
            Assert-AreEqual $secondSku $allServices[$i].Sku
            Assert-AreEqual $secondSkuCapacity $allServices[$i].Capacity
        }
    }
    Assert-True {$found -eq 2} "Api Management services created earlier is not found."

    # Delete listed services
    Get-AzureRMApiManagement | Remove-AzureRMApiManagement -Force

    $allServices = Get-AzureRMApiManagement
    Assert-AreEqual 0 $allServices.Count

    # Remove resource group
    Remove-AzureRMResourceGroup -Name $resourceGroupName -Force
}

<#
.SYNOPSIS
Tests API Management Backup/Restore operations.
#>
function Test-BackupRestoreApiManagement
{
    # Setup
    $location = Get-ProviderLocation "Microsoft.ApiManagement/service"

    # Create resource group
    $resourceGroupName = Get-ResourceGroupName
    New-AzureRMResourceGroup -Name $resourceGroupName -Location $location -Force

    # Create storage account

    $storageLocation = Get-ProviderLocation "Microsoft.ClassicStorage/storageAccounts"
    $storageAccountName = Get-ApiManagementServiceName
    New-AzureRMStorageAccount -StorageAccountName $storageAccountName -Location $storageLocation -ResourceGroupName $resourceGroupName -Type Standard_LRS

    $storageKey = (Get-AzureRMStorageAccountKey -ResourceGroupName $resourceGroupName -StorageAccountName $storageAccountName).Key1
    $storageContext = New-AzureRMStorageContext -StorageAccountName $storageAccountName -StorageAccountKey $storageKey

    $apiManagementName = Get-ApiManagementServiceName
    $organization = "apimpowershellorg"
    $adminEmail = "apim@powershell.org"

    $containerName = "backups"
    $backupName = $apiManagementName + ".apimbackup"

    # Create API Management service
    $apiManagementService = New-AzureRMApiManagement -ResourceGroupName $resourceGroupName -Location $location -Name $apiManagementName -Organization $organization -AdminEmail $adminEmail

    $containerName = "backups"
    $backupName = $apiManagementName + ".apimbackup"

    # Backup API Management service
    Backup-AzureRMApiManagement -ResourceGroupName $resourceGroupName -Name $apiManagementName -StorageContext $storageContext -TargetContainerName $containerName -TargetBlobName $backupName

    # Restore API Management service
    $restoreResult = Restore-AzureRMApiManagement -ResourceGroupName $resourceGroupName -Name $apiManagementName -StorageContext $storageContext -SourceContainerName $containerName -SourceBlobName $backupName -PassThru

    Assert-AreEqual $resourceGroupName $restoreResult.ResourceGroupName
    Assert-AreEqual $apiManagementName $restoreResult.Name
    Assert-AreEqual $location $restoreResult.Location
    Assert-AreEqual "Developer" $restoreResult.Sku
    Assert-AreEqual 1 $restoreResult.Capacity
    Assert-AreEqual "Succeeded" $restoreResult.ProvisioningState

    try
    {
        # Remove the service
        Remove-AzureRMApiManagement -ResourceGroupName $resourceGroupName -Name $apiManagementName -Force

        # Remove storage account
        Remove-AzureRMStorageAccount -ResourceGroupName $resourceGroupName -StorageAccountName $storageAccountName

        # Remove resource group
        Remove-AzureRMResourceGroup -Name $resourceGroupName -Force
    }
    catch
    {
    }
}

<#
.SYNOPSIS
Tests UpdateAzureApiManagementDeployment.
#>
function Test-UpdateApiManagementDeployment
{
    # Setup
    $location = Get-ProviderLocation "Microsoft.ApiManagement/service"

    # Create resource group
    $resourceGroupName = Get-ResourceGroupName
    New-AzureRMResourceGroup -Name $resourceGroupName -Location $location -Force

    $apiManagementName = Get-ApiManagementServiceName
    $organization = "apimpowershellorg"
    $adminEmail = "apim@powershell.org"
    $sku = "Developer"
    $capacity = 1

    # Create API Management service
    New-AzureRMApiManagement -ResourceGroupName $resourceGroupName -Location $location -Name $apiManagementName -Organization $organization -AdminEmail $adminEmail -Sku $sku -Capacity $capacity

    # Get API Management and:
    #- 1) Scale master region to 'Premium' 2 units
    $sku = "Premium"
    $capacity = 2

    $service = Get-AzureRMApiManagement -ResourceGroupName $resourceGroupName -Name $apiManagementName
    $service.Sku = $sku;
    $service.Capacity = $capacity

    # - 2) Add new region 1 unit
    $region1Location = Get-ProviderLocations "Microsoft.ApiManagement/service" | Where {$_ -ne $location} | Select -First 1
    $region1Sku = "Premium"
    $service.AddRegion($region1Location, $region1Sku)

    ## - 3) Add one more region 3 units
    #$region2Location = Get-ProviderLocations "Microsoft.ApiManagement/service" | Where {($_ -ne $location) -and ($_ -ne $region1Location)} | Select -First 1
    #$region2Sku = "Premium"
    #$region2Capacity = 3
    #$service.AddRegion($region2Location, $region2Sku, $region2Capacity)

    Update-AzureRMApiManagementDeployment -ApiManagement $service

    $service = Get-AzureRMApiManagement -ResourceGroupName $resourceGroupName -Name $apiManagementName

    Assert-AreEqual $resourceGroupName $service.ResourceGroupName
    Assert-AreEqual $apiManagementName $service.Name
    Assert-AreEqual $location $service.Location
    Assert-AreEqual $sku $service.Sku
    Assert-AreEqual $capacity $service.Capacity
    Assert-AreEqual "Succeeded" $service.ProvisioningState

    #Assert-AreEqual 2 $service.AdditionalRegions.Count
    Assert-AreEqual 1 $service.AdditionalRegions.Count
    $found = 0
    for ($i = 0; $i -lt $service.AdditionalRegions.Count; $i++)
    {
        if ($service.AdditionalRegions[$i].Location -eq $region1Location)
        {
            $found = $found + 1
            Assert-AreEqual $region1Sku $service.AdditionalRegions[$i].Sku
            Assert-AreEqual 1 $service.AdditionalRegions[$i].Capacity
            Assert-Null $service.AdditionalRegions[$i].VirtualNetwork
        }

        if ($service.AdditionalRegions[$i].Location -eq $region2Location)
        {
            $found = $found + 1
            Assert-AreEqual $region2Sku $service.AdditionalRegions[$i].Sku
            Assert-AreEqual $region2Capacity $service.AdditionalRegions[$i].Capacity
            Assert-Null $service.AdditionalRegions[$i].VirtualNetwork
        }
    }
    #Assert-True {$found -eq 2} "Api Management regions created earlier is not found."
    Assert-True {$found -eq 1} "Api Management regions created earlier is not found."

    # Remove the service
    Remove-AzureRMApiManagement -ResourceGroupName $resourceGroupName -Name $apiManagementName -Force

    # Remove resource group
    Remove-AzureRMResourceGroup -Name $resourceGroupName -Force
}

<#
.SYNOPSIS
Tests UpdateAzureApiManagementDeployment using pipline and helper cmdlets.
#>
function Test-UpdateApiManagementDeploymentWithHelpersAndPipline
{
    # Setup
    $location = Get-ProviderLocation "Microsoft.ApiManagement/service"

    # Create resource group
    $resourceGroupName = Get-ResourceGroupName
    New-AzureRMResourceGroup -Name $resourceGroupName -Location $location -Force

    $apiManagementName = Get-ApiManagementServiceName
    $organization = "apimpowershellorg"
    $adminEmail = "apim@powershell.org"
    $sku = "Developer"
    $capacity = 1

    # Create API Management service
    $service = New-AzureRMApiManagement -ResourceGroupName $resourceGroupName -Location $location -Name $apiManagementName -Organization $organization -AdminEmail $adminEmail -Sku $sku -Capacity $capacity

    # Get API Management and:
    #- 1) Scale master region to 'Premium' 2
    $sku = "Premium"
    $capacity = 2

    # - 2) Add new 'Premium' region 1 unit
    $region1Location = Get-ProviderLocations "Microsoft.ApiManagement/service" | Where {$_ -ne $location} | Select -First 1
    $region1Sku = "Premium"

    ## - 3) Add new 'Premium' region 3 units
    #$region2Location = Get-ProviderLocations "Microsoft.ApiManagement/service" | Where {($_ -ne $location) -and ($_ -ne $region1Location)} | Select -First 1
    #$region2Sku = "Premium"
    #$region2Capacity = 3

    Get-AzureRMApiManagement -ResourceGroupName $resourceGroupName -Name $apiManagementName |
    Update-AzureRMApiManagementRegion -Sku $sku -Capacity $capacity |
    Add-AzureRMApiManagementRegion -Location $region1Location -Sku $region1Sku |
    #Add-AzureRMApiManagementRegion -Location $region2Location -Sku $region2Sku -Capacity $region2Capacity |
    Update-AzureRMApiManagementDeployment

    $service = Get-AzureRMApiManagement -ResourceGroupName $resourceGroupName -Name $apiManagementName

    Assert-AreEqual $resourceGroupName $service.ResourceGroupName
    Assert-AreEqual $apiManagementName $service.Name
    Assert-AreEqual $location $service.Location
    Assert-AreEqual $sku $service.Sku
    Assert-AreEqual $capacity $service.Capacity
    Assert-AreEqual "Succeeded" $service.ProvisioningState

    #Assert-AreEqual 2 $service.AdditionalRegions.Count
    Assert-AreEqual 1 $service.AdditionalRegions.Count

    $found = 0
    for ($i = 0; $i -lt $service.AdditionalRegions.Count; $i++)
    {
        if ($service.AdditionalRegions[$i].Location -eq $region1Location)
        {
            $found = $found + 1
            Assert-AreEqual $region1Sku $service.AdditionalRegions[$i].Sku
            Assert-AreEqual 1 $service.AdditionalRegions[$i].Capacity
            Assert-Null $service.AdditionalRegions[$i].VirtualNetwork
        }

        #if ($service.AdditionalRegions[$i].Location -eq $region2Location)
        #{
        #    $found = $found + 1
        #    Assert-AreEqual $region2Sku $service.AdditionalRegions[$i].Sku
        #    Assert-AreEqual $region2Capacity $service.AdditionalRegions[$i].Capacity
        #    Assert-Null $service.AdditionalRegions[$i].VirtualNetwork
        #}
    }
    #Assert-True {$found -eq 2} "Api Management regions created earlier is not found."
    Assert-True {$found -eq 1} "Api Management regions created earlier is not found."


    # Remove the service
    Remove-AzureRMApiManagement -ResourceGroupName $resourceGroupName -Name $apiManagementName -Force

    # Remove resource group
    Remove-AzureRMResourceGroup -Name $resourceGroupName -Force
}

<#
.SYNOPSIS
Tests ImportApiManagementHostnameCertificate.
#>
function Test-ImportApiManagementHostnameCertificate
{
    $certFilePath = ".\testcertificate.pfx";
    $certPassword = "powershelltest";

    # Setup
    $location = Get-ProviderLocation "Microsoft.ApiManagement/service"

    # Create resource group
    $resourceGroupName = Get-ResourceGroupName
    New-AzureRMResourceGroup -Name $resourceGroupName -Location $location -Force

    $apiManagementName = Get-ApiManagementServiceName
    $organization = "apimpowershellorg"
    $adminEmail = "apim@powershell.org"
    $sku = "Developer"
    $capacity = 1

    # Create API Management service
    $result = New-AzureRMApiManagement -ResourceGroupName $resourceGroupName -Location $location -Name $apiManagementName -Organization $organization -AdminEmail $adminEmail -Sku $sku -Capacity $capacity |
    Get-AzureRMApiManagement |
    Import-AzureRMApiManagementHostnameCertificate -HostnameType "Proxy" -PfxPath $certFilePath -PfxPassword $certPassword -PassThru

    Assert-AreEqual "CN=ailn.redmond.corp.microsoft.com" $result.Subject
    Assert-AreEqual "51A702569BADEDB90A75141B070F2D4B5DDFA447" $result.Thumbprint

    # Remove the service
    Remove-AzureRMApiManagement -ResourceGroupName $resourceGroupName -Name $apiManagementName -Force

    # Remove resource group
    Remove-AzureRMResourceGroup -Name $resourceGroupName -Force
}

<#
.SYNOPSIS
Tests SetApiManagmentVirtualNetworks.
#>
function Test-SetApiManagementVirtualNetworks
{
    #Setup
    $location = Get-ProviderLocation "Microsoft.ApiManagement/service"

    # Create resource group
    $resourceGroupName = Get-ResourceGroupName
    New-AzureRMResourceGroup -Name $resourceGroupName -Location $location -Force

    $apiManagementName = Get-ApiManagementServiceName
    $organization = "apimpowershellorg"
    $adminEmail = "apim@powershell.org"
    $sku = "Developer"
    $capacity = 1

    # Create API Management service
    New-AzureRMApiManagement -ResourceGroupName $resourceGroupName -Location $location -Name $apiManagementName -Organization $organization -AdminEmail $adminEmail -Sku $sku -Capacity $capacity

    $vnetLocation = "East US"
    $vnetId = "53F96AC5-9F46-46CE-BA0F-77DE89943258"
    $subnetName = "Subnet-1"

    $networksList = @()
    $networksList += New-AzureRMApiManagementVirtualNetwork -Location $vnetLocation -VnetId $vnetId -SubnetName $subnetName

    try
    {
        try
        {
            Set-AzureRMApiManagementVirtualNetworks -ResourceGroupName $resourceGroupName -Name $apiManagementName -VirtualNetworks $networksList
        }
        catch
        {
            Assert-True {$_.Exception.Message.Contains("InvalidOperation: Configure VPN capability is not supported for Sku Type 'Developer'")}
        }
    }
    finally
    {
        # Remove the service
        Remove-AzureRMApiManagement -ResourceGroupName $resourceGroupName -Name $apiManagementName -Force

        # Remove resource group
        Remove-AzureRMResourceGroup -Name $resourceGroupName -Force
    }
}