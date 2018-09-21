# Introduction 
OfflineQueueExport is a small console project that exports your queues/journals from your offline database.

# Getting Started
Build the OfflineQueueExport project and use the resulting OfflineQueueExportRKSVEF.exe/OfflineQueueExportRKSVSQLite.exe. Start the console with your command line parameters. The console will notify you on the status and success of an export.

# Build and Test
Build the Programm.cs to generate the exe.  
There are no tests for this project.

# Command Line Parameters
## OfflineQueueExportEFRKSV 

| Parameter 			| Mandatory | Type		| Description 														|
| --------------------- |:---------:|:---------:|-------------------------------------------------------------------|
| connectionstring      | Required  | String	| Connection string to the MS SQL Server database. 					|
| queueid         		| Required	| GUID		| Queue Id to be exported.											|
| cashboxidentification | Required	| String	| CashboxIdentification of the Queue.								|
| encryptionkeybase64   | Required  | String	| EncryptionKeyBase64 of the Queue.									|
| outputfilename        | Required	| String	| Required. Path of the output file for the DEP export.				|
| certificatebase64     | Optional	| String	| Certificate serialized in Base64 of the Signature Creation Unit.	|
| help             		| Optional	| -			| Display the help screen.											|
| version				| Optional	| -			| Display version information.										|

## OfflineQueueExportRKSVSQLite

| Parameter 			| Mandatory | Type		| Description 														|
| --------------------- |:---------:|:---------:|-------------------------------------------------------------------|
| queueid         		| Required	| GUID		| Queue Id to be exported.											|
| servicefolder		    | Required  | String	| Folder containing the SQLite database file.	 					|
| cashboxidentification | Required	| String	| CashboxIdentification of the Queue.								|
| encryptionkeybase64   | Required  | String	| EncryptionKeyBase64 of the Queue.									|
| outputfilename        | Required	| String	| Required. Path of the output file for the DEP export.				|
| certificatebase64     | Optional	| String	| Certificate serialized in Base64 of the Signature Creation Unit.	|
| help             		| Optional	| -			| Display the help screen.											|
| version				| Optional	| -			| Display version information.										|


[//]:#Contribute


[//]:#Informationsources


## Version History
Our usual version format is used, ".{build}" is left out for the moment
{major}.{sprint}.{yyddd}.{build}
https://www.esrl.noaa.gov/gmd/grad/neubrew/Calendar.jsp

## Version 1.23 as of 20180606 = 18157
Improved on command line parameters.

