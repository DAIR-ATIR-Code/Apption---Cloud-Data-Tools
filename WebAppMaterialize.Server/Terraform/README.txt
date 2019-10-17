#
# Instructions for Terraform infrastructure
#
Install Azure CLI  https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest
Install Terraform  https://www.terraform.io/downloads.html
Open Powershell, switch to WebAppMaterialServer/Terraform   (cd "c:\...\Repos\Cloud Data Tools\DataTools\WebAppMaterialize.Server\Terraform")
Execute the Powershell Commands:

az login  (Takes you to a browser page)
az account set --subscription "3b6f5be2-5fcb-4ad7-b202-4b556cb0de98"  (If you need to charge to a different subscription)
terraform init
terraform apply
  (enter 'yes' at prompt)

Once Terraform infrastructure completes, wait 5 minutes until Terraform first-time application installater completes

Visit:   http://datatools.canadacentral.cloudapp.azure.com/

To connect to SQL Server sidecar, use the conneciton string:

Server=10.0.2.4;uid=sa;pwd=@ppt10n d0cker 1m@ge

Note: 10.0.2.4 is the sub-net IP address of the Azure Linux VM.
It is usually 10.0.2.4, but if this fails:

Connect to datatools.canadacentral.cloudapp.azure.com with an SSH client
  user = azureuser
  Pass Phrase: @ppt10n d0cker 1m@ge

  Then use 'ifconfig -a | grep inet' to get list of IP addresses. 
	Local loopback 127.0.0.1 or localhost will not find the SQL Server container
	'172.17.0.1' is used for Azure provisioning and will not find the SQL Server container
	Use the the remaining IP

Use following powershell command to release all Azure resources

terraform destroy
   (enter 'yes' at prompt)

