# Preparing for a new release

1. Update the version number in `app.manifest`.
    ```
    <assemblyIdentity version="0.1.0.0" name="Woohoo.ChecksumMatcher.WinUI.app"/>
    ```
1. Update the version number in `Directory.Build.props`.
    ```
    <Version>0.1.0.0</Version>
    <FileVersion>0.1.0.0</FileVersion>
    ```
1. Update the version number in `Package.appxmanifest`.
    ```
    <Identity
      Name="4b35a33b-60e9-4a15-919e-786ce4a6b63e"
      Publisher="CN=hugue"
      Version="0.1.0.0" />
    ```
