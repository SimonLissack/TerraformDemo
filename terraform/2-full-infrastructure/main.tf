terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "=3.0.2"
    }
  }

}

provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "rg" {
  name     = "fakss-function-demo"
  location = "northeurope"
}

resource "azurerm_storage_account" "storage" {
  name                     = "fakksstorage"
  resource_group_name      = azurerm_resource_group.rg.name
  location                 = azurerm_resource_group.rg.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_storage_queue" "queue" {
  name                 = "fakksdemo"
  storage_account_name = azurerm_storage_account.storage.name
}

resource "azurerm_service_plan" "plan" {
  name                = "fakks-service-plan"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  os_type             = "Linux"
  sku_name            = "Y1"
}

resource "azurerm_linux_function_app" "func" {
  name                = "fakks-func"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location

  storage_account_name = azurerm_storage_account.storage.name
  service_plan_id      = azurerm_service_plan.plan.id

  identity {
    type = "SystemAssigned"
  }

  site_config {}

  app_settings = {
    "Queue__ServiceName" = azurerm_storage_account.storage.primary_queue_endpoint
    "Queue__Name"        = azurerm_storage_queue.queue.name
  }
}

resource "azurerm_role_assignment" "func_to_queue" {
  scope                = azurerm_storage_account.storage.id
  role_definition_name = "Storage Queue Data Contributor"
  principal_id         = azurerm_linux_function_app.func.identity[0].principal_id
}
