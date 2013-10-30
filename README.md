# VirtualPath .NET Library

A simple abstraction layer over file system like APIs

Currently implemented:

- FileSystem (CIFS)
- In Memory (useful for Unit Testing)
- FTP/FTPS via [AlexFTPS](http://ftps.codeplex.com/)
- SFTP via [SshNet](http://sshnet.codeplex.com/)
- Dropbox via [DropNet](https://github.com/dkarzon/DropNet)
- ZipFile via [DotNetZip](http://dotnetzip.codeplex.com)

## Installation

Nuget: install-package VirtualPath

## Usage

Direct instanciation of provider:

	using(var storage = new InMemoryVirtualPathProvider())
	{
		var file = storage.GetFile("/info.txt");
		var info = file.ReadAllText();
	}

Using VirtualPathProvider factory:

	// general usage:
	var storage = VirtualPathProvider.Open(providerName, config);
	
	// Exemples:
	VirtualPathProvider.Open("Memory")
	VirtualPathProvider.Open("FileSystem", "C:\\Temp\\")
	VirtualPathProvider.Open("FTP", "host=ftp.company.com;username=login;password=password")
	VirtualPathProvider.Open("SFTP", "host=ftp.company.com;username=login;password=password")
	VirtualPathProvider.Open("Dropbox", "apikey=;apisecret=;usertoken=;usersecret=")

You can also pass anonymous objects to specify constructor parameters:

	var config = new { host = "ftp.company.com", username = "login", password = "password" };
	VirtualPathProvider.Open("FTP", config);

### Factory behavior:

- If you pass a string: 
	- split it on ';' then build `IDict<string,string>` by splitting on '=', 
	- if no key => wildcard key (works for single parameter constructors like FileSystem)
- If passing an object, build a `IDict<string,object>` from properties

__Finding the constructor:__

 - filter constructors with matching parameter count
 - if one result: return constructor
 - filter remaining constructors on parameter names
 - if one result: return constructor
 - filter remaining on parameter types (including conversions if passed strings)
 - if one result: return constructor

## Available providerNames:

- InMemoryVirtualPathProvider: Memory
- FileSystemVirtualPathProvider: FileSystem, FS
- FtpVirtualPathProvider: AlexFTPS, FTP
- SftpVirtualPathProvider: SshNet, SFTP
- DropboxVirtualPathProvider: DropNet, Dropbox
- ZipVirtualPathProvider: Zip, DotNetZip

## Todo

- FTPS by providerName
- CIFS credentials in FileSystem
- WebDAV
- Cross provider methods
	- CopyFile(string virtualPath, IVirtualFile file)
	- MoveFile(string virtualPath, IVirtualFile file)
	- etc

## Futures (?)

- Directory Move / Copy ?
- Amazon S3
- Google Drive
- Transactional PathProvider (InMemory commiting to FileSystem for example)

## Tanks

- [ServiceStack](http://www.servicestack.net/) for basic implementation of IVirtualPathProvider/FileSystem/InMemory
- [AlexFTPS](http://ftps.codeplex.com/)
- [SshNet](http://sshnet.codeplex.com/)
- [DropNet](https://github.com/dkarzon/DropNet)
- [DotNetZip](http://dotnetzip.codeplex.com)