import * as pulumi from '@pulumi/pulumi';
import * as resources from '@pulumi/azure-native/resources';
import * as storage from '@pulumi/azure-native/storage';

export function signedBlobReadUrl(
  blob: storage.Blob,
  container: storage.BlobContainer,
  account: storage.StorageAccount,
  resourceGroup: resources.ResourceGroup
): pulumi.Output<string> {
  const blobSAS = pulumi
    .all<string>([blob.name, container.name, account.name, resourceGroup.name])
    .apply((args) =>
      storage.listStorageAccountServiceSAS({
        accountName: args[2],
        protocols: storage.HttpProtocol.Https,
        sharedAccessExpiryTime: '2030-01-01',
        sharedAccessStartTime: '2021-01-01',
        resourceGroupName: args[3],
        resource: storage.SignedResource.C,
        permissions: storage.Permissions.R,
        canonicalizedResource: '/blob/' + args[2] + '/' + args[1],
        contentType: 'application/json',
        cacheControl: 'max-age=5',
        contentDisposition: 'inline',
        contentEncoding: 'deflate',
      })
    );

  return pulumi.interpolate`https://${account.name}.blob.core.windows.net/${container.name}/${blob.name}?${blobSAS.serviceSasToken}`;
}
