# Releasing HotConfig

The `nuget-publish.yml` workflow packs `HotConfig` and publishes it to nuget.org through
[Trusted Publishing](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing). No API key is stored in the repository.

## One-time setup (maintainer)

1. Sign in to nuget.org as `emreakdemir`, open **Trusted Publishing** and add a policy:
   - Owner: `emreakdemir`
   - Repository: `emreakdemir/configuration-library`
   - Workflow file: `nuget-publish.yml`
   - Environment: leave empty
2. The package id does not exist yet. nuget.org lets you create the policy before the first publish; the first push then creates the package.

## Release

1. Set `<Version>` in `Directory.Build.props` and merge it to `master`.
2. Create a GitHub release with tag `v<version>` (first release: `v1.0.0`) and publish it.
3. The workflow tests, packs, writes the SBOM, attests the build provenance, attaches the SBOM to the release and pushes the package.
4. After the first release, set `PackageValidationBaselineVersion` in `HotConfig/HotConfig.csproj` to the released version.

Unlist or relist a version: run the workflow manually (`workflow_dispatch`) with `unlist` or `relist` set to `HotConfig:<version>`.

## Verify provenance

Download the `.nupkg` from the workflow run (artifact `nuget`) or from nuget.org, then run:

```
gh attestation verify HotConfig.1.0.0.nupkg --repo emreakdemir/configuration-library
```

The SBOM (`HotConfig.spdx.json`) is attached to the GitHub release.
