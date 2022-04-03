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

variable "prefix" {
  type = string
}

variable "location" {
  type = string
}

resource "azurerm_resource_group" "rg" {
  name     = "${var.prefix}-function-demo"
  location = var.location
}

module "storage_queue" {
  source = "../modules/storage_queue"

  storage_name        = "${var.prefix}storage"
  queue_name          = "${var.prefix}demo"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
}

resource "azurerm_service_plan" "plan" {
  name                = "${var.prefix}-service-plan"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  os_type             = "Linux"
  sku_name            = "Y1"
}

resource "azurerm_linux_function_app" "func" {
  name                = "${var.prefix}-func"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location

  storage_account_name       = module.storage_queue.storage_account_name
  storage_account_access_key = module.storage_queue.storage_key
  service_plan_id            = azurerm_service_plan.plan.id

  identity {
    type = "SystemAssigned"
  }

  site_config {}

  app_settings = {
    "Queue__ServiceUri" = module.storage_queue.queue_endpoint
    "Queue__Name"       = module.storage_queue.queue_name
  }
}

resource "azurerm_role_assignment" "func_to_queue" {
  scope                = module.storage_queue.storage_account_id
  role_definition_name = "Contributor"
  principal_id         = azurerm_linux_function_app.func.identity[0].principal_id
}
