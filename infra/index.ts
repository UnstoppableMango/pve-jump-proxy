import * as pulumi from '@pulumi/pulumi';
import * as web from '@pulumi/azure-native/web';
import * as resources from '@pulumi/azure-native/resources';
import * as signalr from '@pulumi/azure-native/signalrservice';
import * as storage from '@pulumi/azure-native/storage';
import * as fs from 'fs';
import * as path from 'path';
import { pascalCase } from 'pascal-case';
import { signedBlobReadUrl } from './helpers';

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

const container = new storage.BlobContainer('container', {
  accountName: storageAccount.name,
  resourceGroupName: resourceGroup.name,
  publicAccess: storage.PublicAccess.None,
});

const functionDirectory = path.join(__dirname, '..', 'src', 'ProxmoxJumpProxy.Host');
const publishDirectory = path.join(functionDirectory, 'bin', 'Debug', 'net6.0', 'publish');
if (!fs.existsSync(publishDirectory)) {
  console.log(publishDirectory);
  throw new Error("Function app hasn't been built");
}

const dotnetBlob = new storage.Blob('dotnetBlob', {
  resourceGroupName: resourceGroup.name,
  accountName: storageAccount.name,
  containerName: container.name,
  source: new pulumi.asset.FileArchive(publishDirectory),
});

const plan = new web.AppServicePlan('functions', {
  name: 'functions-asp',
  resourceGroupName: resourceGroup.name,
  sku: {
    name: 'Y1',
    tier: 'Dynamic',
  },
});

const dotnetBlobSignedURL = signedBlobReadUrl(dotnetBlob, container, storageAccount, resourceGroup);

const app = new web.WebApp('functions', {
  name: `unmango-proxmox-functions${isProd ? '' : `-${stack}`}`,
  resourceGroupName: resourceGroup.name,
  serverFarmId: plan.id,
  kind: 'FunctionApp',
  siteConfig: {
    appSettings: [
      { name: 'runtime', value: 'dotnet' },
      { name: 'FUNCTIONS_WORKER_RUNTIME', value: 'dotnet' },
      { name: 'WEBSITE_RUN_FROM_PACKAGE', value: dotnetBlobSignedURL },
      { name: 'FUNCTIONS_EXTENSION_VERSION', value: '~3' },
    ],
  },
});

export const hubhost = hubs.hostName;
