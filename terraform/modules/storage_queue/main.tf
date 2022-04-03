variable "resource_group_name" {
  type = string
}

variable "location" {
  type = string
}

variable "storage_name" {
  type = string
}

variable "queue_name" {
  type = string
}

resource "azurerm_storage_account" "storage" {
  name                     = var.storage_name
  resource_group_name      = var.resource_group_name
  location                 = var.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_storage_queue" "queue" {
  name                 = var.queue_name
  storage_account_name = azurerm_storage_account.storage.name
}

output "storage_account_name" {
  value = azurerm_storage_account.storage.name
}

output "storage_account_id" {
  value = azurerm_storage_account.storage.id
}

output "queue_name" {
  value = azurerm_storage_queue.queue.name
}

output "queue_endpoint" {
  value = azurerm_storage_account.storage.primary_queue_endpoint
}

output "storage_key" {
  value     = azurerm_storage_account.storage.primary_access_key
  sensitive = true
}
