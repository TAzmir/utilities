# Introduction 
JWTProofingWithCertificate is a small console project that checks the JWT Certificate of a receipt.

# Getting Started
Build the JWTProofingWithCertificate project and use the resulting JWTProofingWithCertificate.exe. Give the console your CertificatePath and the JWT as parameters and execute. The console will notify on the results.

# Build and Test
Build the Programm.cs to generate the exe. There are no tests for this project.

# Command Line Parameters
## JWTProofingWithCertificate
| Parameter 			| Mandatory | Type		| Description 														|
| --------------------- |:---------:|-----------|-------------------------------------------------------------------|
| certificatepath    	| Required	| String	| Path of the certificate file.										|
| jwt				    | Required  | String	| JWT string to be proofed.	 										|
| help             		| Optional	| -			| Display the help screen.											|
| version				| Optional	| -		 	| Display version information.										|


[//]:#Contribute

[//]:#Informationsources

## Version History
Our usual version format is used, ".{build}" is left out for the moment
{major}.{sprint}.{yyddd}.{build}
https://www.esrl.noaa.gov/gmd/grad/neubrew/Calendar.jsp

## Version 1.23 as of 20180901 = 18244
Improved on command line parameters.

