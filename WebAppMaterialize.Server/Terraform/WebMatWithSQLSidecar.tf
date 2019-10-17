variable "region" {
  default = "Canada Central"
}

provider "azurerm" {
  # whilst the `version` attribute is optional, we recommend pinning to a given version of the Provider
  version = "=1.21.0"
}

resource "azurerm_virtual_network" "DataTools-Network" {
  name                = "DataTools-Network"
  resource_group_name = "${azurerm_resource_group.DataTools.name}"
  location            = "${azurerm_resource_group.DataTools.location}"
  address_space       = ["10.0.0.0/16"]
}

resource "azurerm_resource_group" "DataTools" {
  name     = "ApptionOrchestration"
  location = "${var.region}"
}

resource "azurerm_subnet" "subnet" {
    name                 = "DataToolsSubnet"
    resource_group_name  = "${azurerm_resource_group.DataTools.name}"
    virtual_network_name = "${azurerm_virtual_network.DataTools-Network.name}"
    address_prefix       = "10.0.2.0/24"
}

resource "azurerm_network_security_group" "networkSecurityGroup" {
    name                = "DataTools-NetworkSecurityGroup"
    location            = "${var.region}"
    resource_group_name = "${azurerm_resource_group.DataTools.name}"

    security_rule {
        name                       = "SSH"
        priority                   = 1001
        direction                  = "Inbound"
        access                     = "Allow"
        protocol                   = "Tcp"
        source_port_range          = "*"
        destination_port_range     = "22"
        source_address_prefix      = "*"
        destination_address_prefix = "*"
    }
	security_rule {
        name                       = "HTTP"
        priority                   = 200
        direction                  = "Inbound"
        access                     = "Allow"
        protocol                   = "Tcp"
        source_port_range          = "*"
        destination_port_range     = "80"
        source_address_prefix      = "Internet"
        destination_address_prefix = "*"
    }

    tags {
        environment = "Terraform Demo"
    }
}

resource "azurerm_public_ip" "publicip" {
    name                 = "datatools"
    domain_name_label    = "datatools"
    location             = "${var.region}"
    resource_group_name  = "${azurerm_resource_group.DataTools.name}"
    allocation_method    = "Dynamic"

    tags {
        environment = "Terraform Demo"
    }
}

resource "azurerm_network_interface" "nic" {
    name                = "DataTools-NIC"
    location            = "${var.region}"
    resource_group_name = "${azurerm_resource_group.DataTools.name}"
    network_security_group_id = "${azurerm_network_security_group.networkSecurityGroup.id}"

    ip_configuration {
        name                          = "DataTools-NicConfiguration"
        subnet_id                     = "${azurerm_subnet.subnet.id}"
        private_ip_address_allocation = "dynamic"
        public_ip_address_id          = "${azurerm_public_ip.publicip.id}"
    }

    tags {
        environment = "Terraform Demo"
    }
}

resource "random_id" "randomId" {
    keepers = {
        # Generate a new ID only when a new resource group is defined
        resource_group = "${azurerm_resource_group.DataTools.name}"
    }

    byte_length = 8
}

resource "azurerm_storage_account" "storageaccount" {
    name                = "diag${random_id.randomId.hex}"
    resource_group_name = "${azurerm_resource_group.DataTools.name}"
    location            = "${var.region}"
    account_replication_type = "LRS"
    account_tier = "Standard"

    tags {
        environment = "Terraform Demo"
    }
}

data "template_file" "cloudconfig" {
  template = "${file("./cloud-init.txt")}"
}

#https://www.terraform.io/docs/providers/template/d/cloudinit_config.html
data "template_cloudinit_config" "config" {
  gzip          = true
  base64_encode = true

  part {
    content_type = "text/cloud-config"
    content = "${data.template_file.cloudconfig.rendered}"
  }
}

resource "azurerm_virtual_machine" "vm" {
    name                  = "DataToolsVM"
    location              = "${var.region}"
    resource_group_name   = "${azurerm_resource_group.DataTools.name}"
    network_interface_ids = ["${azurerm_network_interface.nic.id}"]
    vm_size               = "Standard_B1ms"

    storage_os_disk {
        name              = "OsDisk"
        caching           = "ReadWrite"
        create_option     = "FromImage"
        managed_disk_type = "Standard_LRS"
    }

    storage_image_reference {
        publisher = "Canonical"
        offer     = "UbuntuServer"
        sku       = "18.04-LTS"
        version   = "latest"
    }

    os_profile {
        computer_name  = "DataTools"
        admin_username = "azureuser"
        custom_data  = "${data.template_cloudinit_config.config.rendered}" 
    }

    os_profile_linux_config {
        disable_password_authentication = true
        ssh_keys {
            path     = "/home/azureuser/.ssh/authorized_keys"
            key_data = "ssh-rsa AAAAB3NzaC1yc2EAAAABJQAAAgEAhInILcnLmacU6a+FnmC4E8lgSmL03IjfjA4OBgbrRGzz79XJ6JytoiaaqByol17qO+8J5nJ2k7rg5kXuucNd7643M4Hbzeym56eyP9iGhszhZSy2/8g4XvK7/V4xtL49hwLz7sFCwqiPUErKrxcP4OlBbscwRLDte75wYy9E6osYKtxr7xOqfvF6cLq6jxAulMXBD3HBhGYryuKfM3MicWyAkx9U7nsaLJbIkR4Q0OCN7Wnwvozgb99ved90cemlMjfpLWwP+jfyGobt1ktQYuRH/okBqxXrx8JQvWIOJaRACdns5G1LxaI1vvoLvhYrfHJqtRKwkIpvNDdwDzkCSrD7LIhDgIyKMucisHLx/cX6qJco/d45+Ijs53j0+E4I0+tZ0qOsfBSccmwxovvv363Po63f77wxZRT7hQwIBFVl7JvwcPj+G9J1+zXCzJIqPQpxqwLpr5OYLd/Pc0BF9a5POuErk6LCKZXzkTrRm0kyiyJPy1Y5Tzkf1ZBQTY1lYBwQ3IsU0KfdD54iB2n+JAvnJA9zT2uGGt09caKWtYbHD8ZYpQ7uOpupG3sJQEII2pPyeLkD1aspCEnAxVrjaN3cVL2f2tGys2OqQB6Tmciz6KaYfD3zOFmi3lfUf5YoFuQX7i+35FdUp3mNc/m0mpsh7dsPTzh7UHd7bW1zN1s= rsa-key-20190206"
        } 
        # passphrase 'DataTools@pption'
        # Note: ssh key MUST be 4096 bits
    }

    boot_diagnostics {
        enabled     = "true"
        storage_uri = "${azurerm_storage_account.storageaccount.primary_blob_endpoint}"
    }

    tags {
        environment = "Terraform Demo"
    }
}


