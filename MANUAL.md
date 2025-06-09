# Checksum Matcher Manual

## Getting started

<picture>
<source srcset="images/home-dark.png?raw=true"  media="(prefers-color-scheme: dark)"/>
<source srcset="images/home-light.png?raw=true" media="(prefers-color-scheme: light)"/>
<img alt="Home" src="images/home-light?raw=true"/>
</picture>

### Setting up your database folders

1. Create a parent folder to store your databases in the location of your
   choice, e.g., `C:\Databases`.
1. Create a subfolder for each type of databases you want to store, e.g.,
   `C:\Databases\NoIntro`, `C:\Databases\Redump`, etc.
1. Navigate to the **Settings** page and find the **Database** section.
1. Click **Add folder** for the **Database folders** setting and add one or more
   folders. You can choose to add the parent folder, e.g. `C:\Databases` OR you
   can add each subfolder individually.

<picture>
<source srcset="images/settings-dark.png?raw=true"  media="(prefers-color-scheme: dark)"/>
<source srcset="images/settings-light.png?raw=true" media="(prefers-color-scheme: light)"/>
<img alt="Settings" src="images/settings-light?raw=true"/>
</picture>

### Downloading No-Intro databases

1. Use the link from the **Home** page to open the No-Intro web site in your browser.
1. Click on **Database**, then **Download**, then **Daily**, optionally
   check/uncheck the sets you would like included and click **Request**.
1. Extract the downloaded zip file into the appropriate folder, e.g.,
   `C:\Databases\NoIntro`.

### Downloading Redump databases (manual)

1. Use the link from the **Home** page to open the Redump web site in your browser.
1. Click on **Downloads**, then click on **Datfile** for each system that you want
   to download.
1. Move the downloaded files (no need to extract them) into the appropriate
   folder, e.g., `C:\Databases\Redump`. If you download other artifacts, such as
   cue files or key files, you can place them in the same folder as the dat files.

### Downloading Redump databases (automated)

1. Navigate to the **Settings** page and find the **Redump.org** section.
1. If you have dumper status on Redump.org, enter your username and password in
   the **Credentials** setting. Click **Test credentials** to validate your input.
   This allows dowloading of private databases.
1. Click on **Select** for the **Artifacts folder** setting to choose the folder
   where you want the databases to be downloaded, e.g., `C:\Databases\Redump`.
1. Expand the **Systems** setting, and select the systems you want to download
   databases for. By default, all systems are selected.
   If you select systems whose databases are private, but have not entered valid
   credentials, they will be skipped automatically.
1. Click **Download** to start the download process. This may take a while.

## Viewing databases

1. Navigate to the **Databases** page.
1. If you've added database folders in the **Settings** page, they will be
   appear in the tree view on the left.
1. If you have a lot of folders or databases, you can use the search box at the
   top of the tree view to filter the databases.
1. Click on a database in the tree view.
1. On the right, you will see the database **Contents** by default.
1. There are 3 ways to filter the contents:
   - **Scan Status Filter**: Scan status is set after a scan operation.
   - **Dump Status Filter**: Dump status is optionally set by the database author.
   - **Query**: Type in the filter box to filter by name, size of checksum.
1. Click on the **Info** button on the top selection bar to view the database
   information, such as the name, author, date created, and more.
   It also includes computed statistics, such as the number of files, and total size.

<picture>
<source srcset="images/database-file-games-filter-dark.png?raw=true"  media="(prefers-color-scheme: dark)"/>
<source srcset="images/database-file-games-filter-light.png?raw=true" media="(prefers-color-scheme: light)"/>
<img alt="Database Contents" src="images/database-file-games-filter-light?raw=true"/>
</picture>

<picture>
<source srcset="images/database-file-metadata-dark.png?raw=true"  media="(prefers-color-scheme: dark)"/>
<source srcset="images/database-file-metadata-light.png?raw=true" media="(prefers-color-scheme: light)"/>
<img alt="Database Info" src="images/database-file-metadata-light?raw=true"/>
</picture>

## Scanning your collection (single database)

1. Navigate to the **Databases** page.
1. Click on a database in the tree view.
1. Click on the **Scan settings** button on the top selection bar.
1. Click **Add folder** for the **Local folders** setting and add one or more folders.
1. Click on the **Scan** button on the top selection bar.
1. Click on the **Scan** button below to start the scan operation.
1. The application will read the contents of the selected folders and compare 
   them to the database. This may take a while.
1. You can view the results by clicking the desired button on the bottom selection bar.
   Choose from **Matched**, **Missing**, **Wrong Name**, **Unused**.
1. You can save the results to the clipboard using the context menu in any of
   the result lists.
1. The scan status is also visible in the **Contents** view. A badge that is right aligned
   indicates the scan status of each container and file.
1. Click on the **Scan status filter** button to filter the contents by scan status.

<picture>
<source srcset="images/database-file-scan-settings-dark.png?raw=true"  media="(prefers-color-scheme: dark)"/>
<source srcset="images/database-file-scan-settings-light.png?raw=true" media="(prefers-color-scheme: light)"/>
<img alt="Scan Settings" src="images/database-file-scan-settings-light?raw=true"/>
</picture>

<picture>
<source srcset="images/database-file-scan-results-dark.png?raw=true"  media="(prefers-color-scheme: dark)"/>
<source srcset="images/database-file-scan-results-light.png?raw=true" media="(prefers-color-scheme: light)"/>
<img alt="Scan" src="images/database-file-scan-results-light?raw=true"/>
</picture>

<picture>
<source srcset="images/database-file-scan-games-dark.png?raw=true"  media="(prefers-color-scheme: dark)"/>
<source srcset="images/database-file-scan-games-light.png?raw=true" media="(prefers-color-scheme: light)"/>
<img alt="Scan Results" src="images/database-file-scan-games-light?raw=true"/>
</picture>

<picture>
<source srcset="images/database-file-scan-games-missing-dark.png?raw=true"  media="(prefers-color-scheme: dark)"/>
<source srcset="images/database-file-scan-games-missing-light.png?raw=true" media="(prefers-color-scheme: light)"/>
<img alt="Scan Filter" src="images/database-file-scan-games-missing-light?raw=true"/>
</picture>

## Scanning your collection (multiple databases)

1. Navigate to the **Databases** page.
1. Click on a folder in the tree view that contains multiple databases.
1. Click on the **Scan settings** button on the top selection bar.
1. You can optionally add one or more folders to the **Local folders** setting.
   Do this if you follow a specific folder structure where your files are stored
   under a subfolder named after the database **name**.
   This is usually different from the database file name, see the **Info** view
   for each database to determine its name.
   For every database, the application will compute the final path by combining
   the folder you have specified with the database name, for example:
   If you add `D:\Roms\Redump` as a local folder and the database
   name is `Acorn - Archimedes`, the final path that will be scanned is
   `D:\Roms\Redump\Acorn - Archimedes`.
1. If you do not follow that specific folder structure, you can still can multiple
   databases at once, but you will need to configure the scan settings for each
   database individually.
1. When you are ready, click on the **Scan** button on the top selection bar,
   then click on the **Scan** button below to start the scan operation.

<picture>
<source srcset="images/database-folder-scan-settings-dark.png?raw=true"  media="(prefers-color-scheme: dark)"/>
<source srcset="images/database-folder-scan-settings-light.png?raw=true" media="(prefers-color-scheme: light)"/>
<img alt="Scan Settings" src="images/database-folder-scan-settings-light?raw=true"/>
</picture>

<picture>
<source srcset="images/database-folder-scan-dark.png?raw=true"  media="(prefers-color-scheme: dark)"/>
<source srcset="images/database-folder-scan-light.png?raw=true" media="(prefers-color-scheme: light)"/>
<img alt="Scan Multiple Databases" src="images/database-folder-scan-light?raw=true"/>
</picture>

## Rebuilding your collection

When you have a folder of files that are unsorted, you can rebuild them to a
target folder using the database information to sort them into subfolders or
archives.

1. Navigate to the **Databases** page.
1. Click on a database in the tree view.
1. Click on the **Rebuild settings** button on the top selection bar.
1. Click **Select** for the **Source folder** setting and pick the folder that
   contains unsorted files.
1. Click **Select** for the **Target folder** setting and pick the folder where
   you want the sorted files to be placed.
1. Click on the dropdown for the **Target container type** setting and choose
   the type of container you want to create. You can choose from:
   - **Folder**: Sort files into subfolders.
   - **Zip**: Create a zip archive.
   - **Uncompressed 7z**: Create an uncompressed 7z archive.
   - **Torrentzip**: Create a torrent zip archive.
   - **Torrent7z**: Create a torrent 7z archive.
1. Optionally, you can enable **Remove matched files** to delete the source
   files that match the database and are successfully copied to the target
   folder. Note that this is a destructive operation and cannot be undone.
   It is also only supported for the **Folder** and **Zip** source containers.
1. Click on the **Rebuild** button on the top selection bar.
1. Click on the **Rebuild** button below to start the scan operation.
1. The application will read the contents of the selected folders and compare 
   them to the database. Files that match the database will be rebuilt to
   into the desired target container type. This may take a while.
1. You can view the results by clicking the desired button on the bottom selection bar.
   Choose from **Matched** and **Unused**.

<picture>
<source srcset="images/database-file-rebuild-settings-dark.png?raw=true"  media="(prefers-color-scheme: dark)"/>
<source srcset="images/database-file-rebuild-settings-light.png?raw=true" media="(prefers-color-scheme: light)"/>
<img alt="Rebuild Settings" src="images/database-file-rebuild-settings-light?raw=true"/>
</picture>

<picture>
<source srcset="images/database-file-rebuild-dark.png?raw=true"  media="(prefers-color-scheme: dark)"/>
<source srcset="images/database-file-rebuild-light.png?raw=true" media="(prefers-color-scheme: light)"/>
<img alt="Rebuild" src="images/database-file-rebuild-light?raw=true"/>
</picture>

## Creating your own database

If you have a collection of files that you want to manage, you can create
your own database to use with Checksum Matcher.

1. Navigate to the **Create Database** page.
1. Select the **Source folder** where your files are located.
1. Select the **Database destination file** where you want to save
   the database file. This is a `.dat` file in XML format.
1. Enter database metadata such as the name, author, version and url.
1. Click on the **Create** button to start the database creation process.

<picture>
<source srcset="images/database-create-dark.png?raw=true"  media="(prefers-color-scheme: dark)"/>
<source srcset="images/database-create-light.png?raw=true" media="(prefers-color-scheme: light)"/>
<img alt="Create Database" src="images/database-create-light?raw=true"/>
</picture>

## Dealing with external drives

If you have a large collection of files on external drives, you can save
a snapshot of the directory contents of each drive to an offline disk.

This allows you to explore your drives without needing to connect them.

You can also scan the offline disk to find missing, incorrect or unused files.

1. Navigate to the **Settings** page and find the **Offline storage** section.
1. Click **Add folder** for the **Offline storage folders** setting and add one
   or more folders.

### Creating an offline disk

1. Navigate to the **Create Offline Storage** page.
1. From the **Source folder** dropdown, select the drive you want to create
   an offline disk for.
1. By default, the **Target folder** is set to the first **Offline storage folders**
   setting from the **Settings** page. You can change it to a different folder
   if you want.
1. Enter a name for the offline disk in the **Disk name** field. This will be used
   to create a file in the target folder.

<picture>
<source srcset="images/offline-explorer-create-dark.png?raw=true"  media="(prefers-color-scheme: dark)"/>
<source srcset="images/offline-explorer-create-light.png?raw=true" media="(prefers-color-scheme: light)"/>
<img alt="Create Offline Storage" src="images/offline-explorer-create-light?raw=true"/>
</picture>

### Viewing an offline disk

1. Navigate to the **Offline Storage Explorer** page.
1. From the dropdown at the top, select the offline disk you want to view.
1. Use the tree view on the left and the list on the right to navigate through
   the files and folders in the offline disk.
1. You can use the search box at the top to filter the files and folders
   by name, size or checksum. The search is recursive from the current folder.

<picture>
<source srcset="images/offline-explorer-browse-dark.png?raw=true"  media="(prefers-color-scheme: dark)"/>
<source srcset="images/offline-explorer-browse-light.png?raw=true" media="(prefers-color-scheme: light)"/>
<img alt="Browse Offline Storage" src="images/offline-explorer-browse-light?raw=true"/>
</picture>

<picture>
<source srcset="images/offline-explorer-search-dark.png?raw=true"  media="(prefers-color-scheme: dark)"/>
<source srcset="images/offline-explorer-search-light.png?raw=true" media="(prefers-color-scheme: light)"/>
<img alt="Search Offline Storage" src="images/offline-explorer-search-light?raw=true"/>
</picture>

### Scanning your collection (offline disk)

If your collection is stored in archive files, the offline disk will
contain the checksum information for the files inside the archives.

This allows scanning your collection without needing to connect the
external drives.

1. Navigate to the **Databases** page.
1. Click on a database in the tree view.
1. Click on the **Scan settings** button on the top selection bar.
1. Click **Add folder** for the **Offline folders** setting.
1. Select from the **Offline disk** dropdown.
1. Select from the **Offline folder** dropdown.
1. Click OK to add the folder.
1. You can add as many offline folders as you want, which is useful
   if your collection is spread across multiple offline disks.
1. Click on the **Scan** button on the top selection bar.

You can also scan multiple databases at once by selecting a folder
from the database tree view, and configure the scan settings for it
similarly to how you would do it for regular local folders.

## File hash calculator

You can use the built-in file hash calculator to compute the checksum of
any file on your system.

1. Navigate to the **Hash Calculator** page.
1. Either drag and drop a file onto the page, or click the **Select File**
   button to choose a file from your system.
1. The application will compute the checksum of the file and display it in
   a table below.
1. You can copy the full results for the file to the clipboard by clicking
   the **Copy** button next to the file path.
