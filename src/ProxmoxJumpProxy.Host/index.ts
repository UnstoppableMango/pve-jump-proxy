import * as pulumi from '@pulumi/pulumi';
import * as web from '@pulumi/azure-native/web';
import * as resources from '@pulumi/azure-native/resources';
import * as signalr from '@pulumi/azure-native/signalrservice';
import * as storage from '@pulumi/azure-native/storage';
import { pascalCase } from 'pascal-case';

const stack = pulumi.getStack();
const stackUpper = pascalCase(stack);
const isProd = stack === 'prod';

const resourceGroup = new resources.ResourceGroup('resourceGroup', {
  resourceGroupName: `ProxmoxJumpProxy${isProd ? '' : `-${stackUpper}`}`,
});

const hubs = new signalr.SignalR('signalr', {
  resourceName: `unmango-proxmox-signalr${isProd ? '' : `-${stack}`}`,
  resourceGroupName: resourceGroup.name,
  sku: {
    name: 'Free_F1',
    capacity: 1,
  },
  kind: signalr.ServiceKind.SignalR,
  features: [
    {
      flag: signalr.FeatureFlags.ServiceMode,
      value: 'Serverless',
    },
  ],
});

const storageAccount = new storage.StorageAccount('functionsa', {
  resourceGroupName: resourceGroup.name,
  kind: storage.Kind.StorageV2,
  sku: {
    name: storage.SkuName.Standard_LRS,
  },
});

const plan = new web.AppServicePlan('functions', {
  name: 'functions-asp',
  resourceGroupName: resourceGroup.name,
  sku: {
    name: 'Y1',
    tier: 'Dynamic',
  },
});

const app = new web.WebApp('functions', {
  name: `unmango-proxmox-functions${isProd ? '' : `-${stack}`}`,
  resourceGroupName: resourceGroup.name,
  serverFarmId: plan.id,
  kind: 'FunctionApp',
});

export const hostname = hubs.hostName;
